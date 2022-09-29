namespace GPNA.OPCUA2Kafka.Controllers.Base
{
    #region Using
    using AutoMapper;
    using GPNA.Templates.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ModelDto;
    using System.Collections.Generic;
    #endregion Using

    /// <summary>
    /// Базовый класс состояния модуля
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public abstract class MessageStatusModuleController : ControllerBase
    {
        #region Constructors
        public MessageStatusModuleController(IMessageStatusModule module, IMapper mapper)
        {
            _mapper = mapper;
            _module = module;
        }
        #endregion Constructors


        #region Fields
        private readonly IMapper _mapper;
        private readonly IMessageStatusModule _module;
        #endregion Fields


        #region Methods
        /// <summary>
        /// Получить последний статус сообщение
        /// </summary>
        /// <response code="200">Возвращает объект MessageStatusDTO</response>
        [HttpGet("GetLast")]
        public ActionResult<MessageStatusDto> GetLast()
        {
            return Ok(_mapper.Map<MessageStatusDto>(_module.MessageStatusManager.Last));
        }

        /// <summary>
        /// Получить все сообщения
        /// </summary>
        /// <response code="200">Возвращает коллекцию объектов MessageStatusDTO</response>
        [HttpGet("GetAll")]
        public ActionResult<IEnumerable<MessageStatusDto>> GetAll()
        {
            return Ok(_mapper.Map<IEnumerable<MessageStatusDto>>(_module.MessageStatusManager.Entities));
        }

        /// <summary>
        /// Сбросить значения статуса
        /// </summary>
        [HttpPost]
        public ActionResult Reset()
        {
            _module.MessageStatusManager.Reset();
            return Ok();
        }
        #endregion Methods
    }
}
