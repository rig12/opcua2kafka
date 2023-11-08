using AutoMapper;
using OPCUA2Kafka.Interfaces;
using Repository;

namespace OPCUA2Kafka.Model
{
    /// <summary>
    /// конфигурация тега для сбора по OPC UA для хранения в БД конфигурации
    /// </summary>
    [AutoMap(typeof(TagConfiguration), ReverseMap = true)]    
    public class TagConfigurationEntity : EntityBase, ITagConfiguration
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string Node { get; set; } = string.Empty;
        public int Period { get; set; } = 1000;
        public string Description { get; set; } = string.Empty;
        public string EngUnits { get; set; } = string.Empty;
    }
}
