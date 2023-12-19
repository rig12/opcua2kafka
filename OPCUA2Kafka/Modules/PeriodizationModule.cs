
namespace GPNA.OPCUA2Kafka.Modules
{
    using global::Converters.TagValues;
    using global::Extensions.Types;
    using global::MessageQueue.Entities;
    using global::OPCUA2Kafka.Configurations;
    using global::OPCUA2Kafka.Extensions;
    using global::OPCUA2Kafka.Interfaces;
    using global::OPCUA2Kafka.Model;
    using global::Scheduler.Interfaces;
    using global::Templates.Interfaces;
    using global::Templates.Modules;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace GPNA.OPCUA2Kafka.Modules
    {
        public class PeriodizationModule : ConveyorModule<TagValue>, IPeriodizationModule, ISchedulerManager
        {
            #region fields&props

            private readonly ConcurrentDictionary<string, TagValue> _currentPeriodValues = new();
            private readonly ConcurrentDictionary<string, bool> _toSendCurrentValues = new();
            private IEnumerable<string>? _zerotags;

            private readonly ITagConfigurationManager _tagConfigurationManager;
            private readonly IConveyorModule<GenericMessageQueueEntity<TagValue>> _kafkaSendingModule;
            private readonly ISchedulerFactory _schedulerFactory;
            private List<IScheduler>? _schedulers;
            #endregion


            #region ctor

            public PeriodizationModule(IMessageStatusManager messageStatusManager,
                PeriodizationConfiguration periodizationConfiguration,
                ILogger<PeriodizationModule> logger,
                ITagConfigurationManager tagConfigurationManager,
                ISchedulerFactory schedulerFactory,
                IConveyorModule<GenericMessageQueueEntity<TagValue>> kafkaSendingModule)
                : base(messageStatusManager, schedulerFactory, logger, periodizationConfiguration)
            {
                _tagConfigurationManager = tagConfigurationManager;
                _kafkaSendingModule = kafkaSendingModule;
                _schedulerFactory = schedulerFactory;
                foreach (var item in ReloadConfig())
                {
                    logger.LogTrace(item);
                }
                
            }

            #endregion


            #region ReloadConfig

            public IEnumerable<string> ReloadConfig()
            {
                _zerotags = _tagConfigurationManager.TagConfigurations.Where(x => x.Value.Period == 0).Select(x => x.Value.ConvertToString()).ToList();

                _schedulers = new();

                foreach (var item in _tagConfigurationManager.TagConfigurations.Where(x => x.Value.Period > 0).GroupBy(x => x.Value.Period))
                {
                    var scheduler = _schedulerFactory.GetScheduler();

                    var schedulename = $"SendData_{item.Key}sec";

                    scheduler.AndThen(schedulename, async () =>
                    {
                        await SendData(item.Select(x => x.Value).ToList());
                    });
                    scheduler.ToRunEvery(item.Key).Seconds();
                    _schedulers?.Add(scheduler);
                    scheduler.Start();
                    yield return schedulename;
                }
            }
            #endregion


            #region ConveyorModule

            public override void Process(TagValue entity)
            {
                if (entity.Tagname == null)
                {
                    return;
                }

                if (_zerotags != null && _zerotags.Any(x => x == entity.Tagname))
                {
                    if (_tagConfigurationManager.TagConfigurations.TryGetValue(entity.Tagname, out var tagconfig))
                    {                        
                        var message = new GenericMessageQueueEntity<TagValue> { Topic = tagconfig.Topic, Payload = entity };

                        _kafkaSendingModule.Add(message);
                    }
                    else
                    {
                        _logger.LogWarning($"{entity.Tagname} is not found in configuration; message - {JsonConvert.SerializeObject(entity)}");
                    }
                }
                else
                {
                    if (!_currentPeriodValues.TryAdd(entity.Tagname, entity))
                    {
                        _currentPeriodValues[entity.Tagname] = entity;
                    }
                    if (!_toSendCurrentValues.TryAdd(entity.Tagname, true))
                    {
                        _toSendCurrentValues[entity.Tagname] = true;
                    }
                }
            }

            public void StartSchedulers()
            {
                if (_schedulers != null)
                {
                    foreach (var item in _schedulers)
                    {
                        item.Start();
                    }
                }
            }

            public void StopSchedulers()
            {
                if (_schedulers != null)
                {
                    foreach (var item in _schedulers)
                    {
                        item.Stop();
                    }
                }
            }

            private Task SendData(List<TagConfigurationEntity> list)
            {
                return Task.Run(() =>
                {
                    foreach (var item in _currentPeriodValues)
                    {
                        if (_toSendCurrentValues.TryGetValue(item.Key, out var tosend)
                            && tosend
                            && item.Value.Tagname != null
                            && list.Any(x => x.ConvertToString().EqualsInsensitive(item.Value.Tagname)))
                        {
                            _toSendCurrentValues[item.Key] = false;

                            var tagconfig = _tagConfigurationManager.TagConfigurations[item.Value.Tagname];

                            var tagvalue = item.Value;
                                                        
                            var message = new GenericMessageQueueEntity<TagValue> { Topic = tagconfig.Topic, Payload = tagvalue };
                            (_kafkaSendingModule as IConveyorModule<GenericMessageQueueEntity<TagValue>>)?.Add(message);
                        }
                    }
                });
            }


            #endregion
        }
    }
}
