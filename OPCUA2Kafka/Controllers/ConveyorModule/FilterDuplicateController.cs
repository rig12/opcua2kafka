
using Templates.Interfaces;
using OPCUA2Kafka.Controllers.Base;
using OPCUA2Kafka.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OPCUA2Kafka.Controllers.ConveyorModule
{
    /// <summary>
    /// Управление фильтрацией дублированных значений
    /// </summary>
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class FilterDuplicateValuesController : ConveyorModuleController
    {
        public FilterDuplicateValuesController(IFilterDuplicateValuesModule module) : base(module)
        {
        }
    }
}
