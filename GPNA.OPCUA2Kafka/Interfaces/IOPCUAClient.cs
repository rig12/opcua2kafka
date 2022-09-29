using Opc.Ua.Client;
using System.Collections.Generic;

namespace GPNA.OPCUA2Kafka.Interfaces
{
    public interface IOPCUAClient
    {
        MonitoredItemNotificationEventHandler? OnNotification { get; set; }

        IEnumerable<ITagConfiguration> TagConfigurations { get; set; }
    }
}
