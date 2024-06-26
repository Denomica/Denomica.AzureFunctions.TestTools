﻿using Azure.Data.Tables;
using Denomica.AzureFunctions.TestTools.Core;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace Denomica.AzureFunctions.TestTools.InProcess
{
    /// <summary>
    /// A mocker class that is used to mock <see cref="IDurableOrchestrationContext"/> instances for effective testing.
    /// </summary>
    public class OrchestrationContextMocker
    {

        /// <summary>
        /// Creates a new instance of the mocker class and specifies the services to build the mocker from.
        /// </summary>
        /// <param name="services">The services that contain dependencies for the classes declaring orchestration, activity and entity functions.</param>
        /// <param name="behavior">The behavior to use in the mocker.</param>
        /// <exception cref="ArgumentNullException">The exception that is thrown if <paramref name="services"/> is <c>null</c>.</exception>
        public OrchestrationContextMocker(IServiceCollection? services = null, MockBehavior behavior = MockBehavior.Strict)
        {
            this.Services = services ?? new ServiceCollection();
            this._behavior = behavior;
        }

        private readonly MockBehavior _behavior;

        private Dictionary<string, Action> MockSetups = new Dictionary<string, Action>();

        private Func<EntityId, string, bool> EntityIdMatcher = (id, name) =>
        {
            return id.EntityName.ToLower() == name.ToLower();
        };


        /// <summary>
        /// Returns the service collection the mocker is configured in.
        /// </summary>
        public IServiceCollection Services { get; private set; }

        #region Orchestration Context Properties

        private Dictionary<string, DateTime> CurrentUtcDateTimes = new Dictionary<string, DateTime>();
        /// <summary>
        /// Adds a default mock that returns a value from the <see cref="IDurableOrchestrationContext.CurrentUtcDateTime"/> property.
        /// </summary>
        /// <remarks>
        /// The default implementation returns a unique value for each unique callstack from where the 
        /// <see cref="IDurableOrchestrationContext.CurrentUtcDateTime"/> property is called in your code.
        /// </remarks>
        public OrchestrationContextMocker AddCurrentUtcDateTime()
        {
            _orchestrationMock.SetupGet<DateTime>(x => x.CurrentUtcDateTime)
                .Returns(() =>
                {
                    var stack = new System.Diagnostics.StackTrace();
                    var frames = stack.GetFrames();
                    var ix = frames.ToList().FindIndex(x => x.GetMethod().Name == "get_CurrentUtcDateTime");
                    var str = string.Join(string.Empty, frames.Skip(ix + 1).ToList());
                    var id = str.Hash();

                    if(!this.CurrentUtcDateTimes.ContainsKey(id))
                    {
                        this.CurrentUtcDateTimes[id] = DateTime.UtcNow;
                    }

                    return this.CurrentUtcDateTimes[id];
                });
            return this;
        }

        /// <summary>
        /// Adds a <see cref="DateTime"/> value that will be returned by the <see cref="IDurableOrchestrationContext.CurrentUtcDateTime"/> property.
        /// </summary>
        /// <param name="currentUtcDateTime">The date time to return.</param>
        public OrchestrationContextMocker AddCurrentUtcDateTime(DateTime currentUtcDateTime)
        {
            _orchestrationMock.SetupGet<DateTime>(x => x.CurrentUtcDateTime)
                .Returns(currentUtcDateTime.ToUniversalTime());

            return this;
        }

        /// <summary>
        /// Adds a delegate that returns a <see cref="DateTime"/> value that is returned by the <see cref="IDurableOrchestrationContext.CurrentUtcDateTime"/> property.
        /// </summary>
        /// <param name="dateTimeProvider">A delegate that returns a <see cref="DateTime"/> value.</param>
        public OrchestrationContextMocker AddCurrentUtcDateTime(Func<DateTime> dateTimeProvider)
        {
            _orchestrationMock.SetupGet<DateTime>(x => x.CurrentUtcDateTime)
                .Returns(dateTimeProvider.Invoke().ToUniversalTime());

            return this;
        }
        #endregion

        #region Activities

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <see cref="IDurableActivityContext"/> as input and does not return any value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass>(Expression<Func<TClass, Func<IDurableActivityContext, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableActivityContext, Task> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { input }) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <see cref="IDurableActivityContext"/> as input and does not return a value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass>(Expression<Func<TClass, Func<IDurableActivityContext, Task>>> expression, Func<IDurableActivityContext, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallActivityAsync(name, It.IsAny<IDurableActivityContext>()))
                    .Returns((string fn, IDurableActivityContext input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<IDurableActivityContext>()))
                    .Returns((string fn, RetryOptions ro, IDurableActivityContext input) => callback(input));
            };

            return this;
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <see cref="IDurableActivityContext"/> as input and returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TResult">The type of the return value from the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass, TResult>(Expression<Func<TClass, Func<IDurableActivityContext, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableActivityContext, Task<TResult>> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { input }) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <see cref="IDurableActivityContext"/> as input and returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TResult">The type of the return value from the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass, TResult>(Expression<Func<TClass, Func<IDurableActivityContext, Task<TResult>>>> expression, Func<IDurableActivityContext, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallActivityAsync<TResult>(name, It.IsAny<IDurableActivityContext>()))
                    .Returns((string fn, IDurableActivityContext input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<IDurableActivityContext>()))
                    .Returns((string fn, RetryOptions ro, IDurableActivityContext input) => callback(input));

                //----------------------------------------------------------------------------------------------------------------
                // Add also the methods that don't specify the result type, because activity functions can be
                // called without handling the return value.
                moc.Setup(x => x.CallActivityAsync(name, It.IsAny<object?>()))
                    .Returns((string fn, IDurableActivityContext input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<object?>()))
                    .Returns((string fn, RetryOptions ro, IDurableActivityContext input) => callback(input));
                //----------------------------------------------------------------------------------------------------------------
            };

            return this;
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <typeparamref name="TInput"/> as input and does not return a value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter to the activity function. The input parameter is decorated with the <see cref="ActivityTriggerAttribute"/> attribute.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass, TInput>(Expression<Func<TClass, Func<TInput, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<TInput, Task> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { input }) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <typeparamref name="TInput"/> as input and does not return a value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter to the activity function. The input parameter is decorated with the <see cref="ActivityTriggerAttribute"/> attribute.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual activity function.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public OrchestrationContextMocker AddActivityFunction<TClass, TInput>(Expression<Func<TClass, Func<TInput, Task>>> expression, Func<TInput, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallActivityAsync(name, It.IsAny<TInput>()))
                    .Returns((string fn, TInput input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<TInput>()))
                    .Returns((string fn, RetryOptions ro, TInput input) => callback(input));
            };

            return this;
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <typeparamref name="TInput"/> as input and returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter to the activity function. The input parameter is decorated with the <see cref="ActivityTriggerAttribute"/> attribute.</typeparam>
        /// <typeparam name="TResult">The type of the return value from the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass, TInput, TResult>(Expression<Func<TClass, Func<TInput, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<TInput, Task<TResult>> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { input }) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an activity function to the mocker. The function takes a <typeparamref name="TInput"/> as input and returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the activity function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter to the activity function. The input parameter is decorated with the <see cref="ActivityTriggerAttribute"/> attribute.</typeparam>
        /// <typeparam name="TResult">The type of the return value from the activity function.</typeparam>
        /// <param name="expression">An expression representing the activity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual activity function.</param>
        public OrchestrationContextMocker AddActivityFunction<TClass, TInput, TResult>(Expression<Func<TClass, Func<TInput, Task<TResult>>>> expression, Func<TInput, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallActivityAsync<TResult>(name, It.IsAny<TInput>()))
                    .Returns((string fn, TInput input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<TInput>()))
                    .Returns((string fn, RetryOptions ro, TInput input) => callback(input));

                //----------------------------------------------------------------------------------------------------------------
                // Add also the methods that don't specify the result type, because activity functions can be
                // called without handling the return value.
                moc.Setup(x => x.CallActivityAsync(name, It.IsAny<TInput>()))
                    .Returns((string fn, TInput input) => callback(input));

                moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<TInput>()))
                    .Returns((string fn, RetryOptions ro, TInput input) => callback(input));
                //----------------------------------------------------------------------------------------------------------------
            };

            return this;
        }

        #endregion

        #region Orchestrations

        /// <summary>
        /// Adds an orchestration function to the mocker. The function does not return a value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <param name="expression">The expression representing the orchestration function.</param>
        public OrchestrationContextMocker AddOrchestrationFunction<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableOrchestrationContext, Task> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { context }) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddOrchestrationFunction(expression, callback);
        }

        /// <summary>
        /// Adds an orchestration function to the mocker. The function does not return a value.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <param name="expression">The expression representing the orchestration function.</param>
        /// <param name="callback">The callback that will be called instead of the actual orchestration function.</param>
        public OrchestrationContextMocker AddOrchestrationFunction<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> expression, Func<IDurableOrchestrationContext, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallSubOrchestratorAsync(name, It.IsAny<object>()))
                    .Returns((string fn, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorAsync(name, It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((string fn, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<object>()))
                    .Returns((string fn, RetryOptions ro, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((string fn, RetryOptions ro, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));
            };

            return this;
        }

        /// <summary>
        /// Adds an orchestration function to the mocker. The function returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <typeparam name="TResult">The type for the value returned by the orchestration.</typeparam>
        /// <param name="expression">The expression representing the orchestration function.</param>
        public OrchestrationContextMocker AddOrchestrationFunction<TClass, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableOrchestrationContext, Task<TResult>> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, new object?[] { context }) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddOrchestrationFunction(expression, callback);
        }

        /// <summary>
        /// Adds an orchestration function to the mocker. The function returns a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <typeparam name="TResult">The type for the value returned by the orchestration.</typeparam>
        /// <param name="expression">The expression representing the orchestration function.</param>
        /// <param name="callback">The callback that will be called instead of the actual orchestration function.</param>
        public OrchestrationContextMocker AddOrchestrationFunction<TClass, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task<TResult>>>> expression, Func<IDurableOrchestrationContext, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            this.MockSetups[name] = () =>
            {
                moc.Setup(x => x.CallSubOrchestratorAsync<TResult>(name, It.IsAny<object>()))
                    .Returns((string fn, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorAsync<TResult>(name, It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((string fn, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<object>()))
                    .Returns((string fn, RetryOptions ro, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((string fn, RetryOptions ro, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));
            };

            return this;
        }

        #endregion

        #region Entities

        /// <summary>
        /// Adds an entity function to the mocker.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the entity function.</typeparam>
        /// <param name="expression">The expression representing the entity function.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="expression"/> does not represent an entity function.</exception>
        public OrchestrationContextMocker AddEntityFunction<TClass>(Expression<Func<TClass, Action<IDurableEntityContext>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateEntityMethod<TClass>(mi, out var name, out var mock);

            Action<IDurableEntityContext> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                mi.Invoke(svc, new object?[] { context });
            };


            return this.AddEntityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an entity function to the mocker.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the entity function.</typeparam>
        /// <param name="expression">The expression representing the entity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual </param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="expression"/> does not represent an entity function.</exception>
        public OrchestrationContextMocker AddEntityFunction<TClass>(Expression<Func<TClass, Action<IDurableEntityContext>>> expression, Action<IDurableEntityContext> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateEntityMethod<TClass>(mi, out var name, out var mock);

            this.MockSetups[name] = () =>
            {
                mock.Setup(x => x.CallEntityAsync(It.Is<EntityId>(x => this.EntityIdMatcher(x, name)), It.IsAny<string>()))
                    .Returns((EntityId id, string operationName) =>
                    {
                        var eMocker = this.GetRequiredService<EntityContextMocker>();
                        callback(eMocker.GetEntityContext());
                        return Task.CompletedTask;
                    });

                mock.Setup(x => x.CallEntityAsync(It.Is<EntityId>(x => this.EntityIdMatcher(x, name)), It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((EntityId id, string operationName, object input) =>
                    {
                        var eMocker = this.GetRequiredService<EntityContextMocker>();
                        callback(eMocker.GetEntityContext(input));
                        return Task.CompletedTask;
                    });
            };

            return this;
        }

        /// <summary>
        /// Adds an entity function to the mocker.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the entity function.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression">The expression representing the entity function.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="expression"/> does not represent an entity function.</exception>
        public OrchestrationContextMocker AddEntityFunction<TClass, TResult>(Expression<Func<TClass, Action<IDurableEntityContext>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateEntityMethod<TClass>(mi, out var name, out var mock);

            Func<IDurableEntityContext, TResult> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                var result = mi.Invoke(svc, new object?[] { context });
                return result is TResult ? (TResult)result : throw new Exception($"The entity function '{name}' did not return a valid result of type '{typeof(TResult).FullName}'.");
            };

            return this.AddEntityFunction(expression, callback);
        }

        /// <summary>
        /// Adds an entity function to the mocker.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the entity function.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression">The expression representing the entity function.</param>
        /// <param name="callback">The callback that will be called instead of the actual </param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="expression"/> does not represent an entity function.</exception>
        public OrchestrationContextMocker AddEntityFunction<TClass, TResult>(Expression<Func<TClass, Action<IDurableEntityContext>>> expression, Func<IDurableEntityContext, TResult> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateEntityMethod<TClass>(mi, out var name, out var mock);

            this.MockSetups[name] = () =>
            {
                mock.Setup(x => x.CallEntityAsync<TResult>(It.Is<EntityId>(x => this.EntityIdMatcher(x, name)), It.IsAny<string>()))
                    .Returns((EntityId id, string operationName) =>
                    {
                        var eMocker = this.GetRequiredService<EntityContextMocker>();
                        var result = callback(eMocker.GetEntityContext());
                        return Task.FromResult(result);
                    });

                mock.Setup(x => x.CallEntityAsync<TResult>(It.Is<EntityId>(x => this.EntityIdMatcher(x, name)), It.IsAny<string>(), It.IsAny<object>()))
                    .Returns((EntityId id, string operationName, object input) =>
                    {
                        var eMocker = this.GetRequiredService<EntityContextMocker>();
                        var result = callback(eMocker.GetEntityContext(input));
                        return Task.FromResult(result);
                    });
            };

            return this;
        }

        #endregion

        /// <summary>
        /// Calls the orchestration function represented by <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <param name="function">An expression representing the orchestration function.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="function"/> does not point to a callable orchestration function.</exception>
        public Task CallOrchestrationFunctionAsync<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> function) where TClass : class
        {
            var method = function.ToMethodInfo() ?? throw new ArgumentException("The expression does not represent a method.");
            this.ValidateOrchestrationMethod<TClass>(method, out var name, out var mock);

            this.ApplySetups(mock);
            var service = this.GetRequiredService<TClass>();
            var result = method.Invoke(service, new object?[] { mock.Object }) as Task;
            return result ?? Task.CompletedTask;
        }

        /// <summary>
        /// Calls the orchestration function represented by <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter.</typeparam>
        /// <param name="function">An expression representing the orchestration function.</param>
        /// <param name="input">The input to send to the orchestration function.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="function"/> does not point to a callable orchestration function.</exception>
        public Task CallOrchestrationFunctionAsync<TClass, TInput>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> function, TInput input) where TClass : class
        {
            var method = function.ToMethodInfo() ?? throw new ArgumentException("The expression does not represent a method.");
            this.ValidateOrchestrationMethod<TClass>(method, out var name, out var mock);

            this.MockInput<TInput>(mock, input);

            this.ApplySetups(mock);
            var service = this.GetRequiredService<TClass>();
            var result = method.Invoke(service, new object?[] { mock.Object }) as Task;
            return result ?? Task.CompletedTask;
        }

        /// <summary>
        /// Calls the orchestration function represented by <paramref name="function"/> and returns the value returned by the orchestration.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the orchestration.</typeparam>
        /// <param name="function">An expression representing the orchestration function.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="function"/> does not point to a callable orchestration function.</exception>
        /// <exception cref="Exception">The exception that is thrown if the orchestration function does not return a value with the type specified in <typeparamref name="TResult"/>.</exception>
        public Task<TResult> CallOrchestrationFunctionAsync<TClass, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> function) where TClass : class
        {
            var method = function.ToMethodInfo() ?? throw new ArgumentException("The expression does not represent a method.");
            this.ValidateOrchestrationMethod<TClass>(method, out var name, out var mock);

            this.ApplySetups(mock);
            var service = this.GetRequiredService<TClass>();
            var result = method.Invoke(service, new object?[] { mock.Object }) as Task<TResult>;
            return result ?? throw new Exception($"Orchestration function '{name}' did not return Task<{typeof(TResult).FullName}>.");
        }

        /// <summary>
        /// Calls the orchestration function represented by <paramref name="function"/> and returns the value returned by the orchestration.
        /// </summary>
        /// <typeparam name="TClass">The type of the class declaring the orchestration function.</typeparam>
        /// <typeparam name="TInput">The type of the input parameter.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the orchestration.</typeparam>
        /// <param name="function">An expression representing the orchestration function.</param>
        /// <param name="input">The input parameter.</param>
        /// <exception cref="ArgumentException">The exception that is thrown if <paramref name="function"/> does not point to a callable orchestration function.</exception>
        /// <exception cref="Exception">The exception that is thrown if the orchestration function does not return a value with the type specified in <typeparamref name="TResult"/>.</exception>
        public Task<TResult> CallOrchestrationFunctionAsync<TClass, TInput, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> function, TInput input) where TClass : class
        {
            var method = function.ToMethodInfo() ?? throw new ArgumentException("The expression does not represent a method.");
            this.ValidateOrchestrationMethod<TClass>(method, out var name, out var mock);

            this.MockInput<TInput>(mock, input);

            this.ApplySetups(mock);
            var service = this.GetRequiredService<TClass>();
            var result = method.Invoke(service, new object?[] { mock.Object }) as Task<TResult>;
            return result ?? throw new Exception($"Orchestration function '{name}' did not return Task<{typeof(TResult).FullName}>.");
        }

        /// <summary>
        /// Returns a mocked orchestration context instance without a specified input.
        /// </summary>
        public IDurableOrchestrationContext GetOrchestrationContext()
        {
            this.ApplySetups(this._orchestrationMock);
            return this._orchestrationMock.Object;
        }

        /// <summary>
        /// Returns a mocked orchestration context instance that specifies <paramref name="input"/> as input to the orchestration function the context is sent to.
        /// </summary>
        /// <typeparam name="TInput">The type of the input.</typeparam>
        /// <param name="input">The input value to send to the orchestration function.</param>
        public IDurableOrchestrationContext GetOrchestrationContext<TInput>(TInput input)
        {
            this.MockInput<TInput>(this._orchestrationMock, input);
            this.ApplySetups(this._orchestrationMock);

            return this._orchestrationMock.Object;
        }

        /// <summary>
        /// Returns a service from the service collection or <c>null</c> if the specified service is not found.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        public TService? GetService<TService>() where TService : class
        {
            return this.GetServiceProvider().GetService<TService>();
        }

        /// <summary>
        /// Returns a service from the service collection or <c>null</c> if the specified service is not found.
        /// </summary>
        /// <param name="serviceType">The type of the service to return.</param>
        public object? GetService(Type serviceType)
        {
            return this.GetServiceProvider().GetService(serviceType);
        }

        /// <summary>
        /// Returns a service from the service collection or throws an exception if the service is not found.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        public TService GetRequiredService<TService>() where TService : class
        {
            return this.GetServiceProvider().GetRequiredService<TService>();
        }

        /// <summary>
        /// Returns a service from the service collection or throws an exception if the service is not found.
        /// </summary>
        /// <param name="serviceType">The type of the service to return.</param>
        public object GetRequiredService(Type serviceType)
        {
            return this.GetServiceProvider().GetRequiredService(serviceType);
        }



        private Mock<IDurableOrchestrationContext> _orchestrationMock = new Mock<IDurableOrchestrationContext>();

        private void ApplySetups(Mock<IDurableOrchestrationContext> mock)
        {
            this.MockSetups.Values.ToList().ForEach(x => x());
            this.MockSetups.Clear();
        }

        private IServiceProvider GetServiceProvider()
        {
            return this.Services.BuildServiceProvider();
        }

        private void MockInput<TInput>(Mock<IDurableOrchestrationContext> mock, TInput input)
        {
            mock.Setup(x => x.GetInput<TInput>()).Returns(input);
        }

        private void ValidateActivityMethod<TClass>(MethodInfo? method, out string name, out Mock<IDurableOrchestrationContext> mock) where TClass : class
        {
            if (null == method) throw new ArgumentNullException(nameof(method), "Not a method.");
            if (!method.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if (!method.HasParameterWithAttribute<ActivityTriggerAttribute>()) throw new ArgumentException("The given method does not have an activity trigger parameter.");

            name = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");

            this.Services.AddSingleton<TClass>();
            mock = this._orchestrationMock;
        }

        private void ValidateEntityMethod<TClass>(MethodInfo? method, out string name, out Mock<IDurableOrchestrationContext> mock) where TClass : class
        {
            if (null == method) throw new ArgumentNullException(nameof(method), "Not a method.");
            if (!method.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if (!method.HasParameterWithAttribute<EntityTriggerAttribute>()) throw new ArgumentException("The given method does not have an entity trigger parameter.");

            name = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");
            this.Services.AddSingleton<TClass>();
            mock = this._orchestrationMock;
        }

        private void ValidateOrchestrationMethod<TClass>(MethodInfo? method, out string name, out Mock<IDurableOrchestrationContext> mock) where TClass : class
        {
            if (null == method) throw new ArgumentNullException(nameof(method), "Not a method.");
            if (!method.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if (!method.HasParameterWithAttribute<OrchestrationTriggerAttribute>()) throw new ArgumentException("The given method does not have an orchestration trigger parameter.");

            name = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");

            this.Services.AddSingleton<TClass>();
            mock = this._orchestrationMock;
        }

    }
}
