using Common.Logging;
using Core.Interfaces;
using Core.Services.Base;
using Data.UnitOfWork;
using Npgsql;

namespace Core.Services;

public class DbConnectionService(IUnitOfWork unitOfWork, IAppLogger<DbConnectionService> logger) 
    : DatabaseService<DbConnectionService>(unitOfWork, logger), IDbConnectionService
{
    public async Task TestConnectionAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT version()", connection);
        await command.ExecuteScalarAsync();
    }

    public async Task PreheatConnectionAsync()
    {
        await UnitOfWork.UserRepository.GetUserEntityAsync(string.Empty);
    }
}