using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = new();
    private InsuranceApiFactory? _apiFactory;
    private Respawner _respawner = null!;

    internal IServiceProvider Services => _apiFactory!.Services;

    public async Task InitializeAsync()
    {
        try
        {
            await _database.CreateAsync();
            _apiFactory = new InsuranceApiFactory(_database.ConnectionString);

            await using (var scope = Services.CreateAsyncScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<InsuranceDbContext>()
                    .Database
                    .MigrateAsync();
            }

            await using var connection = new NpgsqlConnection(_database.ConnectionString);
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore =
                [
                    new("public", "__EFMigrationsHistory"),
                    new("public", "countries"),
                    new("public", "counties"),
                    new("public", "cities")
                ]
            });
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    internal HttpClient CreateHttpClient() => _apiFactory!.CreateClient();

    internal async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_apiFactory is not null)
            {
                await _apiFactory.DisposeAsync();
                _apiFactory = null;
            }
        }
        finally
        {
            await _database.DisposeAsync();
        }
    }
}
