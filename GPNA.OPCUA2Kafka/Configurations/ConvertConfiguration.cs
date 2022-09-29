using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Configurations
{
    /// <summary>
    /// Конфигурация конвертации значений
    /// </summary>
    public class ConvertConfiguration
    {
        /// <summary>
        /// задаёт использование типа конвертации - попытка преобразования данных в double
        /// bool -> 0.0/1.0; int -> x.0; string -> null
        /// </summary>
        public bool ConvertValuesToDouble { get; set; }
        
        /// <summary>
        /// Формат даты для парсинга значения тэга типа "дата-время"
        /// </summary>
        public string DatetimeParseFormat { get; set; } = "yyyy-MM-dd HH:mm";
        
        /// <summary>
        /// Отправлять сообщения со значением NULL
        /// </summary>
        public bool ToSendNullValues { get; set; }
    }
}
