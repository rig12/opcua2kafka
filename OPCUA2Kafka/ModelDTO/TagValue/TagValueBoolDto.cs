﻿using AutoMapper;
using Converters.TagValues;
using Newtonsoft.Json;
using System;

namespace OPCUA2Kafka.ModelDTO.TagValue
{
    /// <summary>
    /// 
    /// </summary>
    [AutoMap(typeof(TagValueBool), ReverseMap = true)]
    public class TagValueBoolDto
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
        public bool Value { get; set; }

        /// <summary>
        /// Полное наименование тэга
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Tagname { get; set; } = string.Empty;
    }
}
