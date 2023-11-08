using AutoMapper;
using Converters.TagValues;
using MessageQueue.Entities;
using Repository;
using Scheduler.Interfaces;
using Templates.Interfaces;
using Templates.Modules;
using OPCUA2Kafka.Configurations;
using OPCUA2Kafka.Interfaces;
using OPCUA2Kafka.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPCUA2Kafka.Modules
{
    public class CacheModule : ConveyorModuleIEnumerable<GenericMessageQueueEntity<TagValue>>, ICacheModule<GenericMessageQueueEntity<TagValue>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IScheduler _sendingScheduler;

        public CacheModule(IMessageStatusManager messageStatusManager
            , ISchedulerFactory schedulerFactory
            , ILogger<ConveyorModule<IEnumerable<GenericMessageQueueEntity<TagValue>>>> logger
            , CacheConfiguration cacheConfiguration
            , IRepositoryFactory repositoryFactory
            , IMapper mapper
            )
            : base(messageStatusManager, schedulerFactory, logger, cacheConfiguration)
        {
            _mapper = mapper;
            _repositoryFactory = repositoryFactory;
            _sendingScheduler = schedulerFactory.GetScheduler();
            _sendingScheduler.AndThen("CacheSendingScheduler", async () =>
            {
                try
                {
                    using var repository = _repositoryFactory.GetRepository<TagValueMessageEntity>();
                    var entites = repository.Get(cacheConfiguration.HandlersLimit).ToList();
                    if (entites.Count < 1)
                    {
                        repository.Shrink();
                        _sendingScheduler.Stop();
                        return;
                    }
                    var mapped = _mapper.Map<IEnumerable<GenericMessageQueueEntity<TagValue>>>(entites);
                    OnUncache?.Invoke(this, mapped);
                    foreach (var item in entites)
                    {
                        repository.Delete(item.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                await System.Threading.Tasks.Task.CompletedTask;

            });
            _sendingScheduler.ToRunEvery(cacheConfiguration.CacheSendIntervalSec).Seconds();
        }

        public event EventHandler<IEnumerable<GenericMessageQueueEntity<TagValue>>>? OnUncache;

        public override void Process(IEnumerable<GenericMessageQueueEntity<TagValue>> entities)
        {
            var mappedentities = _mapper.Map<IEnumerable<TagValueMessageEntity>>(entities);
            if (mappedentities is IEnumerable<TagValueMessageEntity> notnullmappedentities && notnullmappedentities.Any())
            {
                using var repository = _repositoryFactory.GetRepository<TagValueMessageEntity>();
                repository.Add(notnullmappedentities);
            }
        }

        public void SetConnectionState(bool value)
        {
            if (value)
            {
                _sendingScheduler.Start();
            }
            else
            {
                _sendingScheduler.Stop();
            }
        }
    }
}

