using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace Claims.Api.Tests.Infrastructure;

/// <summary>
/// Boots the real API against throwaway PostgreSQL and Kafka containers (Testcontainers; requires Docker).
/// Shared by every test class in the "api" collection, so containers start once per test run.
/// </summary>
public sealed class ClaimsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly KafkaContainer _kafka = new KafkaBuilder("apache/kafka:4.1.0").Build();

    public string KafkaBootstrapServers => _kafka.GetBootstrapAddress();

    public async Task InitializeAsync() =>
        await Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ClaimsDb", _postgres.GetConnectionString());
        builder.UseSetting("Kafka:BootstrapServers", KafkaBootstrapServers);
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Kafka:ConsumerEnabled", "true");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _kafka.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ClaimsApiFactory>
{
    public const string Name = "api";
}
