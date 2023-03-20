using GPNA.Templates.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Interfaces
{
    /// <summary>
    /// интерфейс модуля OPC UA - соединения
    /// </summary>
    public interface IOPCUAConnectorModule : IMessageStatusModule
    {
        /// <summary>
        /// перезагрузка конфигурации
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> CompleteReload();
    }
}
