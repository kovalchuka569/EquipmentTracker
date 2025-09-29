using Common.Exceptions;

namespace Core.Interfaces;

public interface IDbConnectionService
{
    Task TestConnectionAsync(string connectionString);
}