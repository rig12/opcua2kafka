namespace GPNA.OPCUA2Kafka
{
    #region Using
    using AutoMapper;
    using Configurations;
    using Delegates;
    using GPNA.Converters.TagValues;
    using GPNA.Extensions.Configurations;
    using GPNA.MessageQueue.Configurations;
    using GPNA.MessageQueue.Entities;
    using GPNA.MessageQueue.Extensions;
    using GPNA.OPCUA2Kafka.Extensions;
    using GPNA.OPCUA2Kafka.Model;
    using GPNA.Repository;
    using GPNA.SuiteLinkConnector.Interfaces;
    using GPNA.SuiteLinkConnector.Services;
    using GPNA.Templates.Constants;
    using GPNA.Templates.Interfaces;
    using GPNA.Templates.Messages;
    using Hellang.Middleware.ProblemDetails;
    using Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Services;
    using System;
    using System.Reflection;

    #endregion Using

    public class Startup
    {
        #region Constructor

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion Constructos


        #region Field

        private ILogger<Startup>? _logger;

        private readonly IConfiguration _configuration;
        private InitializationModuleConfiguration? _initializationModuleConfig;

        private IConveyorModule<GenericMessageQueueEntity<TagValue>>? _kafkaSendingModule;
        private IOPCUAConnectorModule? _oPCUAConnectorModule;
        //private IPeriodizationModule? _periodizationModule;
        private IFilterDuplicateValuesModule? _filterDuplicateValuesModule;
        private ICacheModule<GenericMessageQueueEntity<TagValue>>? _cacheModule;
        private Interfaces.ITagValueConverter? _tagValueConverter;
        #endregion Fields

        #region Methods
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            var assembly = Assembly.GetExecutingAssembly();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());


                var tagValueMessageEntityMap = cfg.CreateMap<GenericMessageQueueEntity<TagValue>, TagValueMessageEntity>()
                    .IncludeMembers(x => x.Payload);

                tagValueMessageEntityMap.ForMember("Topic", opt => opt.MapFrom<TagValueTopicResolver>());
                tagValueMessageEntityMap.ForMember("Value", opt => opt.MapFrom<TagValueResolver>());

                tagValueMessageEntityMap.ForMember("Type", opt => opt.MapFrom(src =>
                    src.Payload != null
                    ? src.Payload.GetType().AssemblyQualifiedName
                    : typeof(TagValueNull).ToString()));

                var tagValueMessageEntityMapReverse = tagValueMessageEntityMap.ReverseMap()
                    .ForMember(dest => dest.Payload, src => src.MapFrom(x => x));

                var tagValueMap = cfg.CreateMap<TagValue, TagValueMessageEntity>();

                var tagValueMapReverse = tagValueMap.ReverseMap();

                tagValueMapReverse.ConstructUsing(src => src.Type != null
                    ? (Type.GetType(src.Type).Constructor()(new object[] { }) as TagValue)
                        ?? new TagValueNull()
                    : new TagValueNull());

                tagValueMapReverse.AfterMap((src, dest) =>
                {
                    if (_tagValueConverter is Interfaces.ITagValueConverter tagValueConverter)
                    {
                        if (_tagValueConverter.GetTagValue(src.Value) is object value)
                        {
                            dest.SetTagValue(value);
                        }
                    }
                });
            });
            var kafkaConfiguration = _configuration.GetSection<KafkaConfiguration>();
            services.AddSingleton(s => config.CreateMapper());

            void producerConfigAction(ProducerConfiguration configuration)
            {
                configuration.SetBootstrapServers(kafkaConfiguration.Brokers);
                configuration.SetClientId(kafkaConfiguration.ClientId);
                configuration.SetMessageTimeoutMs(kafkaConfiguration.MessageTimeoutMs);
                configuration.SetEnableDeliveryReports(kafkaConfiguration.EnableDeliveryReports);
                configuration.SetQueueBufferingMaxMs(kafkaConfiguration.QueueBufferingMaxMs);
                configuration.SetBatchNumMessages(kafkaConfiguration.BatchNumMessages);
            }

            services.AddMessageQueueAsyncConfirmProducer<TagValue>(producerConfigAction);
            
            services.AddSingleton(_configuration.GetSection<InitializationModuleConfiguration>());
            services.AddSingleton(_configuration.GetSection<KafkaConfiguration>());
            services.AddSingleton(_configuration.GetSection<OPCUAConfiguration>());
            services.AddSingleton(_configuration.GetSection<ConvertConfiguration>());
            services.AddSingleton(_configuration.GetSection<CacheConfiguration>());
            //services.AddSingleton(_configuration.GetSection<PeriodizationConfiguration>());
            services.AddSingleton(_configuration.GetSection<FilterConfiguration>());
            services.AddSingleton<Scheduler.Interfaces.ISchedulerFactory, Scheduler.Services.SchedulerFactory>();
            services.AddSingleton<IConveyorModule<GenericMessageQueueEntity<TagValue>>, KafkaSendingModule>();
            services.AddSingleton<IOPCUAConnectorModule, OPCUAConnectorModule>();
            services.AddSingleton<IFilterDuplicateValuesModule, FilterDuplicateValuesModule>();
            services.AddTransient<IMessageStatusManager, MessageStatusManager>();
            services.AddSingleton<IRepositoryFactory, LiteDbRepositoryFactory>();
            services.AddTransient<IConnector, Connector>();
            services.AddSingleton<ITagConfigurationManager, TagConfigurationManager>();
            services.AddSingleton<Interfaces.ITagValueConverter, Services.TagValueConverter>();
            services.AddSingleton<Converters.Interfaces.ITagValueConverter, Converters.Services.TagValueConverter>();
            //services.AddSingleton<IPeriodizationModule, PeriodizationModule>();
            services.AddSingleton<ICacheModule<GenericMessageQueueEntity<TagValue>>, CacheModule>();
            services.AddProblemDetails(ConfigureProblemDetails);
            

            services.AddSwaggerGen(options => options.SwaggerDocVersion("v1",
                assembly?.GetName()?.Name, assembly?.GetName()?.Version?.ToString()));

            services.ConfigureSwaggerGen(options =>
            {
                var programPath = AppContext.BaseDirectory;
                options.IncludeXmlFile(programPath, $"{this.GetType().Namespace}.xml");
            });
        }

        private static void StartModule(IActiveModule? module, bool toStart)
        {
            if (toStart)
            {
                module?.Start();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            ILogger<Startup> logger,
            InitializationModuleConfiguration initializationModuleConfig,
            IHostApplicationLifetime applicationLifetime,
            IWebHostEnvironment env,
            IConveyorModule<GenericMessageQueueEntity<TagValue>> kafkaSendingModule,
            IOPCUAConnectorModule oPCUAConnectorModule,
            //IPeriodizationModule periodizationModule,
            IFilterDuplicateValuesModule filterDuplicateValuesModule,
            ICacheModule<GenericMessageQueueEntity<TagValue>> cacheModule,
            Interfaces.ITagValueConverter tagValueConverter
            )
        {
            _logger = logger;
            _logger?.LogInformation(MessageConstants.IS_STARTED_TEXT);

            _initializationModuleConfig = initializationModuleConfig;
            _kafkaSendingModule = kafkaSendingModule;
            _oPCUAConnectorModule = oPCUAConnectorModule;
            //_periodizationModule = periodizationModule;
            _filterDuplicateValuesModule = filterDuplicateValuesModule;
            _cacheModule = cacheModule;
            _tagValueConverter = tagValueConverter;

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseProblemDetails();
            app.UseCors(builder =>
                builder.WithOrigins()
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json",
                        $"{Assembly.GetExecutingAssembly().GetName().Name} V1");
                })
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            StartModules();
        }

        private void StartModules()
        {
            StartModule(_kafkaSendingModule, true);
            StartModule(_cacheModule, _initializationModuleConfig?.CacheStarted ?? true);
            //StartModule(_periodizationModule, _initializationModuleConfig?.PeriodizationStarted ?? true);
            StartModule(_oPCUAConnectorModule, _initializationModuleConfig?.OPCUAConnectorStarted ?? true);
            StartModule(_filterDuplicateValuesModule, true);
            //(_periodizationModule as ISchedulerManager)?.StartSchedulers();
        }
        
        private void OnShutdown()
        {
            _logger?.LogInformation(MessageConstants.IS_STOPPED_TEXT);
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            options.OnBeforeWriteDetails = (ctx, problem) =>
            {
                problem.Extensions["traceId"] = ctx.TraceIdentifier;
            };
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }
        #endregion Methods
    }
}
