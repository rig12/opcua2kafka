using GPNA.DataModel.EMail;
using GPNA.OPCUA2Kafka.Configurations;
using GPNA.OPCUA2Kafka.Interfaces;
using GPNA.OPCUA2Kafka.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPNA.OPCUA2Kafka.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class OPCUAClient : IOPCUAClient
    {
        private const int ReconnectPeriod = 10;
        private ISession? session;
        private SessionReconnectHandler? reconnectHandler;
        private readonly OPCUAModuleConfiguration _oPCUAModuleConfiguration;
        private readonly int _clientRunTime = Timeout.Infinite;
        //private readonly ITagConfigurationManager _tagConfigurationManager;

        private readonly ILogger<OPCUAClient> _logger;
        private readonly string _endpointurl;
        private readonly IEnumerable<TagConfigurationEntity> _tags;

        //private readonly OPCUAConfiguration _oPCUAConfiguration;
        private static ExitCode exitCode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oPCUAModuleConfiguration"></param>
        /// <param name="stopTimeout"></param>
        /// <param name="tags"></param>
        public OPCUAClient(OPCUAModuleConfiguration oPCUAModuleConfiguration, int stopTimeout,string endpointurl, IEnumerable<TagConfigurationEntity> tags)
        {
            _oPCUAModuleConfiguration = oPCUAModuleConfiguration;
            _clientRunTime = stopTimeout <= 0 ? Timeout.Infinite : stopTimeout * 1000;
            //_tagConfigurationManager = tagConfigurationManager;
            _logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<OPCUAClient>();
            _endpointurl = endpointurl;
            _tags = tags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> Run(IEnumerable<TagConfigurationEntity> tags)
        {
            try
            {
                await ConsoleSampleClient(tags);
            }
            catch(AggregateException ae)
            {
                _logger.LogWarning(ae.ToString());

                foreach (var item in ae.InnerExceptions)
                {
                    _logger.LogWarning(item.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = $"ServiceResultException:{ex}";
                Utils.Trace(msg);
                _logger.LogWarning(msg);
                return new string[] { ex.ToString() };
            }

            ManualResetEvent quitEvent = new(false);
            try
            {
                Console.CancelKeyPress += (sender, eArgs) =>
                {
                    quitEvent.Set();
                    eArgs.Cancel = true;
                };
            }
            catch
            {
            }

            // wait for timeout or Ctrl-C
            quitEvent.WaitOne(_clientRunTime);

            // return error conditions
            if (session?.KeepAliveStopped == true)
            {
                exitCode = ExitCode.ErrorNoKeepAlive;
                return new string[] { exitCode.ToString() };
            }

            exitCode = ExitCode.Ok;
            return new string[] { exitCode.ToString() };
        }

        /// <summary>
        /// 
        /// </summary>
        public static ExitCode ExitCode { get => exitCode; }

        /// <summary>
        /// 
        /// </summary>
        public MonitoredItemNotificationEventHandler? OnNotification { get; set; }

        private async Task ConsoleSampleClient(IEnumerable<TagConfigurationEntity> tags)
        {
            _logger.LogTrace("1 - Create an Application Configuration.");
            exitCode = ExitCode.ErrorCreateApplication;

            ApplicationInstance application = new()
            {
                ApplicationName = "UA Core Sample Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = Utils.IsRunningOnMono() ? "GPNA.OPCUA2Kafka.Mono" : "GPNA.OPCUA2Kafka.Config",
                
            };

            // load the application configuration.
            //$"{application.ConfigSectionName}.xml",
            ApplicationConfiguration config = await application.LoadApplicationConfiguration($"{application.ConfigSectionName}.xml", true);
            config.CertificateValidator.CertificateValidation+= (s, e) => { e.Accept = true; };
            //выключаем проверку сертификатов
            config.CertificateValidator.AutoAcceptUntrustedCertificates = true;
            config.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;
            
            config.CertificateValidator=new CertificateValidator { AutoAcceptUntrustedCertificates=true};
            // check the application certificate.
            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
          
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }
            
            if (haveAppCertificate)
            {
                config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                config.SecurityConfiguration.AutoAcceptUntrustedCertificates = _oPCUAModuleConfiguration.AutoAccept;
                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            else
            {
                _logger.LogWarning("WARN: missing application certificate, using unsecure connection.");
            }

            _logger.LogInformation("2 - Discover endpoints of {0}.", _endpointurl);
            exitCode = ExitCode.ErrorDiscoverEndpoints;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(_endpointurl, !_oPCUAModuleConfiguration.SecurityNone, 15000);
            
            _logger.LogInformation("    Selected endpoint uses: {0}",
                selectedEndpoint.SecurityPolicyUri[(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1)..]);
            
            _logger.LogInformation(JsonConvert.SerializeObject(selectedEndpoint));

            if (_oPCUAModuleConfiguration.SecurityNone)
            {
                selectedEndpoint.SecurityMode = MessageSecurityMode.None;
            }
            _logger.LogInformation("3 - Create a session with OPC UA server.");
            exitCode = ExitCode.ErrorCreateSession;
            

            _logger.LogInformation(JsonConvert.SerializeObject(config.SecurityConfiguration));
            var endpointConfiguration = EndpointConfiguration.Create(config);
            
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            session = await Session.Create(config, endpoint, true, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            // register keep alive handler
            session.KeepAlive += Client_KeepAlive;

            _logger.LogInformation("4 - Browse the OPC UA server namespace.");
            exitCode = ExitCode.ErrorBrowseNamespace;
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;

            references = session.FetchReferences(ObjectIds.ObjectsFolder);

            session.Browse(
                null,
                null,
                ObjectIds.ObjectsFolder,
                0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                out continuationPoint,
                out references);

            _logger.LogInformation(" DisplayName, BrowseName, NodeClass");
            foreach (var rd in references)
            {
                _logger.LogInformation(" {0}, {1}, {2}", rd.DisplayName, rd.BrowseName, rd.NodeClass);
                session.Browse(
                    null,
                    null,
                    ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris),
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out byte[] nextCp,
                    out ReferenceDescriptionCollection nextRefs);

                foreach (var nextRd in nextRefs)
                {
                    _logger.LogInformation("   + {0}, {1}, {2}", nextRd.DisplayName, nextRd.BrowseName, nextRd.NodeClass);
                }
            }

            _logger.LogInformation("5 - Create a subscription with publishing interval depends on TagConfiguration.Period");
            exitCode = ExitCode.ErrorCreateSubscription;

            foreach (var periodgroup in _tags.GroupBy(x => x.Period))
            {
                var subscription = new Subscription(session.DefaultSubscription)
                {
                    PublishingInterval = periodgroup.Key > 0 ? periodgroup.Key : _oPCUAModuleConfiguration.DefaultPublishingInterval
                };

                _logger.LogInformation("6 - Add a list of items (server current time and status) to the subscription.");
                exitCode = ExitCode.ErrorMonitoredItem;
                if (periodgroup.AsEnumerable().Count() > 0)
                {
                    var list = new List<MonitoredItem>();
                    foreach (var item in periodgroup.AsEnumerable())
                    {
                        var monitoreditem = new MonitoredItem(subscription.DefaultItem)
                        {
                            DisplayName = item.Tagname,
                            StartNodeId = item.Node
                        };
                        monitoreditem.Notification += OnNotification;
                        list.Add(monitoreditem);
                    }

                    subscription.AddItems(list);
                    foreach (var item in list)
                    {
                        _logger.LogInformation($"item {item.DisplayName}(node='{item.StartNodeId}') is added to subscription");
                    }

                    _logger.LogInformation($"7 - Add the subscription ({subscription.PublishingInterval}) to the session.");
                    exitCode = ExitCode.ErrorAddSubscription;
                    session.AddSubscription(subscription);
                    subscription.Create();
                }
            }

            _logger.LogInformation("8 - Running...Press Ctrl-C to exit...");
            exitCode = ExitCode.ErrorRunning;
        }

        private void Client_KeepAlive(ISession session, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                _logger.LogInformation("{0} {1}/{2}", e.Status, session.OutstandingRequestCount, session.DefunctRequestCount);

                if (reconnectHandler == null)
                {
                    _logger.LogInformation("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(session, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }


        private void Client_ReconnectComplete(object? sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }
            if (reconnectHandler != null)
            {
                session = reconnectHandler?.Session;

                reconnectHandler?.Dispose();
                reconnectHandler = null;

                _logger.LogInformation("--- RECONNECTED ---");
            }
        }


        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(e));

            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = _oPCUAModuleConfiguration.AutoAccept;
                if (_oPCUAModuleConfiguration.AutoAccept)
                {
                    _logger.LogInformation("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    _logger.LogInformation("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }

    }
}
