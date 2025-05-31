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
    private const string RabbitMQ = "RABBITMQ";
    private const string ServiceBus = "SERVICEBUS";
    public static IServiceCollection AddMassTransitWithMessageBroker(
        this IServiceCollection services, IConfiguration configuration,
         Action<IRetryConfigurator> configureRetries = null)
    {
        var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
        switch (serviceSettings?.MessageBroker?.ToUpperInvariant())
        {
            case ServiceBus:
                services.AddMassTransitWithServiceBus(configureRetries);
                break;
            case RabbitMQ:
            default:
                services.AddMassTransitWithRabbitMQ(configureRetries);
                break;
        }
        return services;
    }
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

    public static IServiceCollection AddMassTransitWithServiceBus(
        this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.UsingPlayEconomyAzureServiceBus(configureRetries);
        });
        services.AddMassTransitHostedService(true);
        return services;
    }

    public static void UsingPlayEconomyMessageBroker(this IServiceCollectionBusConfigurator configure,
    IConfiguration configuration,
    Action<IRetryConfigurator> configureRetries = null)
    {
        var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
        
        switch (serviceSettings.MessageBroker?.ToUpperInvariant())
        {
            case ServiceBus:
                configure.UsingPlayEconomyAzureServiceBus(configureRetries);
                break;
            case RabbitMQ:
            default:
                configure.UsingPlayEconomyRabbitMq(configureRetries);
                break;
        }
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

    public static void UsingPlayEconomyAzureServiceBus(this IServiceCollectionBusConfigurator configure,
    Action<IRetryConfigurator> configureRetries = null)
    {
        configure.UsingAzureServiceBus((context, cfg) =>
        {
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();

            cfg.Host(serviceBusSettings.ConnectionString);
            // configura como usar los namespaces dentro de rabbitmq
            cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

            configureRetries ??= (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));

            cfg.UseMessageRetry(configureRetries);
        });
    }
}