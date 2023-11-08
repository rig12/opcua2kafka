using Opc.Ua.Client;
using System;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Interfaces
{
    public interface IOPCUAClientHelper
    {
        event Action<bool, DateTime, string, object?>? OnStatusChange;

        Task<Session> ConnectAsync(
            string? serverUrl = null,
            bool useSecurity = false,
            uint sessionTimeout = 0
            );
    }
}
