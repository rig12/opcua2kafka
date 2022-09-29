
using GPNA.Templates.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Configurations
{
    /// <summary>
    /// Конфигурация фильтрации
    /// </summary>
    public class FilterConfiguration : ConveyorConfiguration
    {
        /// <summary>
        /// Фильтрация включена
        /// </summary>
        public bool IsEnabled { get; set; }

        public override int HandlersNumber { get; set; }

        public override int HandlersLimit { get; set; }

        public override int PauseIfFullQueue { get; set; }
        public override bool ToTraceProcessedCount { get; set; }
    }
}
