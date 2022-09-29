using GPNA.Templates.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GPNA.OPCUA2Kafka.Controllers.Base
{
    /// <summary>
    /// Базовый класс управления конвейерным модулем
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public abstract class ConveyorModuleController : ActiveModuleController
    {
        public ConveyorModuleController(IConveyorModule module) : base(module)
        {
        }

        /// <summary>
        /// Получить количество принятых сообщений в обработке
        /// </summary>
        /// <response code="200">Количество принятых сообщений в обработке</response>
        /// <returns></returns>И
        [HttpGet(nameof(Count))]
        public ActionResult<int> Count()
        {
            return Ok((Module as IConveyorModule)?.Count);
        }
    }
}
