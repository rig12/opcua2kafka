using Repository;
using System;

namespace OPCUA2Kafka.Model
{
    public class TagValueMessageEntity : EntityBase
    {
        public string TagName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime DateTimeUtc { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public DateTime Datetime { get; set; }
        public int OpcQuality { get; set; }
        public long TagId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Type { get; set; }= string.Empty;
    }
}
