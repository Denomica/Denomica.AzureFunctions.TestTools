using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

#if INPROCESS
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
#endif

namespace Denomica.AzureFunctions.TestTools.Core.Reflection
{
    public static class ReflectionExtensions
    {

#if INPROCESS
        public static bool IsOrchestrationFunction(this MethodInfo method)
        {
            return method.IsPublic
                && method.HasAttribute<FunctionNameAttribute>()
                && method.HasParameterWithAttribute<OrchestrationTriggerAttribute>();
        }
#endif

#if ISOLATED
        
#endif
        /// <summary>
        /// Returns <c>true</c> if <paramref name="method"/> is decorated with the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The method attribute to check.</typeparam>
        /// <param name="method">The method to check.</param>
        public static bool HasAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return null != method.GetCustomAttribute<TAttribute>();
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="method"/> has a parameter decorated with the given attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The parameter attribute to check.</typeparam>
        /// <param name="method">The method to check.</param>
        /// <returns></returns>
        public static bool HasParameterWithAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetParameters().Where(x => null != x.GetCustomAttribute<TAttribute>()).Count() > 0;
        }

        /// <summary>
        /// Returns the method info for the method that <paramref name="expression"/> describes.
        /// </summary>
        public static MethodInfo? ToMethodInfo(this Expression expression)
        {
            return expression
                ?.ToLambdaExpression()
                ?.ToUnaryExpression()
                ?.ToMethodCallExpression()
                ?.ToConstantExpression()
                ?.Value as MethodInfo;
        }



        private static LambdaExpression? ToLambdaExpression(this Expression expression)
        {
            return expression as LambdaExpression;
        }

        private static UnaryExpression? ToUnaryExpression(this LambdaExpression expression)
        {
            return expression?.Body as UnaryExpression;
        }

        private static MethodCallExpression? ToMethodCallExpression(this UnaryExpression expression)
        {
            return expression?.Operand as MethodCallExpression;
        }

        private static ConstantExpression? ToConstantExpression(this MethodCallExpression expression)
        {
            return expression?.Object as ConstantExpression;
        }
    }
}
