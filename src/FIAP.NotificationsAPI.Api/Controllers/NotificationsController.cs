using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.NotificationsAPI.Api.Controllers;

[Route("api/notifications")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("welcome")]
    public async Task<IActionResult> SendWelcomeAsync(
        SendWelcomeEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificationService.SendWelcomeEmailAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("payment-processed")]
    public async Task<IActionResult> SendPaymentProcessedAsync(
        SendPaymentProcessedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificationService.SendPaymentProcessedEmailAsync(request, cancellationToken);
        return Ok(response);
    }
}
