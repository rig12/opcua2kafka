using GPNA.Converters.TagValues;
using System;
using System.Linq.Expressions;

namespace GPNA.OPCUA2Kafka.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// присваивает свойству <paramref name="propertyName"/> вызываемого объекта значение <paramref name="value"/>
        /// </summary>
        /// <param name="tagvalue"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetValue(this TagValue tagvalue, string propertyName, object? value)
        {
            if (tagvalue != null)
            {
                var paramexpr = Expression.Parameter(tagvalue.GetType());
                var propexpr = Expression.PropertyOrField(paramexpr, propertyName);

                var constantExpression = Expression.Constant(value);

                var convertexpr = Expression.Convert(constantExpression, propexpr.Type);

                var assign = Expression.Assign(propexpr, convertexpr);

                if (assign is BinaryExpression binaryExpression)
                {
                    var lambda = Expression.Lambda(binaryExpression, new ParameterExpression[] { paramexpr });
                    var action = lambda.Compile();
                    action.DynamicInvoke(tagvalue);

                    return;
                }
            }
            throw new ArgumentException($"Unable to set Value {value} for object {tagvalue}");
        }

        public static object? GetPropertyValue(this TagValue tagvalue, string propName)
        {
            var type = tagvalue.GetType();
            var prop = type.GetProperty(propName);
            var value=prop?.GetValue(tagvalue, null);
            
            return value;
            
        }
    }
}
