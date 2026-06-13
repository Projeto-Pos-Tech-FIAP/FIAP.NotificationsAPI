namespace FIAP.NotificationsAPI.Domain.Events
{
    public class PaymentProcessedEvent
    {
        public int OrderId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
