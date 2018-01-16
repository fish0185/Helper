using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public static class ObjectCreator<T> where T : new()
    {
        public static T InstanceNew() //~1800 ms
        {
            return new T();
        }

        public static T InstanceActivator() //~1800 ms
        {
            return Activator.CreateInstance<T>();
        }

        public static readonly Func<T> InstanceNewFunc = () => new T(); //~1800 ms

        public static readonly Func<T> InstanceActivatorFunc = Activator.CreateInstance<T>; //~1800 ms

        //works for types with no default constructor as well
        public static readonly Func<T> InstanceFormatterServices = () =>
            (T)FormatterServices.GetUninitializedObject(typeof(T)); //~2000 ms


        public static readonly Func<T> Instance =
            Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        //~50 ms for classes and ~100 ms for structs


        // autofac reflection activator
        public static Func<object[], object> GetConstructorInvoker(ConstructorInfo constructorInfo)
        {
            var paramsInfo = constructorInfo.GetParameters();

            var parametersExpression = Expression.Parameter(typeof(object[]), "args");
            var argumentsExpression = new Expression[paramsInfo.Length];

            for (int paramIndex = 0; paramIndex < paramsInfo.Length; paramIndex++)
            {
                var indexExpression = Expression.Constant(paramIndex);
                var parameterType = paramsInfo[paramIndex].ParameterType;

                var parameterIndexExpression = Expression.ArrayIndex(parametersExpression, indexExpression);
                var convertExpression = parameterType.GetTypeInfo().IsPrimitive
                    ? Expression.Convert(ConvertPrimitiveType(parameterIndexExpression, parameterType), parameterType)
                    : Expression.Convert(parameterIndexExpression, parameterType);
                argumentsExpression[paramIndex] = convertExpression;

                if (!parameterType.GetTypeInfo().IsValueType) continue;

                var nullConditionExpression = Expression.Equal(
                    parameterIndexExpression, Expression.Constant(null));
                argumentsExpression[paramIndex] = Expression.Condition(
                    nullConditionExpression, Expression.Default(parameterType), convertExpression);
            }

            var newExpression = Expression.New(constructorInfo, argumentsExpression);
            var lambdaExpression = Expression.Lambda<Func<object[], object>>(newExpression, parametersExpression);

            return lambdaExpression.Compile();
        }

        public static MethodCallExpression ConvertPrimitiveType(Expression valueExpression, Type conversionType)
        {
            var changeTypeMethod = typeof(Convert).GetRuntimeMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) });
            return Expression.Call(changeTypeMethod, valueExpression, Expression.Constant(conversionType));
        }
    }
}
