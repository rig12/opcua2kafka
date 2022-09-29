namespace GPNA.OPCUA2Kafka.Controllers.Msgp
{
    #region Using
    using Attributes;
    using GPNA.OPCUA2Kafka.Base;
    using GPNA.OPCUA2Kafka.Extensions;
    using GPNA.Templates.Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    #endregion Using


    /// <summary>
    /// Конфигурация тегов
    /// </summary>
    [Route("api/msgp/[controller]")]
    public class TagConfigurationController : TagConfigurationBaseController
    {
        #region Constructors
        public TagConfigurationController(ITagConfigurationManager tagConfigurationManager) : base(tagConfigurationManager)
        { }
        #endregion Constructors


        #region Methods
        /// <summary>
        /// Получить записи
        /// </summary>
        /// <param name="itemsOnPage">Элементов на странице. 0 - выборка всех</param>
        /// <param name="pageNumber">Номер страницы. 0 - выборка всех</param>
        /// <response code="200">Массив объектов. Формат MessagePack (byte[]). Модель: GET /model </response>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagConfigurationDto>>> Get(
            [Required][Range(0, int.MaxValue)] int itemsOnPage = 10,
            [Required][Range(0, int.MaxValue)] int pageNumber = 1)
        {
            var items = GetHandler(itemsOnPage, pageNumber);
            var buff = await items.ToMessagePackBuff();
            return File(buff.ToArray(), "application/octet-stream");
        }

        /// <summary>
        /// Получить по идентификатору записи
        /// </summary>
        /// <param name="id">Идентификатор в строковом виде. Число больше 0</param>
        /// <response code="200">Объект <see cref="TagConfigurationDto"/> в формате MessagePack (byte[]). Модель: GET /model </response>
        /// <response code="404">Запись с таким идентификатором не найдена</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagConfigurationDto>> GetById(
            [Required][StringIsLongId(ErrorMessage = MessageConstants.INCORRECT_STRING_ID_FORMAT_TEXT)][StringLength(19, MinimumLength = 1)] long id)
        {
            var item = GetByIdHandler(id);
            if (item == default)
            {
                return NotFound();
            }
            var buff = await item.ToMessagePackBuff();
            return File(buff.ToArray(), "application/octet-stream");
        }
        #endregion Methods
    }
}
