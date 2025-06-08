using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Play.Common.HealthChecks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoClient mongoClient;

    public MongoDbHealthCheck(MongoClient mongoClient)
    {
        this.mongoClient = mongoClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await mongoClient.ListDatabasesAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex,
                description: "MongoDB is not reachable or not responding.");
        }
    }
}