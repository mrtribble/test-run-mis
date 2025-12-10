using MySql.Data.MySqlClient;

namespace api.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection GetConnection()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured. Please set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
        }
        return new MySqlConnection(_connectionString);
    }

    public async Task<MySqlConnection> GetConnectionAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured. Please set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
        }
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}

