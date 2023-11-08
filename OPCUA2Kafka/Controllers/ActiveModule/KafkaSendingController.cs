namespace OPCUA2Kafka.Controllers.ActiveModule
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using ModelDto;
    using OPCUA2Kafka.Controllers.Base;
    using Templates.Interfaces;
    using MessageQueue.Entities;
    using Converters.TagValues;
    using OPCUA2Kafka.Modules;
    #endregion Using

    /// <summary>
    /// Управление отправкой сообщений в Kafka
    /// </summary>
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class KafkaSendingController : ActiveModuleController
    {
        #region Constructors
        public KafkaSendingController(IConveyorModule<GenericMessageQueueEntity<TagValue>> module) : base(module)
        {
            _module = module;
        }
        #endregion Constructors


        #region Fields
        protected readonly IConveyorModule<GenericMessageQueueEntity<TagValue>> _module;
        #endregion Fields


        #region Methods
        /// <summary>
        /// Получить текущее количество сообщений, ждущих отправки
        /// </summary>
        /// <response code="200">Объект значения счетчика</response>
        [HttpPost(nameof(GetCount))]
        public ActionResult<CountResultDto<int>> GetCount()
        {
            return Ok(new CountResultDto<int>()
            {
                Value = (_module as IConveyorModule<GenericMessageQueueEntity<TagValue>>)?.Count ?? 0
            });
        }
        #endregion Methods
    }
}
