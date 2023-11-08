
using GPNA.Converters.TagValues;
using GPNA.DataModel.Integration;
using GPNA.Scheduler.Interfaces;
using GPNA.Templates.Interfaces;
using GPNA.Templates.Modules;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.OPCUA2Kafka.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GPNA.Converters.Interfaces;

namespace GPNA.OPCUA2Kafka.Modules
{
    public class FilterDuplicateValuesModule_ : ConveyorModule<TagEvent>, IFilterDuplicateValuesModule
    {
        private readonly FilterConfiguration _filterConfiguration;
        private readonly ITagConfigurationManager _tagConfigurationManager;
        private readonly IPeriodizationModule _periodizationModule;
        private readonly Interfaces.ITagValueConverter _tagValueConverter;
        private readonly ConvertConfiguration _convertConfiguration;

        private readonly ConcurrentDictionary<string, string?> _currentStrValues = new();
        private readonly ConcurrentDictionary<string, TagValue> _currentValues = new();


        #region ctor


        public FilterDuplicateValuesModule_(IMessageStatusManager messageStatusManager,
            FilterConfiguration filterConfiguration,
            ConvertConfiguration convertConfiguration,
            Interfaces.ITagValueConverter tagValueConverter,
            IPeriodizationModule periodizationModule,
            ILogger<FilterDuplicateValuesModule_> logger,
            ITagConfigurationManager tagConfigurationManager,
            ISchedulerFactory schedulerFactory
            )
            : base(messageStatusManager, schedulerFactory, logger, filterConfiguration)
        {
            _tagValueConverter = tagValueConverter;
            _convertConfiguration = convertConfiguration;
            _periodizationModule = periodizationModule;
            _filterConfiguration = filterConfiguration;
            _tagConfigurationManager = tagConfigurationManager;
        }

        #endregion

        public IReadOnlyDictionary<string, TagValue> CurrentValues => _currentValues;
        public override void Process(TagEvent lmxtagevent)
        {
            var tosend = true;
            TagValue tagvalue;
            if (_convertConfiguration.ConvertValuesToDouble)
            { 
                tagvalue = _tagValueConverter.GetTagValueDouble(lmxtagevent, _convertConfiguration.DatetimeParseFormat); 
            }
            else
            {
                tagvalue = _tagValueConverter.GetTagValue(lmxtagevent, _convertConfiguration.DatetimeParseFormat);                
            }
            tagvalue.TagId = _tagConfigurationManager.GetByTagName(tagvalue.Tagname)?.Id ?? 0;
            if(string.IsNullOrEmpty(lmxtagevent.Name))
            {
                _logger.LogWarning($"NO TAGNAME - {JsonConvert.SerializeObject(lmxtagevent)}");
                return;
            }
            if (!_currentValues.TryAdd(lmxtagevent.Name, tagvalue))
            {
                _currentValues[lmxtagevent.Name] = tagvalue;
            }
            if (tagvalue is TagValueNull && !_convertConfiguration.ToSendNullValues)
            {
                return;
            }
            if (_filterConfiguration.IsEnabled == true)
            {
                var value = lmxtagevent.Value?.ToString();

                if (_currentStrValues.TryGetValue(lmxtagevent.Name, out var archivedStrValue))
                {
                    if (string.Equals(archivedStrValue, value))
                    {
                        tosend = false;
                    }
                    else
                    {
                        _currentStrValues[lmxtagevent.Name] = value;
                    }
                }
                else
                {
                    _currentStrValues.AddOrUpdate(lmxtagevent.Name, value, (key, value) => value);
                }
            }
            if (tosend)
            {
                if (_periodizationModule.IsStarted)
                {
                    (_periodizationModule as IConveyorModule<TagValue>)?.Add(tagvalue);
                }
            }
        }
    }
}