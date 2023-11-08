namespace OPCUA2Kafka.Controllers.Base
{
    using Templates.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Базовый класс управления модулем
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public abstract class ActiveModuleController : ControllerBase
    {
        #region Constructors
        public ActiveModuleController(IActiveModule module)
        {
            Module = module;
        }
        #endregion Constructors


        #region Fields
        public readonly IActiveModule Module;
        #endregion Fields


        #region Methods
        /// <summary>
        /// Перезапустить
        /// </summary>
        [HttpPost("Restart")]
        public ActionResult Restart()
        {
            if ( Module.Restart())
            {
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }

        /// <summary>
        /// Запустить
        /// </summary>
        [HttpPost("Start")]
        public ActionResult Start()
        {
            if (Module.Start())
            {
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }

        /// <summary>
        /// Остановить
        /// </summary>
        [HttpPost("Stop")]
        public ActionResult Stop()
        {
            if (Module.Stop())
            {
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }
        #endregion Methods
    }
}
