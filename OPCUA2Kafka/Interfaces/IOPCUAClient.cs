using Opc.Ua.Client;
using System.Collections.Generic;

namespace OPCUA2Kafka.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOPCUAClient
    {
        /// <summary>
        /// 
        /// </summary>
        MonitoredItemNotificationEventHandler? OnNotification { get; set; }

    }
}
