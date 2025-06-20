using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Play.Common.HealthChecks;

public class MongoDbHealthCheck(MongoClient mongoClient, ILogger logger) : IHealthCheck
{
    private readonly MongoClient mongoClient = mongoClient;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Checking MongoDB health...");
            cancellationToken = cancellationToken == default
                ? new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token
                : cancellationToken;
            // logger?.LogInformation("Using cancellation token with a timeout of 20 seconds.");
            // logger?.LogInformation("Listing database names to check connectivity...");

            mongoClient.ListDatabaseNames(cancellationToken);
            // await mongoClient.ListDatabaseNamesAsync(cancellationToken);
            logger?.LogInformation("MongoDB is reachable. Database names listed successfully.");

            await Task.CompletedTask;

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            // logger?.LogError(ex, "MongoDB health check failed: {Message}", ex.Message);
            return HealthCheckResult.Unhealthy(
                description: "MongoDB is not reachable.",
                exception: ex);
        }
    }
}