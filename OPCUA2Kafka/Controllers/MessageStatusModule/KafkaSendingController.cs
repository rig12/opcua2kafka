namespace OPCUA2Kafka.Controllers.MessageStatusModule
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Interfaces;
    using AutoMapper;
    using Base;
    using Converters.TagValues;
    using MessageQueue.Entities;
    using Templates.Interfaces;
    #endregion Using

    /// <summary>
    /// Состояние отправки сообщений в Kafka
    /// </summary>
    [Route("api/[controller]")]
    public class KafkaSendingController : MessageStatusModuleController
    {
        #region Constructors
        public KafkaSendingController(IConveyorModule<GenericMessageQueueEntity<TagValue>> module, IMapper mapper) : base(module, mapper)
        { }
        #endregion Constructors
    }
}
