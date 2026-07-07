using Confluent.Kafka;
using FIAP.NotificationsAPI.Application.Interfaces;
using FIAP.NotificationsAPI.Domain.Events;
using FIAP.NotificationsAPI.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using FIAP.NotificationsAPI.Application.DTOs.Requests;

namespace FIAP.NotificationsAPI.Infrastructure.Consumers
{
    public class UserCreatedEventConsumer : BackgroundService
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly INotificationService _sendNotificationsServices;
        private readonly ILogger<UserCreatedEventConsumer> _logger;
        public UserCreatedEventConsumer(
            IOptions<KafkaSettings> kafkaOptions,
            INotificationService sendNotificationsServices,
            ILogger<UserCreatedEventConsumer> logger)
        {
            _kafkaSettings = kafkaOptions.Value;
            _sendNotificationsServices = sendNotificationsServices;
            _logger = logger;
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
            consumer.Subscribe(_kafkaSettings.UserCreatedTopic);
            _logger.LogInformation($"Kafka consumer started. Listening topic: {_kafkaSettings.UserCreatedTopic}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);

                    var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(
                        result.Message.Value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (userCreatedEvent is null)
                    {
                        _logger.LogWarning("Invalid UserCreatedEvent message received.");
                        continue;
                    }

                    var request = new SendWelcomeEmailRequest
                    {
                        Email = userCreatedEvent.Email,
                        Name = userCreatedEvent.Name
                    };

                    await _sendNotificationsServices.SendWelcomeEmailAsync(request, stoppingToken);

                    _logger.LogInformation("UserCreatedEvent processed successfully.");

                    consumer.Commit(result);

                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("UserCreatedEventConsumer stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing UserCreatedEvent.");
                }
            }
            consumer.Close();
        }
    }
}
