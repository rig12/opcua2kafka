using OPCUA2Kafka.Model;

namespace OPCUA2Kafka.Configurations
{
    public class InitializationModuleConfiguration
    {
        /// <summary>
        /// запуск модуля дискретности <seealso cref="TagConfiguration.Period"/>
        /// </summary>
        public bool PeriodizationStarted { get; set; }

        /// <summary>
        /// Запуск сбора данных с Suitelink
        /// </summary>
        public bool OPCUAConnectorStarted { get; set; }

        /// <summary>
        /// включение кэширования (Store and Forward)
        /// </summary>
        public bool CacheStarted { get; set; }

    }
}
