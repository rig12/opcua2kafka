
using GPNA.OPCUA2Kafka.Model;

namespace GPNA.OPCUA2Kafka.Configurations
{   
    /// <summary>
    /// Конфигурация инициализации модулей
    /// </summary>
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
        /// Сохранение данных в MSSQL-БД
        /// </summary>
        public bool RegisterSaveStarted { get; set; }

        /// <summary>
        /// включение кэширования (Store and Forward)
        /// </summary>
        public bool CacheStarted { get; internal set; }
    }
}
