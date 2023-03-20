using GPNA.Templates.Configurations;

namespace GPNA.OPCUA2Kafka.Configurations
{
    public class OPCUAConfiguration : ConveyorConfiguration
    {
        public string EndpointURL { get; set; } = string.Empty;
        public override int HandlersNumber { get; set; } = 1;
        public override int HandlersLimit { get; set; } = 10000;
        public override int PauseIfFullQueue { get; set; } = 100;
        public override bool ToTraceProcessedCount { get; set; }
        public int DefaultPublishingInterval { get; set; } = 1000;
        public bool SecurityNone { get; set; }
        public bool AutoAccept { get; set; }
    }
}

