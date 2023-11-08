namespace OPCUA2Kafka.Base
{
    #region Using
    using Extensions.Types;
    using OPCUA2Kafka.Model;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    #endregion Using

    /// <summary>
    /// Базовый класс конфигурации тегов
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public abstract class TagConfigurationBaseController : ControllerBase
    {
        #region Constructors
        public TagConfigurationBaseController(ITagConfigurationManager tagConfigurationManager)
        {
            _tagConfigurationManager = tagConfigurationManager;
        }
        #endregion Constructors


        #region Fields
        private readonly ITagConfigurationManager _tagConfigurationManager;
        #endregion Fields


        #region Methods

        protected TagConfigurationEntity? GetByIdHandler(long id)
        {
            //var idLong = long.Parse(id);
            return _tagConfigurationManager.TagConfigurations.Values.Cast<TagConfigurationEntity>().FirstOrDefault(x => x.Id == id);
        }

        protected IEnumerable<TagConfigurationEntity> GetHandler(int itemsOnPage, int pageNumber)
        {
            return _tagConfigurationManager.TagConfigurations.Values
                .Pagination(itemsOnPage, pageNumber)
                .Cast<TagConfigurationEntity>();
        }

        protected IEnumerable<TagConfigurationEntity> GetTagsByAlias(string tagname)
        {
            return _tagConfigurationManager.TagConfigurations.Values.Where(x =>
                x?.Alias is string notnulltagname && notnulltagname.Contains(tagname, System.StringComparison.OrdinalIgnoreCase))
                .Cast<TagConfigurationEntity>();
        }
        #endregion Methods
    }
}
