namespace GPNA.OPCUA2Kafka.Services
{
    #region Using
    using GPNA.Converters.TagValues;
    using GPNA.DataModel.Integration;
    using GPNA.Extensions.Types;
    using GPNA.OPCUA2Kafka.Extensions;
    using GPNA.Templates.Constants;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    #endregion Using

    /// <summary>
    /// Класс преобразования значений тегов
    /// </summary>
    public class TagValueConverter : ITagValueConverter
    {
        #region Constructors

        public TagValueConverter()
        {
            foreach (var item in _pairs)
                _lambdas.Add(item.Key, BuildLambda(item.Key));
        }

        #endregion Constructors


        #region fields

        private readonly Dictionary<Type, Func<TagValue>> _lambdas = new();

        private static readonly Dictionary<Type, Type> _pairs = new()
        {
            { typeof(double), typeof(TagValueDouble) },
            { typeof(bool), typeof(TagValueBool) },
            { typeof(int), typeof(TagValueInt32) },
            { typeof(string), typeof(TagValueString) },
            { typeof(DateTime), typeof(TagValueDateTime) }

        };

        #endregion fields


        #region Methods

        /// <summary>
        /// Возвращает лябмда-выражение, возвращающее экземпляр указанного в аргументе типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<TagValue> BuildLambda(Type type)
        {
            var newExpr = Expression.New(_pairs[type]);
            var newlambda = Expression.Lambda(newExpr);
            var func = newlambda.Compile();
            if (func is Func<TagValue> nnfunc)
                return nnfunc;

            throw new ApplicationException($"Undefined value type in pairs dictionary");
        }

        /// <summary>
        /// возвращает значение value в переменной исходного типа (tryparse) в кортеже с типом
        /// </summary>
        /// <param name="value"></param>
        /// <param name="datetimeParseFormat"></param>
        /// <returns></returns>
        private static dynamic? GetValue(string? value, string datetimeParseFormat)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            if (DateTime.TryParseExact(value, datetimeParseFormat, null, DateTimeStyles.None, out var datetimeval))
                return datetimeval.ToUniversalTime();

            if (int.TryParse(value, out var intval))
                return intval;

            if (bool.TryParse(value, out var boolval))
                return boolval;

            if (double.TryParse(value, out var doubleval))
                return doubleval;

            return value;

        }

        #endregion

        #region ITagValueConverter

        public dynamic? GetTagValue(string valueString, string datetimeParseFormat = "yyyy-MM-dd HH:mm")
        {
            return GetValue(valueString, datetimeParseFormat);
        }



        public TagValue GetTagValue(TagEvent lmxTagEvent, string datetimeParseFormat)
        {
            var value = TagValueConverter.GetValue(lmxTagEvent.Value?.ToString(), datetimeParseFormat);
            TagValue? result;
            if (value != null)
            {
                var objvalue = value as object;
                var func = _lambdas[objvalue.GetType()];
                result = func();

                if (objvalue?.GetType() is Type type && (type.IsSimple() || type == typeof(DateTime)))
                {
                    var valueproperty = result.GetType().GetProperty(CommonConstants.VALUE_PROPERTY_NAME);
                    if (valueproperty != null)
                    {
                        if (type == typeof(string) && objvalue.ToString()?.Equals(CommonConstants.NULL_VALUE_WORD, StringComparison.OrdinalIgnoreCase) is bool isnil && isnil)
                        {
                            valueproperty.SetValue(result, null);
                        }
                        else
                        {
                            valueproperty.SetValue(result, objvalue);
                        }
                    }
                }
            }
            else
            {
                result = new TagValueNull();
            }
            
            result.FillTagValueData(lmxTagEvent);

            return result;
        }

        /// <summary>
        /// конвертация значения тэга в double
        /// </summary>
        /// <param name="lmxTagEvent"></param>
        /// <param name="datetimeParseFormat"></param>
        /// <returns>double as is; int->x.0; bool->0.0/1.0; string->null</returns>
        public TagValue GetTagValueDouble(TagEvent lmxTagEvent,
            string datetimeParseFormat)
        {
            if (lmxTagEvent.Value == default)
            {
                return new TagValueNull();
            }
            else
            {
                var result = new TagValueDouble();
                result.FillTagValueData(lmxTagEvent);

                var valuestr = lmxTagEvent.Value?.ToString();

                if (double.TryParse(valuestr, out var doublevalue))
                {
                    result.Value = doublevalue;
                    return result;
                }
                else
                if (bool.TryParse(valuestr, out var boolvalue))
                {
                    result.Value = boolvalue ? 1 : 0;
                    return result;
                }
                else
                if (DateTime.TryParseExact(valuestr, datetimeParseFormat, null, DateTimeStyles.None, out var datetimevalue))
                {
                    result.Value = datetimevalue.ToUniversalTime().ConvertToUnixTimestamp();
                    return result;
                }
                else
                {
                    result.Value = null;
                    return result;
                }
            }
        }

        #endregion

    }
}
