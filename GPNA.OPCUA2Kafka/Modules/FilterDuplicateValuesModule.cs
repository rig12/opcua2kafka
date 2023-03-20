
using GPNA.Converters.Model;
using GPNA.Converters.TagValues;
using GPNA.MessageQueue.Entities;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.OPCUA2Kafka.Interfaces;
using GPNA.OPCUA2Kafka.Services;
using GPNA.Scheduler.Interfaces;
using GPNA.Templates.Interfaces;
using GPNA.Templates.Modules;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GPNA.OPCUA2Kafka.Modules
{
    public class FilterDuplicateValuesModule : ConveyorModule<TagValue>, IFilterDuplicateValuesModule
    {
        private readonly FilterConfiguration _filterConfiguration;
        private readonly IConveyorModule<GenericMessageQueueEntity<TagValue>> _kafkaSendingModule;
        private readonly ITagConfigurationManager _tagConfigurationManager;

        //private readonly IPeriodizationModule _periodizationModule;
        private readonly ConvertConfiguration _convertConfiguration;

        private readonly ConcurrentDictionary<string, string?> _currentStrValues = new();
        private readonly ConcurrentDictionary<string, TagValue> _currentValues = new();
                

        #region ctor


        public FilterDuplicateValuesModule(IMessageStatusManager messageStatusManager,
            FilterConfiguration filterConfiguration,
            ConvertConfiguration convertConfiguration,
            //IPeriodizationModule periodizationModule,
            IConveyorModule<GenericMessageQueueEntity<TagValue>> kafkaSendingModule,
            ILogger<FilterDuplicateValuesModule> logger,
            ITagConfigurationManager tagConfigurationManager,
            ISchedulerFactory schedulerFactory
            )
            : base(messageStatusManager, schedulerFactory, logger, filterConfiguration)
        {            
            _convertConfiguration = convertConfiguration;
            //_periodizationModule = periodizationModule;
            _filterConfiguration = filterConfiguration;
            _kafkaSendingModule = kafkaSendingModule;
            _tagConfigurationManager = tagConfigurationManager;
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyDictionary<string, TagValue> CurrentValues => _currentValues;
        public override void Process(TagValue tagvalue)
        {
            var tosend = true;

            if (!_currentValues.TryAdd(tagvalue.Tagname, tagvalue))
            {
                _currentValues[tagvalue.Tagname] = tagvalue;
            }
            if (tagvalue is TagValueNull && !_convertConfiguration.ToSendNullValues)
            {
                return;
            }
            if (_filterConfiguration.IsEnabled == true)
            {
                var value = tagvalue.GetValue()?.ToString();

                if (_currentStrValues.TryGetValue(tagvalue.Tagname, out var archivedStrValue))
                {
                    if (string.Equals(archivedStrValue, value))
                    {
                        tosend = false;
                    }
                    else
                    {
                        _currentStrValues[tagvalue.Tagname] = value;
                    }
                }
                else
                {
                    _currentStrValues.AddOrUpdate(tagvalue.Tagname, value, (key, value) => value);
                }
            }
            if (tosend && _tagConfigurationManager.TagConfigurations.TryGetValue(tagvalue.Tagname, out var tagconfig))
            {
                _kafkaSendingModule.Add(new GenericMessageQueueEntity<TagValue>
                {
                    Payload = tagvalue,
                    Topic = tagconfig.Topic
                });
            }
        }
    }
}