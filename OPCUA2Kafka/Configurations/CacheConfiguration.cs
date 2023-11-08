using Templates.Configurations;

namespace OPCUA2Kafka.Configurations
{
    public class CacheConfiguration : ConveyorConfiguration
    {
        public override int HandlersNumber { get; set; } = 1;
        public override int HandlersLimit { get; set; } = 1000;
        public override int PauseIfFullQueue { get; set; } = 100;
        public override bool ToTraceProcessedCount { get; set; } = false;
        public int CacheSendIntervalSec { get; set; } = 1;
    }
}
