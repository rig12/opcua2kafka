﻿using Converters.TagValues;
using OPCUA2Kafka.Configurations;
using OPCUA2Kafka.Extensions;
using OPCUA2Kafka.Interfaces;
using OPCUA2Kafka.Model;
using OPCUA2Kafka.Services;
using Scheduler.Interfaces;
using Templates.Interfaces;
using Templates.Modules;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITagValueConverter = Converters.Interfaces.ITagValueConverter;

namespace OPCUA2Kafka.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public class OPCUAConnectorModule: ConveyorModule<DataValueTagname>, IOPCUAConnectorModule
    {
        private readonly ITagValueConverter _tagValueConverter;
        private readonly ITagConfigurationManager _tagConfigurationManager;
        private readonly IFilterDuplicateValuesModule _filterDuplicateValuesModule;
        private readonly OPCUAModuleConfiguration _oPCUAModuleConfiguration;

        private List<OPCUAClient> _clients = new();
        private static Func<DateTime> DateTimeNow => () => DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageStatusManager"></param>
        /// <param name="schedulerFactory"></param>
        /// <param name="logger"></param>
        /// <param name="oPCUAModuleConfiguration"></param>
        /// <param name="tagValueConverter"></param>
        /// <param name="tagConfigurationManager"></param>
        /// <param name="filterDuplicateValuesModule"></param>
        public OPCUAConnectorModule(IMessageStatusManager messageStatusManager, 
            ISchedulerFactory schedulerFactory, 
            ILogger<ConveyorModule<DataValueTagname>> logger, 
            OPCUAModuleConfiguration oPCUAModuleConfiguration,
            ITagValueConverter tagValueConverter,
            ITagConfigurationManager tagConfigurationManager,
            IFilterDuplicateValuesModule filterDuplicateValuesModule) 
            : base(messageStatusManager, schedulerFactory, logger, oPCUAModuleConfiguration)
        {
            _tagValueConverter = tagValueConverter;
            _tagConfigurationManager = tagConfigurationManager;
            _filterDuplicateValuesModule = filterDuplicateValuesModule;
            _oPCUAModuleConfiguration = oPCUAModuleConfiguration;
            
            Task.Run(async () =>
            {
                foreach (var item in await CompleteReload())
                {
                    _logger.LogInformation(item);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> CompleteReload()
        {
            var results = Enumerable.Empty<string>().ToList();
            foreach (var taggroup in _tagConfigurationManager.TagConfigurations.Values.GroupBy(x=>x.ServerUrl))
            {
                try
                {
                    var client = new OPCUAClient(_oPCUAModuleConfiguration,
                    Timeout.Infinite,
                    taggroup.Key,
                    taggroup.ToList())
                    {
                        OnNotification = _onNotification
                    };
                    _clients.Add(client);
                    results.Add(string.Join("; ", await client.Run(taggroup.ToList())));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Faile to create OPC UA Client: {ex}");
                }
            }
            return results;           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataValueTagname"></param>
        public override void Process(DataValueTagname dataValueTagname)
        {
            if (!string.IsNullOrEmpty(dataValueTagname.Tagname)
                && dataValueTagname.DataValue is DataValue dataValue
                && dataValue.Value.ToString() is string value)
            {
                var tagvalue = _tagValueConverter.GetTagValue(value, dataValue.SourceTimestamp, dataValueTagname.Tagname, (int)dataValue.StatusCode.Code);
                if (_tagConfigurationManager.TagConfigurations.Values.FirstOrDefault(x => x.ConvertToString() == tagvalue.Tagname) is TagConfigurationEntity tagconfig)
                {
                    tagvalue.TimeStampUtc = DateTimeNow().ToUniversalTime();
                    tagvalue.OpcQuality = (int)dataValue.StatusCode.Code;
                    tagvalue.TagId = tagconfig.Id;
                    tagvalue.Tagname = tagconfig.ConvertToString();

                    (_filterDuplicateValuesModule as ConveyorModule<TagValue>)?.Add(tagvalue);
                }
            }
        }


        private void _onNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Add(new DataValueTagname { DataValue = value, Tagname = item.DisplayName });
            }
        }
    }
}
