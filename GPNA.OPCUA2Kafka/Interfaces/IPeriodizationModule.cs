using GPNA.Templates.Interfaces;
using System.Collections.Generic;

namespace GPNA.OPCUA2Kafka.Interfaces
{

    /// <summary>
    /// модуль отправки данных с определённой периодичностью (последнее значение в интервале)
    /// </summary>
    public interface IPeriodizationModule : IConveyorModule
    {
        /// <summary>
        /// перезагрузка конфигурации дискретности
        /// </summary>
        IEnumerable<string> ReloadConfig();
    }
}