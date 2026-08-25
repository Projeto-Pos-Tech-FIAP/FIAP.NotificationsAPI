using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.KafkaEvents;
using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.Extensions;
using FIAP.NotificationsAPI.Application.Interfaces;
using FIAP.NotificationsAPI.Domain.Enums;
using FIAP.NotificationsAPI.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAP.NotificationsAPI.Lambda;

public class Function
{
    private const string UserCreatedTopic = "user-created";
    private const string PaymentProcessedTopic = "payment-processed";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly INotificationService _notificationService;

    public Function() : this(BuildServiceProvider().GetRequiredService<INotificationService>())
    {
    }

    public Function(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private static ServiceProvider BuildServiceProvider() =>
        new ServiceCollection().AddApplication().BuildServiceProvider();

    public async Task FunctionHandler(KafkaEvent kafkaEvent, ILambdaContext context)
    {
        foreach (var (topicPartition, records) in kafkaEvent.Records)
        {
            foreach (var record in records)
            {
                try
                {
                    await ProcessRecordAsync(record, context);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(
                        $"Error while processing record from {topicPartition} at offset {record.Offset}: {ex}");
                    throw;
                }
            }
        }
    }

    private async Task ProcessRecordAsync(KafkaEvent.KafkaEventRecord record, ILambdaContext context)
    {
        var value = Decode(record.Value);

        switch (record.Topic)
        {
            case UserCreatedTopic:
                await HandleUserCreatedAsync(value, context);
                break;

            case PaymentProcessedTopic:
                await HandlePaymentProcessedAsync(value, context);
                break;

            default:
                context.Logger.LogWarning($"Unexpected topic received: {record.Topic}");
                break;
        }
    }

    private async Task HandleUserCreatedAsync(string value, ILambdaContext context)
    {
        var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(value, JsonOptions);

        if (userCreatedEvent is null)
        {
            context.Logger.LogWarning("Invalid UserCreatedEvent message received.");
            return;
        }

        var request = new SendWelcomeEmailRequest
        {
            Email = userCreatedEvent.Email,
            Name = userCreatedEvent.Name
        };

        await _notificationService.SendWelcomeEmailAsync(request);

        context.Logger.LogInformation("UserCreatedEvent processed successfully.");
    }

    private async Task HandlePaymentProcessedAsync(string value, ILambdaContext context)
    {
        var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(value, JsonOptions);

        if (paymentProcessedEvent is null)
        {
            context.Logger.LogWarning("Invalid PaymentProcessedEvent message received.");
            return;
        }

        if (!string.Equals(paymentProcessedEvent.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            context.Logger.LogInformation(
                $"Payment not approved. Notification skipped. CorrelationId: {paymentProcessedEvent.CorrelationId}, Status: {paymentProcessedEvent.Status}");
            return;
        }

        var request = new SendPaymentProcessedEmailRequest
        {
            UserId = paymentProcessedEvent.UserId,
            GameId = paymentProcessedEvent.GameId,
            CorrelationId = paymentProcessedEvent.CorrelationId,
            Status = PaymentStatus.Approved
        };

        await _notificationService.SendPaymentProcessedEmailAsync(request);

        context.Logger.LogInformation(
            $"PaymentProcessedEvent processed successfully. CorrelationId: {paymentProcessedEvent.CorrelationId}");
    }

    private static string Decode(MemoryStream value) =>
        Encoding.UTF8.GetString(value.ToArray());
}
