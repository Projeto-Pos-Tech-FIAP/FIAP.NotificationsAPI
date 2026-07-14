using FIAP.NotificationsAPI.Application.Interfaces;
using FIAP.NotificationsAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.NotificationsAPI.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Singleton: NotificationService não tem estado nem dependências (só simula
        // envio via Console.WriteLine), e precisa ser injetável nos BackgroundServices
        // (Kafka consumers), que são singletons — não podem consumir um serviço Scoped.
        services.AddSingleton<INotificationService, NotificationService>();

        return services;
    }
}
