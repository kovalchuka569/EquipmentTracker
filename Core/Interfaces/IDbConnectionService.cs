namespace Core.Interfaces;

public interface IDbConnectionService
{
    Task TestConnectionAsync(string connectionString);
    
    Task PreheatConnectionAsync();
}