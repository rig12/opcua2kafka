
using Converters.TagValues;
using MessageQueue.Entities;
using MessageQueue.Interfaces;
using OPCUA2Kafka.Configurations;
using OPCUA2Kafka.Interfaces;
using Scheduler.Interfaces;
using Templates.Constants;
using Templates.Interfaces;
using Templates.Modules;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace OPCUA2Kafka.Modules
{

    /// <summary>
    /// Класс модуля отсылки данных в Kafka
    /// </summary>
    public class KafkaSendingModule : ConveyorModuleIEnumerable<GenericMessageQueueEntity<TagValue>>
    {
        private readonly IQueueMessageService<TagValue> _queueMessageService;
        private readonly ITagConfigurationManager _tagConfigurationManager;
        private readonly Dictionary<ITagConfiguration, string> _tagAliases = new();

        #region Constructors
        public KafkaSendingModule(IMessageStatusManager messageStatusManager,
            KafkaConfiguration kafkaConfiguration,
            ILogger<KafkaSendingModule> logger,
            IQueueMessageService<TagValue> queueMessageService,
            ISchedulerFactory schedulerFactory,
            ITagConfigurationManager tagConfigurationManager
            )
            : base(messageStatusManager, schedulerFactory, logger, kafkaConfiguration)
        {
            _queueMessageService = queueMessageService;
            _tagConfigurationManager = tagConfigurationManager;

            MessageStatusManager.Add(MessageConstants.HAS_BEEN_INITIATED_TEXT);
        }
        #endregion Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        public override void Process(IEnumerable<GenericMessageQueueEntity<TagValue>> messages)
        {
            var result = new List<(string, TagValue)>();

            foreach (var message in messages)
            {
                if (!string.IsNullOrEmpty(message.Topic)
                    && message.Payload is TagValue tagValue
                    && _tagConfigurationManager.TagConfigurations.TryGetValue(tagValue.Tagname, out var tagconfig))
                {
                    result.Add((tagconfig.Topic, tagValue));
                }
            }
            _queueMessageService.SendMessages(result);
        }
    }
}