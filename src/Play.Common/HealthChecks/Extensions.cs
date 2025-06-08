
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.HealthChecks;

public static class Extensions
{
    private const string MongoCheckName = "mongoDb";
    private const string ReadyTagName = "ready";
    private const int DefaultSeconds = 3;

    public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, TimeSpan? timeSpan = default)
    {
        return builder.Add(new HealthCheckRegistration(MongoCheckName,
                sp =>
                {
                    var configuration = sp.GetService<IConfiguration>();
                    var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings))
                    .Get<MongoDbSettings>();
                    var client = new MongoClient(mongoDbSettings.ConnectionString);
                    return new MongoDbHealthCheck(client);
                },
                HealthStatus.Unhealthy,
                [ReadyTagName],
                TimeSpan.FromSeconds(DefaultSeconds)));
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
