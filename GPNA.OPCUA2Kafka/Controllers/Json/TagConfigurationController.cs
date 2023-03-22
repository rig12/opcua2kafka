using GPNA.OPCUA2Kafka.Base;
using GPNA.OPCUA2Kafka.Interfaces;
using GPNA.OPCUA2Kafka.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GPNA.OPCUA2Kafka.Controllers.Json
{


    /// <summary>
    /// Конфигурация тегов
    /// </summary>
    [Route("api/[controller]")]
    public class TagConfigurationController : TagConfigurationBaseController
    {
        #region Constructors

        public TagConfigurationController(ITagConfigurationManager tagConfigurationManager)
            : base(tagConfigurationManager) { }

        #endregion Constructors


        #region Methods
        /// <summary>
        /// Получить записи
        /// </summary>
        /// <param name="itemsOnPage">Элементов на странице. 0 - выборка всех</param>
        /// <param name="pageNumber">Номер страницы. 0 - выборка всех</param>
        /// <response code="200">Массив объектов. Формат JSON. Модель: GET /model </response>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<TagConfigurationEntity>> Get(
            [Required][Range(0, int.MaxValue)] int itemsOnPage = 10,
            [Required][Range(0, int.MaxValue)] int pageNumber = 1)
        {
            return Ok(GetHandler(itemsOnPage, pageNumber));
        }


        /// <summary>
        /// Получить по идентификатору записи
        /// </summary>
        /// <param name="id">Идентификатор в строковом виде. Число больше 0</param>
        /// <response code="200">Объект <see cref="TagConfigurationEntity"/> </response>
        /// <response code="404">Запись с таким идентификатором не найдена</response>
        /// <returns></returns>
        [HttpGet("GetById")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TagConfigurationEntity> GetById(
            [Required] long id)
        {
            var item = GetByIdHandler(id);
            if (item == default)
            {
                return NotFound();
            }
            return Ok(item);
        }


        /// <summary>
        /// Получить по имени тега или части имени тега
        /// </summary>
        /// <param name="tagname">имя тега или его часть</param>
        /// <response code="200">Объект <see cref="TagConfigurationEntity"/> </response>      
        /// <response code="404">Запись не найдена</response>
        /// <returns></returns>
        [HttpGet("GetByTagname")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TagConfigurationEntity>> GetByTagname([Required] string tagname)
        {
            var items = GetTagsByAlias(tagname);
            if (items == default)
            {
                return NotFound(tagname);
            }
            return Ok(items);
        }

        #endregion Methods
    }
}
