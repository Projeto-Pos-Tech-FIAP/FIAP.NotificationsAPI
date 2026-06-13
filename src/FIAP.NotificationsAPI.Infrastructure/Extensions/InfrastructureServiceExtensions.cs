using FIAP.NotificationsAPI.Infrastructure.Consumers;
using FIAP.NotificationsAPI.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.NotificationsAPI.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(
            configuration.GetSection("Kafka"));

        services.AddHostedService<UserCreatedEventConsumer>();
        services.AddHostedService<PaymentProcessedEventConsumer>();
        return services;
    }
}
