using GPNA.Templates.Interfaces;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Interfaces
{
    public interface IOPCUAConnectorModule : IMessageStatusModule
    {
        Task<string> CompleteReload();
    }
}
