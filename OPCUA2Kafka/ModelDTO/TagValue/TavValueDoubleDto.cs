using AutoMapper;
using Converters.TagValues;
using Newtonsoft.Json;
using System;

namespace OPCUA2Kafka.ModelDTO.TagValue
{
    [AutoMap(typeof(TagValueDouble), ReverseMap = true)]
    public class TagValueDoubleDto
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
        public double Value { get; set; }

        /// <summary>
        /// Полное наименование тэга
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Tagname { get; set; } = string.Empty;
    }
}
