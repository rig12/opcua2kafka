using AutoMapper;
using GPNA.Converters.TagValues;
using Newtonsoft.Json;
using System;

namespace GPNA.OPCUA2Kafka.ModelDTO.TagValue
{
    [AutoMap(typeof(TagValueString), ReverseMap = true)]
    public class TagValueStringDto
    {
        /// <summary>
        /// Идентификатор типа тега
        /// </summary>
        public long TagId { get; set; }

        /// <summary>
        /// Время фиксации тега
        /// </summary>
        public DateTime? DateTime { get; set; }

        /// <summary>
        /// Уровень качества OPC
        /// </summary>
        public int OpcQuality { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Полное наименование тэга
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Tagname { get; set; } = string.Empty;
    }
}
