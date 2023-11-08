using Templates.Configurations;

namespace OPCUA2Kafka.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class OPCUAModuleConfiguration : ConveyorConfiguration
    {
        public override int HandlersNumber { get; set; } = 1;
        public override int HandlersLimit { get; set; } = 10000;
        public override int PauseIfFullQueue { get; set; } = 100;
        public override bool ToTraceProcessedCount { get; set; }
        public bool SecurityNone { get; set; }
        public bool AutoAccept { get; set; }
        public int DefaultPublishingInterval { get; set; } = 5000;
    }
}

