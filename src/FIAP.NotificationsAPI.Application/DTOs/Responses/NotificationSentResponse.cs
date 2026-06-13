namespace FIAP.NotificationsAPI.Application.DTOs.Responses;

public class NotificationSentResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = null!;
    public string Message { get; set; } = null!;
}
