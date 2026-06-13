namespace FIAP.NotificationsAPI.Domain.Events
{
    public class UserCreatedEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
