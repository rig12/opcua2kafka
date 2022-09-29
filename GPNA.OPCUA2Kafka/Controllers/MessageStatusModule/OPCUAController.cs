namespace GPNA.OPCUA2Kafka.Controllers.MessageStatusModule
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Interfaces;
    using AutoMapper;
    using Base;
    using GPNA.OPCUA2Kafka.Modules;
    #endregion Using

    /// <summary>
    /// Состояние сбора данных с Suitelink
    /// </summary>
    [Route("api/[controller]")]
    public class OPCUAController : MessageStatusModuleController
    {
        #region Constructors
        public OPCUAController(IOPCUAConnectorModule module, IMapper mapper) : base(module, mapper)
        { }
        #endregion Constructors
    }
}
