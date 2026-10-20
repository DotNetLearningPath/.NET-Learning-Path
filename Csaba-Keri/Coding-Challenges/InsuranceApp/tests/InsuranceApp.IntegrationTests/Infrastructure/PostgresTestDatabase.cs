using Npgsql;

namespace InsuranceApp.IntegrationTests.Infrastructure;

internal sealed class PostgresTestDatabase : IAsyncDisposable
{
    private const string ConnectionStringEnvVariable = "INSURANCE_TEST_POSTGRES_CONNECTION_STRING";
    private const string AdminDatabaseName = "insurance_app_test_admin";
    private const string TestUsername = "insurance_test";
    private const string DefaultConnectionString = $"Host=localhost;Port=5433;Database={AdminDatabaseName};Username={TestUsername};Password=insurance_test_password";

    private readonly string _databaseName = $"insurance_app_test_{Guid.NewGuid():N}";
    private readonly string _adminConnectionString;
    private bool _created;

    public string ConnectionString { get; }

    public PostgresTestDatabase()
    {
        var configuredConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVariable);

        var builder = new NpgsqlConnectionStringBuilder(
            !string.IsNullOrWhiteSpace(configuredConnectionString)
                ? configuredConnectionString
                : DefaultConnectionString
        );

        // Fail instead of silently repurposing an application's connection string.
        if (builder.Database != AdminDatabaseName || builder.Username != TestUsername)
        {
            throw new InvalidOperationException(
                $"Integration tests require database '{AdminDatabaseName}' and user '{TestUsername}'."
            );
        }

        // Do not retain idle connections after disposing the shared test host.
        builder.Pooling = false;
        builder.Timeout = 10;

        _adminConnectionString = builder.ConnectionString;
        builder.Database = _databaseName;
        ConnectionString = builder.ConnectionString;
    }

    public async Task CreateAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);

        try
        {
            await connection.OpenAsync();
        }
        catch (NpgsqlException exception)
        {
            throw new InvalidOperationException(
                "Cannot connect to the integration-test PostgreSQL server.",
                exception
            );
        }

        using var commandBuilder = new NpgsqlCommandBuilder();
        var quotedDatabaseName = commandBuilder.QuoteIdentifier(_databaseName);
        
        await using var createDbCommand = new NpgsqlCommand(
            $"CREATE DATABASE {quotedDatabaseName} TEMPLATE template0",
            connection
        );
        
        await createDbCommand.ExecuteNonQueryAsync();
        _created = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_created)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();

        using var commandBuilder = new NpgsqlCommandBuilder();
        var quotedDatabaseName = commandBuilder.QuoteIdentifier(_databaseName);

        await using var dropDbCommand = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS {quotedDatabaseName} WITH (FORCE)",
            connection
        );
        
        await dropDbCommand.ExecuteNonQueryAsync();
        _created = false;
    }
}
