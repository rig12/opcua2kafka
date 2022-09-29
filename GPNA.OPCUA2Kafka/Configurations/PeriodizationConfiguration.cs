using GPNA.Templates.Configurations;

namespace GPNA.OPCUA2Kafka.Configurations
{
    public class PeriodizationConfiguration : ConveyorConfiguration
    {
        public override int HandlersNumber { get; set; }
        public override int HandlersLimit { get; set; }
        public override int PauseIfFullQueue { get; set; }
        public override bool ToTraceProcessedCount { get; set; }
    }
}
