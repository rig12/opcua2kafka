using Opc.Ua.Client;
using Opc.Ua;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using GPNA.OPCUA2Kafka.Model;
using Opc.Ua.Configuration;
using GPNA.OPCUA2Kafka.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GPNA.OPCUA2Kafka.Services
{
    public class OPCUAClient : IOPCUAClient
    {
        private const int ReconnectPeriod = 10;
        private Session? session;
        private SessionReconnectHandler? reconnectHandler;
        private readonly string _endpointURL;
        private readonly int _clientRunTime = Timeout.Infinite;
        private readonly ILogger<OPCUAClient> _logger;
        private static bool _autoAccept = false;
        private readonly int _defaultPublishingInterval;
        private static ExitCode exitCode;
        public IEnumerable<ITagConfiguration> TagConfigurations { get; set; }

        public OPCUAClient(string endpointURL, bool autoAccept, int stopTimeout, int defaultPublishingInterval, IEnumerable<ITagConfiguration> tagConfigurations)
        {
            _endpointURL = endpointURL;
            _autoAccept = autoAccept;
            _defaultPublishingInterval = defaultPublishingInterval;
            _clientRunTime = stopTimeout <= 0 ? Timeout.Infinite : stopTimeout * 1000;
            TagConfigurations=tagConfigurations;
            _logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<OPCUAClient>();
        }


        public async Task<IEnumerable<string>> Run()
        {
            try
            {
                await ConsoleSampleClient();
            }
            catch (Exception ex)
            {
                Utils.Trace("ServiceResultException:" + ex.Message);
                _logger.LogInformation("Exception: {0}", ex.Message);
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

        public static ExitCode ExitCode { get => exitCode; }
        public MonitoredItemNotificationEventHandler? OnNotification { get; set; }

        private async Task ConsoleSampleClient()
        {
            _logger.LogTrace("1 - Create an Application Configuration.");
            exitCode = ExitCode.ErrorCreateApplication;

            ApplicationInstance application = new()
            {
                ApplicationName = "UA Core Sample Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = Utils.IsRunningOnMono() ? "GPNA.OPCUA2Kafka.Mono" : "GPNA.OPCUA2Kafka.Config"
            };

            // load the application configuration.
            //$"{application.ConfigSectionName}.xml",
            ApplicationConfiguration config = await application.LoadApplicationConfiguration($"{application.ConfigSectionName}.xml", true);

            //выключаем проверку сертификатов
            config.CertificateValidator.AutoAcceptUntrustedCertificates = true;
            config.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;

            // check the application certificate.
            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _autoAccept = true;
                }
                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            else
            {
                _logger.LogWarning("WARN: missing application certificate, using unsecure connection.");
            }

            _logger.LogTrace("2 - Discover endpoints of {0}.", _endpointURL);
            exitCode = ExitCode.ErrorDiscoverEndpoints;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(_endpointURL, haveAppCertificate, 15000);
            _logger.LogInformation("    Selected endpoint uses: {0}",
                selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

            _logger.LogTrace("3 - Create a session with OPC UA server.");
            exitCode = ExitCode.ErrorCreateSession;
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            session = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            // register keep alive handler
            session.KeepAlive += Client_KeepAlive;

            _logger.LogTrace("4 - Browse the OPC UA server namespace.");
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

            _logger.LogTrace(" DisplayName, BrowseName, NodeClass");
            foreach (var rd in references)
            {
                _logger.LogTrace(" {0}, {1}, {2}", rd.DisplayName, rd.BrowseName, rd.NodeClass);
                ReferenceDescriptionCollection nextRefs;
                byte[] nextCp;
                session.Browse(
                    null,
                    null,
                    ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris),
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out nextCp,
                    out nextRefs);

                foreach (var nextRd in nextRefs)
                {
                    _logger.LogTrace("   + {0}, {1}, {2}", nextRd.DisplayName, nextRd.BrowseName, nextRd.NodeClass);
                }
            }

            _logger.LogTrace("5 - Create a subscription with publishing interval of 1 second.");
            exitCode = ExitCode.ErrorCreateSubscription;

            foreach (var period in TagConfigurations.GroupBy(x => x.Period))
            {
                var subscription = new Subscription(session.DefaultSubscription)
                {
                    PublishingInterval = period.Key > 0 ? period.Key : _defaultPublishingInterval
                };

                _logger.LogTrace("6 - Add a list of items (server current time and status) to the subscription.");
                exitCode = ExitCode.ErrorMonitoredItem;
                if (TagConfigurations?.Count() > 0)
                {
                    var list = new List<MonitoredItem>();
                    foreach (var item in period.AsEnumerable())
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
                        _logger.LogTrace($"item {item.DisplayName} is added to subscription");
                    }
                    _logger.LogTrace($"7 - Add the subscription ({subscription.PublishingInterval}) to the session.");
                    exitCode = ExitCode.ErrorAddSubscription;
                    session.AddSubscription(subscription);
                    subscription.Create();
                }
            }

            _logger.LogTrace("8 - Running...Press Ctrl-C to exit...");
            exitCode = ExitCode.ErrorRunning;

        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                _logger.LogInformation("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (reconnectHandler == null)
                {
                    _logger.LogInformation("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
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
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = _autoAccept;
                if (_autoAccept)
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
