using AutoMapper;
using GPNA.Converters.TagValues;
using Newtonsoft.Json;
using System;

namespace GPNA.OPCUA2Kafka.ModelDTO.TagValue
{
    [AutoMap(typeof(TagValueInt32), ReverseMap = true)]
    public class TagValueIntDto
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
        public int Value { get; set; }

        /// <summary>
        /// Полное наименование тэга
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Tagname { get; set; } = string.Empty;
    }
}
