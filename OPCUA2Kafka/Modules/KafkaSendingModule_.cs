using AutoMapper;
using global::GPNA.Converters.TagValues;
using global::GPNA.DataModel.Common;
using global::GPNA.Extensions.Types;
using global::GPNA.MessageQueue.Entities;
using global::GPNA.MessageQueue.Interfaces;
using global::GPNA.Scheduler.Interfaces;
using global::GPNA.Templates.Interfaces;
using global::GPNA.Templates.Modules;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.OPCUA2Kafka.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GPNA.OPCUA2Kafka.Modules
{
    /// <summary>
    /// Модуль отсылки данных в Kafka
    /// </summary>
    public class KafkaSendingModule_ : ConveyorModule<GenericMessageQueueEntity<TagValue>>, IKafkaSendingModule
    {
        #region Constructors
        public KafkaSendingModule_(KafkaConfiguration kafkaConfiguration,
            IMessageStatusManager messageStatusManager,
            ILogger<KafkaSendingModule_> logger,
            ISchedulerFactory schedulerFactory,
            IGenericMessageProducer<TagValue> messageProducer,
            ICacheModule<GenericMessageQueueEntity<TagValue>> cacheModule,
            IMapper mapper
            )
            : base(messageStatusManager, schedulerFactory, logger, kafkaConfiguration)
        {
            Name = nameof(KafkaSendingModule_).CamelCaseToHuman();
            _kafkaConfiguration = kafkaConfiguration;
            _messageProducer = messageProducer;
            _cacheModule = cacheModule;
            _mapper = mapper;

            messageProducer.SetConnectionStateTimeout(new TimeSpan(0, 0, _kafkaConfiguration.MessageTimeoutMs / 1000));

        }

        #endregion Constructors


        #region Fields
        private readonly KafkaConfiguration _kafkaConfiguration;
        private readonly IGenericMessageProducer<TagValue> _messageProducer;
        private readonly ICacheModule<GenericMessageQueueEntity<TagValue>> _cacheModule;
        private readonly IMapper _mapper;
        private readonly ConcurrentQueue<(string, TagValue)> _queue = new();
        private bool _isConnected = true;


        #endregion Fields



        #region Methods

        public override bool Start()
        {
            _messageProducer.OnConnectionStateChange += _messageProducer_OnConnectionStateChange;
            _messageProducer.OnLogError += MessageProducer_OnLogError;
            _cacheModule.OnUncache += _cacheModule_OnUncache;

            _cacheModule.SetConnectionState(true);
            return base.Start();
        }
        public override bool Stop()
        {
            _messageProducer.OnConnectionStateChange -= _messageProducer_OnConnectionStateChange;
            _messageProducer.OnLogError -= MessageProducer_OnLogError;
            _cacheModule.OnUncache -= _cacheModule_OnUncache;
            return base.Stop();
        }


        private void _cacheModule_OnUncache(object? sender, IEnumerable<GenericMessageQueueEntity<TagValue>> e)
        {            
            foreach (var item in e)
            {
                Add(item);
            }
        }
        private void _messageProducer_OnConnectionStateChange(bool obj)
        {
            _isConnected = obj;
            _cacheModule.SetConnectionState(_isConnected);
            if (obj)
            {
                _logger.LogInformation("Connection State - CONNECTED");
            }
            else
            {
                _logger.LogWarning("Connection State - DISCONNECTED");
            }
        }

        public override void Process(GenericMessageQueueEntity<TagValue> entity)
        {
            if (entity.Payload is TagValue tagValue)
            {
                if (_isConnected)
                {
                    _messageProducer.Produce(entity.Topic, tagValue);
                }
                else
                {
                    SendToCache(entity.Topic, tagValue);
                }
            }
        }

        private void MessageProducer_OnLogError(ErrorMessageResult<TagValue> obj)
        {
            _logger.LogError(obj.Message);
            if (obj.Payload is TagValue tagValue)
            {
                SendToCache(obj.Topic, tagValue);
            }
        }

        private void SendToCache(string topic, TagValue tagValue)
        {
            (_cacheModule as IConveyorModule<GenericMessageQueueEntity<TagValue>>)?.Add(
                   new GenericMessageQueueEntity<TagValue>
                   {
                       Payload = tagValue,
                       Topic = topic
                   });
        }

        #endregion Methods

    }
}
