using Templates.Interfaces;
using System;
using System.Collections.Generic;

namespace OPCUA2Kafka.Interfaces
{
    public interface ICacheModule<T> : IMessageStatusModule
    {
        event EventHandler<IEnumerable<T>>? OnUncache;
        void SetConnectionState(bool value);
    }
}
