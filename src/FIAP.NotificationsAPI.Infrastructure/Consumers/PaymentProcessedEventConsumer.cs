using Confluent.Kafka;
using FIAP.NotificationsAPI.Application.DTOs.Requests;
using FIAP.NotificationsAPI.Application.Interfaces;
using FIAP.NotificationsAPI.Domain.Events;
using FIAP.NotificationsAPI.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FIAP.NotificationsAPI.Infrastructure.Consumers
{
    public class PaymentProcessedEventConsumer : BackgroundService
    {
        private readonly ILogger<PaymentProcessedEventConsumer> _logger;
        private readonly INotificationService _notificationService;
        private readonly KafkaSettings _kafkaSettings;

        public PaymentProcessedEventConsumer(
            ILogger<PaymentProcessedEventConsumer> logger,
            INotificationService notificationService,
            IOptions<KafkaSettings> kafkaOptions)
        {
            _logger = logger;
            _notificationService = notificationService;
            _kafkaSettings = kafkaOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(_kafkaSettings.PaymentProcessedTopic);
            _logger.LogInformation("Kafka consumer started. Listening topic: {Topic}", _kafkaSettings.PaymentProcessedTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);

                    var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(
                        result.Message.Value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (paymentProcessedEvent is null)
                    {
                        _logger.LogWarning("Invalid PaymentProcessedEvent message received.");
                        continue;
                    }

                    if (!string.Equals(paymentProcessedEvent.Status, "Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation(
                            "Payment not approved. Notification skipped. CorrelationId: {CorrelationId}, Status: {Status}",
                            paymentProcessedEvent.CorrelationId,
                            paymentProcessedEvent.Status);

                        consumer.Commit(result);
                        continue;
                    }

                    var request = new SendPaymentProcessedEmailRequest
                    {
                        UserId = paymentProcessedEvent.UserId,
                        GameId = paymentProcessedEvent.GameId,
                        CorrelationId = paymentProcessedEvent.CorrelationId,
                        Status = Domain.Enums.PaymentStatus.Approved
                    };

                    await _notificationService.SendPaymentProcessedEmailAsync(request, stoppingToken);

                    consumer.Commit(result);

                    _logger.LogInformation(
                        "PaymentProcessedEvent processed successfully. CorrelationId: {CorrelationId}",
                        paymentProcessedEvent.CorrelationId);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("PaymentProcessedEventConsumer stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing PaymentProcessedEvent.");
                }
            }
            consumer.Close();
        }
    }
}

