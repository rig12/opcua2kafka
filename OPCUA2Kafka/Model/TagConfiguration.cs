using OPCUA2Kafka.Interfaces;

namespace OPCUA2Kafka.Model
{
    /// <summary>
    /// конфигурация тега для сбора по OPC UA
    /// </summary>
    public class TagConfiguration: ITagConfiguration
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
