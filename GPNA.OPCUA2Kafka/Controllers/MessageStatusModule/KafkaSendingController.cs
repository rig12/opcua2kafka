namespace GPNA.OPCUA2Kafka.Controllers.MessageStatusModule
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Interfaces;
    using AutoMapper;
    using Base;
    using GPNA.Converters.TagValues;
    using GPNA.MessageQueue.Entities;
    using GPNA.Templates.Interfaces;
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
