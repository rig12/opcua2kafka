
using Extensions.Attributes;
using OPCUA2Kafka.Interfaces;
using OPCUA2Kafka.Model;
using Templates.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPCUA2Kafka.Controllers
{

    /// <summary>
    /// Конфигурация обработки тегов
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class TagConfigurationController : ControllerBase
    {
        #region Constructors
        public TagConfigurationController(ITagConfigurationManager tagConfigurationManager,
            IOPCUAConnectorModule oPCUAConnectorModule
            //IPeriodizationModule periodizationModule
            )
        {
            _tagConfigurationManager = tagConfigurationManager;
            _oPCUAConnectorModule = oPCUAConnectorModule;
            //_periodizationModule = periodizationModule;
        }
        #endregion Constructors


        #region Fields
        private readonly ITagConfigurationManager _tagConfigurationManager;
        private readonly IOPCUAConnectorModule _oPCUAConnectorModule;
        //private readonly IPeriodizationModule _periodizationModule;
        #endregion Fields


        #region Methods
        #region Common


        /// <summary>
        /// Загрузка конфигурации
        /// </summary>
        /// <response code="200">Конфигурация успешно загружена</response>
        /// <response code="409">Конфигурация не загружена</response>
        [HttpPost("Load")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<bool> Load()
        {
            //Periodization - {string.Join("; ",_periodizationModule.ReloadConfig())};
            return Ok($"Tags Reloaded - {_oPCUAConnectorModule.CompleteReload().Result}");
        }
        #endregion Common

        /// <summary>
        /// Создать конфигурацию
        /// </summary>
        /// <response code="200">Возвращает объект <see cref="TagConfigurationEntity"/></response>
        /// <response code="500">Возвращает сведения о внутренней ошибке</response>
        /// <param name="tagConfigurations">Объект конфигурации</param>
        [HttpPost("AddConfigurations")]
        public ActionResult<IEnumerable<TagConfigurationEntity>> Add(IEnumerable<TagConfiguration> tagConfigurations)
        {
            return Ok(_tagConfigurationManager.Add(tagConfigurations));
        }


        #region Delete
        /// <summary>
        /// Удалить конфигурацию по идентификатору
        /// </summary>
        /// <response code="200">Возвращает в случае успеха</response>
        /// <response code="500">Возвращает сведения о внутренней ошибке</response>
        /// <response code="404">Возвращает если конфигурации с таким идентификатором не существует</response>
        [HttpDelete("Delete")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(
            [Required][StringIsLongId(ErrorMessage = MessageConstants.INCORRECT_STRING_ID_FORMAT_TEXT)][StringLength(19, MinimumLength = 1)] string id)
        {
            if (_tagConfigurationManager.Delete(long.Parse(id)))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
        #endregion Delete

        #region Update
        /// <summary>
        /// Обновить конфигурацию
        /// </summary>
        /// <response code="200">Возвращает в случае успеха</response>
        /// <response code="500">Возвращает сведения о внутренней ошибке</response>
        /// <response code="404">Возвращает если конфигурации с таким идентификатором не существует</response>
        /// <param name="tagConfiguration">Объект конфигурации</param>
        [HttpPost("UpdateConfiguration")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Update(TagConfigurationEntity tagConfiguration)
        {
            if (_tagConfigurationManager.Update(tagConfiguration))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
        #endregion Update
        #endregion Methods
    }
}
