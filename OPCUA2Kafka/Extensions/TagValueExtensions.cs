using Converters.TagValues;
using Extensions.Types;
using Templates.Constants;
using System;

namespace OPCUA2Kafka.Extensions
{
    public static class TagValueExtensions
    {
        public static void SetTagValue(this TagValue tagValue, object value)
        {
            if (value.GetType() is Type type && (type.IsSimple() || type == typeof(DateTime)))
            {
                var valueproperty = tagValue.GetType().GetProperty(CommonConstants.VALUE_PROPERTY_NAME);
                if (valueproperty != null)
                {
                    if (type == typeof(string) && value.ToString()?.Equals(CommonConstants.NULL_VALUE_WORD, StringComparison.OrdinalIgnoreCase) is bool isnil && isnil)
                    {
                        valueproperty.SetValue(tagValue, null);
                    }
                    else
                    {
                        valueproperty.SetValue(tagValue, value);
                    }
                }
            }
        }
    }
}
