
using System;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.HealthChecks;

public static class Extensions
{
    private const string MongoCheckName = "mongoDb";
    private const string ReadyTagName = "ready";
    private const int DefaultSeconds = 15;

    public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, TimeSpan? timeSpan = default)
    {
        return builder.Add(new HealthCheckRegistration(MongoCheckName,
                sp =>
                {
                    var logger = sp.GetService<ILogger>();
                    MongoClient mongoClient;
                    try
                    {
                        // logger?.Log(LogLevel.Information, "Getting IConfiguration from service provider.");
                        var configuration = sp.GetService<IConfiguration>() ?? throw new InvalidOperationException("Configuration is required to create MongoClient.");
                        // logger?.Log(LogLevel.Information, "Configuration retrieved successfully. Getting MongoDbSettings.");
                        var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                        if (mongoDbSettings == null || string.IsNullOrWhiteSpace(mongoDbSettings.ConnectionString))
                        {
                            // logger?.Log(LogLevel.Error, "MongoDbSettings or ConnectionString is null or empty. Cannot create MongoClient.");
                            throw new InvalidOperationException("MongoDbSettings and ConnectionString are required to create MongoClient.");
                        }
                        MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbSettings.ConnectionString));
                        // logger?.Log(LogLevel.Information, "MongoClientSettings created from connection string: {connectionString}", mongoDbSettings.ConnectionString);
                        settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                        // logger?.Log(LogLevel.Information, "SSL settings configured for MongoClient.");
                        mongoClient = new MongoClient(settings);
                        logger?.Log(LogLevel.Information, "MongoClient created successfully.");

                    }
                    catch (Exception ex)
                    {
                        // get logger and log the exception
                        logger?.Log(LogLevel.Error, "Failed to create MongoClient: {message}", ex.Message);
                        throw;
                    }
                    return new MongoDbHealthCheck(mongoClient, logger);
                },
                HealthStatus.Unhealthy,
                [ReadyTagName],
                timeSpan ?? TimeSpan.FromSeconds(DefaultSeconds)));
    }

    public static void MapPlayEcnomyHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTagName)
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => true
        });
    }
}
