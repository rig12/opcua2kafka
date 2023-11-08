using AutoMapper;
using GPNA.OPCUA2Kafka.Controllers.Base;
using GPNA.OPCUA2Kafka.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GPNA.OPCUA2Kafka.Controllers.MessageStatusModule
{
    /// <summary>
    /// Состояние отправки сообщений в Kafka
    /// </summary>
    [Route("api/[controller]")]
    public class PeriodizationController : MessageStatusModuleController
    {
        public PeriodizationController(IPeriodizationModule module, IMapper mapper) : base(module, mapper)
        {
        }
    }
}
