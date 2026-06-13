using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.Services;

namespace FIAP.NotificationsAPI.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task SendWelcomeEmailAsync_ReturnsSentResponse()
    {
        var service = new NotificationService();
        var request = new SendWelcomeEmailRequest
        {
            Email = "player@fiap.com.br",
            Name = "Player One"
        };

        var response = await service.SendWelcomeEmailAsync(request);

        Assert.True(response.Success);
        Assert.Equal("Sent", response.Status);
    }

    [Fact]
    public async Task SendPaymentProcessedEmailAsync_ReturnsSentResponse()
    {
        var service = new NotificationService();
        var request = new SendPaymentProcessedEmailRequest
        {
            Email = "player@fiap.com.br",
            Name = "Player One",
            OrderId = 123,
            PaymentAmount = 99.90m,
            PaymentDate = new DateTime(2026, 6, 13)
        };

        var response = await service.SendPaymentProcessedEmailAsync(request);

        Assert.True(response.Success);
        Assert.Equal("Sent", response.Status);
    }
}
