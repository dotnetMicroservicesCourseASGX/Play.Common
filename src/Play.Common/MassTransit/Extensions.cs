using System;
using System.Reflection;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMassTransitWithRabbitMQ(
        this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.UsingPlayEconomyRabbitMq(configureRetries);
        });
        services.AddMassTransitHostedService(true);
        return services;
    }

    public static void UsingPlayEconomyRabbitMq(this IServiceCollectionBusConfigurator configure,
    Action<IRetryConfigurator> configureRetries = null)
    {
        configure.UsingRabbitMq((context, cfg) =>
        {
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

            cfg.Host(rabbitMQSettings.Host);
            // configura como usar los namespaces dentro de rabbitmq
            cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

            configureRetries ??= (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));

            cfg.UseMessageRetry(configureRetries);
        });
    }
}