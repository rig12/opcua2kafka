using GPNA.OPCUA2Kafka.Interfaces;

namespace GPNA.OPCUA2Kafka.Model
{
    public class TagConfiguration: ITagConfiguration
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string Tagname { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string Node { get; set; } = string.Empty;
        public int Period { get; set; } = 1000;
        public string Description { get; set; } = string.Empty;
        public string EngUnits { get; set; } = string.Empty;
    }
}
