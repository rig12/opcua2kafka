namespace GPNA.OPCUA2Kafka.Controllers.ConveyorModule
{
    #region Using
    using AutoMapper;
    using Base;
    using GPNA.Converters.Interfaces;
    using GPNA.Converters.TagValues;
    using GPNA.Extensions.Types;
    using GPNA.OPCUA2Kafka.ModelDTO.TagValue;
    using Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    #endregion Using

    /// <summary>
    /// Управление сбором данных с Suitelink
    /// </summary>
    [Route("api/[controller]")]
    public class OPCUAController : ActiveModuleController
    {
        #region fields
        private readonly IFilterDuplicateValuesModule _filterDuplicateValuesModule;
        private readonly IMapper _mapper;

        #endregion


        #region Constructors
        public OPCUAController(IOPCUAConnectorModule module, 
            IMapper mapper,
            IFilterDuplicateValuesModule filterDuplicateValuesModule) : base(module)

        {
            _filterDuplicateValuesModule = filterDuplicateValuesModule;
            _mapper = mapper;
        }
        #endregion Constructors


        #region methods

        /// <summary>
        /// Получить текущие значения тэгов
        /// </summary>
        /// <param name="itemsOnPage">Элементов на странице. 0 - выборка всех</param>
        /// <param name="pageNumber">Номер страницы. 0 - выборка всех</param>
        /// <response code="200">Массив объектов. Формат JSON. Модель: GET /model </response>
        /// <returns></returns>
        [HttpGet(nameof(GetTagValues))]
        public ActionResult<TagValueDataDto> GetTagValues(
            [Required][Range(0, int.MaxValue)] int itemsOnPage = 10,
            [Required][Range(0, int.MaxValue)] int pageNumber = 1)
        {
            var currentvalues = _filterDuplicateValuesModule.CurrentValues?.Values?.Pagination(itemsOnPage, pageNumber) ?? Enumerable.Empty<TagValue>();

            return Ok(GetFromCurrentValues(currentvalues));
        }


        /// <summary>
        /// Получить текущее значение тэга
        /// </summary>
        /// <param name="tagname">Имя тэга или часть имени тэга</param>        
        /// <response code="200">Массив объектов. Формат JSON. Модель: GET /model </response>
        /// <returns></returns>
        [HttpGet(nameof(GetTagValueByName))]
        public ActionResult<TagValueDataDto> GetTagValueByName([Required] string tagname)
        {
            var currentvalues =
                _filterDuplicateValuesModule.CurrentValues
                    .Where(x => x.Value?.Tagname != null && x.Value.Tagname.Contains(tagname, System.StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value);

            if (currentvalues is IEnumerable<TagValue> collection)
                return Ok(GetFromCurrentValues(collection));

            return NotFound(tagname);
        }

        /// <summary>
        /// Получить текущие значения тегов по указанным именам
        /// </summary>
        /// <param name="requestTagnames">Имена запрашиваемых тэгов</param>        
        /// <response code="200">Массив объектов. Формат JSON. Модель: GET /model </response>
        /// <returns></returns>
        [HttpGet(nameof(GetTagValuesByNames))]
        public ActionResult<TagValueDataDto> GetTagValuesByNames([Required][FromQuery] IEnumerable<string> requestTagnames)
        {
            var result = Enumerable.Empty<TagValue>().ToHashSet();
            
            foreach (var tagname in requestTagnames)
            {
                if (!string.IsNullOrEmpty(tagname))
                {
                    var currentvalues = _filterDuplicateValuesModule.CurrentValues?.Where(x => x.Value?.Tagname != null
                        && x.Value.Tagname.EqualsInsensitive(tagname))?.Select(x => x.Value);

                    if (currentvalues != default)
                        foreach (var currentvalue in (currentvalues))
                        {
                            result.Add(currentvalue);
                        }
                }
            }

            if (result.Count > 0)
                return Ok(GetFromCurrentValues(result));

            return NotFound();
        }


        private TagValueDataDto GetFromCurrentValues(IEnumerable<TagValue> currentvalues)
        {
            var result = new TagValueDataDto
            {
                TagValuesNull = _mapper.Map<IEnumerable<TagValueNullDto>>(currentvalues.OfType<TagValueNull>()),
                TagValuesBool = _mapper.Map<IEnumerable<TagValueBoolDto>>(currentvalues.OfType<TagValueBool>()),
                TagValuesDouble = _mapper.Map<IEnumerable<TagValueDoubleDto>>(currentvalues.OfType<TagValueDouble>()),
                TagValuesInt = _mapper.Map<IEnumerable<TagValueIntDto>>(currentvalues.OfType<TagValueInt32>()),
                TagValuesString = _mapper.Map<IEnumerable<TagValueStringDto>>(currentvalues.OfType<TagValueString>())
            };
            return result;
        }

        #endregion
    }
}
