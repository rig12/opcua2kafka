using GPNA.Converters.Interfaces;
using GPNA.Converters.TagValues;
using GPNA.DataModel.Integration;
using GPNA.MessageQueue.Entities;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.OPCUA2Kafka.Extensions;
using GPNA.OPCUA2Kafka.Interfaces;
using GPNA.OPCUA2Kafka.Model;
using GPNA.OPCUA2Kafka.Services;
using GPNA.Scheduler.Interfaces;
using GPNA.Templates.Interfaces;
using GPNA.Templates.Modules;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITagValueConverter = GPNA.Converters.Interfaces.ITagValueConverter;

namespace GPNA.OPCUA2Kafka.Modules
{
    public class OPCUAConnectorModule: ConveyorModule<DataValueTagname>, IOPCUAConnectorModule
    {
        private readonly ITagValueConverter _tagValueConverter;
        private readonly ITagConfigurationManager _tagConfigurationManager;
        private readonly IFilterDuplicateValuesModule _filterDuplicateValuesModule;
        private readonly OPCUAConfiguration _oPCUAConfiguration;
        private OPCUAClient? _client;
        private static Func<DateTime> DateTimeNow => () => DateTime.Now;

        public OPCUAConnectorModule(IMessageStatusManager messageStatusManager, 
            ISchedulerFactory schedulerFactory, 
            ILogger<ConveyorModule<DataValueTagname>> logger, 
            OPCUAConfiguration oPCUAConfiguration,
            ITagValueConverter tagValueConverter,
            ITagConfigurationManager tagConfigurationManager,
            IFilterDuplicateValuesModule filterDuplicateValuesModule) 
            : base(messageStatusManager, schedulerFactory, logger, oPCUAConfiguration)
        {
            _tagValueConverter = tagValueConverter;
            _tagConfigurationManager = tagConfigurationManager;
            _filterDuplicateValuesModule = filterDuplicateValuesModule;
            _oPCUAConfiguration = oPCUAConfiguration;
            //tagConfigurationManager.Load();

            Task.Run(()=> CompleteReload());
        }


        public async Task<string> CompleteReload()
        //Task<string>
        {
            _client = new(_oPCUAConfiguration.EndpointURL,
                true,
                Timeout.Infinite,
                _oPCUAConfiguration.DefaultPublishingInterval,
                _tagConfigurationManager)
            {
                OnNotification = _onNotification
            };

            //            var result = await _client.Run();
            return string.Join("; ", await _client.Run());


            //return result?.ToString() ?? string.Empty;
        }

        public override void Process(DataValueTagname dataValueTagname)
        {
            if (!string.IsNullOrEmpty(dataValueTagname.Tagname)
                && dataValueTagname.DataValue is DataValue dataValue
                && dataValue.Value.ToString() is string value)
            {
                var tagvalue = _tagValueConverter.GetTagValue(value, dataValue.SourceTimestamp, dataValueTagname.Tagname, (int)dataValue.StatusCode.Code);
                if (_tagConfigurationManager.TagConfigurations.Values.FirstOrDefault(x=>x.Tagname==tagvalue.Tagname) is TagConfigurationEntity tagconfig)
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
                //Console.WriteLine("{0}: {1}, {2}, {3}", item.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode);
            }
        }
    }
}
