using System.IO;
using System.Security.Cryptography;
using Core.Interfaces;
using Npgsql;

namespace Core.Services;

public class DbConnectionService : IDbConnectionService
{
    public async Task TestConnectionAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT version()", connection);
        await command.ExecuteScalarAsync();
    }
}