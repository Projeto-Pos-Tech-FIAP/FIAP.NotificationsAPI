using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.DTOs.Responses;

namespace FIAP.NotificationsAPI.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationSentResponse> SendWelcomeEmailAsync(
        SendWelcomeEmailRequest request,
        CancellationToken cancellationToken = default);

    Task<NotificationSentResponse> SendPaymentProcessedEmailAsync(
        SendPaymentProcessedEmailRequest request,
        CancellationToken cancellationToken = default);
}
