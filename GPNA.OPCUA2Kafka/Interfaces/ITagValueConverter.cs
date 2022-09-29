
using GPNA.Converters.TagValues;
using GPNA.DataModel.Integration;
using GPNA.OPCUA2Kafka.Configurations;

namespace GPNA.OPCUA2Kafka.Interfaces
{
    /// <summary>
    /// Интерфейс преобразования значений тегов
    /// </summary>
    public interface ITagValueConverter
    {
        /// <summary>
        /// Преобразование значения тэга для отправки в kafka
        /// </summary>
        /// <param name="lmxTagEvent">событие тега SuiteLink</param>
        /// <param name="datetimeParseFormat">формат парсинга даты-времени в случае типа значения тега дата-время</param>
        /// <returns>если <seealso cref="ConvertConfiguration.ConvertValuesToDouble"/>, преобразует значение в double (int->x.0; bool->0.0/1.0; string->'empty'/></returns>
        /// <returns></returns>
        TagValue GetTagValue(TagEvent lmxTagEvent, string datetimeParseFormat);

        /// <summary>
        /// Преобразование значения тэга в тип значения с плавающей запятой для отправки в kafka
        /// </summary>
        /// <param name="lmxTagEvent">событие тега SuiteLink</param>
        /// <param name="datetimeParseFormat">формат парсинга даты-времени в случае типа значения тега дата-время</param>
        /// <returns></returns>
        TagValue GetTagValueDouble(TagEvent lmxTagEvent, string datetimeParseFormat);


        dynamic? GetTagValue(string valueString, string datetimeParseFormat = "yyyy-MM-dd HH:mm");

    }
}
