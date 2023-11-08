namespace GPNA.OPCUA2Kafka.Controllers.ConveyorModule
{
    #region using
    using GPNA.OPCUA2Kafka.Controllers.Base;
    using GPNA.OPCUA2Kafka.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    #endregion

    /// <summary>
    /// Управление периодизацией (дискретностью) отправки данных в кафку
    /// </summary>
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class PeriodizationController : ConveyorModuleController
    {
        public PeriodizationController(IPeriodizationModule module) : base(module) { }
    }
}