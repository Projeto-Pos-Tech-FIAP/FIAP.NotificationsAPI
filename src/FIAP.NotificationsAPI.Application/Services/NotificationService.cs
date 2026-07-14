using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.DTOs.Responses;
using FIAP.NotificationsAPI.Application.Interfaces;

namespace FIAP.NotificationsAPI.Application.Services;

public class NotificationService : INotificationService
{
    public Task<NotificationSentResponse> SendWelcomeEmailAsync(
        SendWelcomeEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine("==============================================");
        Console.WriteLine("EMAIL SENDING SIMULATION");
        Console.WriteLine("==============================================");
        Console.WriteLine($"To: {request.Email}");
        Console.WriteLine("Subject: Welcome to FIAP Cloud Games!");
        Console.WriteLine();
        Console.WriteLine($"Hello, {request.Name}!");
        Console.WriteLine();
        Console.WriteLine("Welcome to FIAP Cloud Games.");
        Console.WriteLine("Your account has been successfully created, and you can now explore our cloud gaming platform.");
        Console.WriteLine();
        Console.WriteLine("Best regards,");
        Console.WriteLine("FIAP Cloud Games Team");
        Console.WriteLine("==============================================");

        return Task.FromResult(new NotificationSentResponse
        {
            Success = true,
            Status = "Sent",
            Message = "Welcome email sent successfully."
        });
    }

    public Task<NotificationSentResponse> SendPaymentProcessedEmailAsync(
        SendPaymentProcessedEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine("==============================================");
        Console.WriteLine("EMAIL SENDING SIMULATION");
        Console.WriteLine("==============================================");
        Console.WriteLine($"To: User {request.UserId}");
        Console.WriteLine("Subject: Payment Successfully Processed - FIAP Cloud Games");
        Console.WriteLine();
        Console.WriteLine($"Hello, User {request.UserId}!");
        Console.WriteLine();

        if (!string.IsNullOrWhiteSpace(request.CorrelationId))
            Console.WriteLine($"Order ID: {request.CorrelationId}");

        Console.WriteLine($"Game ID: {request.GameId}");

        if (request.Status.HasValue)
            Console.WriteLine($"Payment Status: {request.Status.Value}");

        Console.WriteLine();
        Console.WriteLine("Your payment has been successfully processed.");
        Console.WriteLine("Your FIAP Cloud Games purchase is now confirmed.");
        Console.WriteLine();
        Console.WriteLine("Best regards,");
        Console.WriteLine("FIAP Cloud Games Team");
        Console.WriteLine("==============================================");

        return Task.FromResult(new NotificationSentResponse
        {
            Success = true,
            Status = "Sent",
            Message = "Payment processed email sent successfully."
        });
    }
}
