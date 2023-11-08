using GPNA.Templates.Interfaces;
using System;

namespace GPNA.OPCUA2Kafka.Interfaces
{

    /// <summary>
    /// Интерфейс модуля отсылки данных в Kafka
    /// </summary>
    public interface IKafkaSendingModule : IMessageStatusModule 
    {
    }

}
