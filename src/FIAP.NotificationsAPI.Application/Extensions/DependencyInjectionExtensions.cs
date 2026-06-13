using FIAP.NotificationsAPI.Application.Interfaces;
using FIAP.NotificationsAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.NotificationsAPI.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
