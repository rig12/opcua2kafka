using Confluent.Kafka;
using Templates.Configurations;

namespace OPCUA2Kafka.Configurations
{
    public class KafkaConfiguration : ConveyorConfiguration
    {
        public override int HandlersNumber { get; set; }
        public override int HandlersLimit { get; set; }
        public override int PauseIfFullQueue { get; set; }
        public override bool ToTraceProcessedCount { get; set; }

        /// <summary>
        /// Таймаут сообщения
        /// </summary>
        public int MessageTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Хосты брокеров
        /// </summary>
        public string Brokers { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор клиент-сервиса
        /// </summary>
        public string ClientId { get; set; } = "client_id";

        /// <summary>
        /// отчёт о доставке
        /// </summary>
        public bool EnableDeliveryReports { get; set; }

        /// <summary>
        /// количество сообщений в пачке
        /// </summary>
        public int QueueBufferingMaxMs { get; set; }

        public int BatchNumMessages { get; set; }
    }
}
