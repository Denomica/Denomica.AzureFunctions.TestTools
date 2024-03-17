using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AzureFunctions.TestTools.Core.Reflection
{
    public static class ReflectionExtensions
    {

        public static bool HasAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return null != method.GetCustomAttribute<TAttribute>();
        }

        public static bool HasParameterWithAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetParameters().Where(x => null != x.GetCustomAttribute<TAttribute>()).Count() > 0;
        }

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
