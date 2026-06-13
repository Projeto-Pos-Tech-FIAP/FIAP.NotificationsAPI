namespace FIAP.NotificationsAPI.Infrastructure.Settings
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string UserCreatedTopic { get; set; } = string.Empty;
        public string PaymentProcessedTopic { get; set; } = string.Empty;
    }
}
