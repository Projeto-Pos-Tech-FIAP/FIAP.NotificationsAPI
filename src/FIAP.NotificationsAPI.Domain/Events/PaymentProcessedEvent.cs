namespace FIAP.NotificationsAPI.Domain.Events
{
    // Shape matches FIAP.PaymentAPI.Domain.Events.PaymentProcessedEvent
    public class PaymentProcessedEvent
    {
        public string CorrelationId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public int GameId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
