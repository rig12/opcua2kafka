using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCUA2Kafka.ModelDTO.TagValue
{
    /// <summary>
    /// текущие значения тэгов
    /// </summary>
    public class TagValueDataDto
    {
        /// <summary>
        /// значения тэгов типа число с плавающей точкой
        /// </summary>
        public IEnumerable<TagValueDoubleDto> TagValuesDouble { get; set; } = Enumerable.Empty<TagValueDoubleDto>();

        /// <summary>
        /// значения тэгов целочисленного типа
        /// </summary>
        public IEnumerable<TagValueIntDto> TagValuesInt { get; set; } = Enumerable.Empty<TagValueIntDto>();

        /// <summary>
        /// значения тэгов булевого типа
        /// </summary>
        public IEnumerable<TagValueBoolDto> TagValuesBool { get; set; } = Enumerable.Empty<TagValueBoolDto>();

        /// <summary>
        /// значения тэгов строкового типа
        /// </summary>
        public IEnumerable<TagValueStringDto> TagValuesString { get; set; } = Enumerable.Empty<TagValueStringDto>();

        /// <summary>
        /// тэги с неопределённым значением (null)
        /// </summary>
        public IEnumerable<TagValueNullDto> TagValuesNull { get; set; } = Enumerable.Empty<TagValueNullDto>();


    }
}
