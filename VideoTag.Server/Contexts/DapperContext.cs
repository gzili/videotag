using System.Data;
using Microsoft.Data.Sqlite;

namespace VideoTag.Server.Contexts;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Missing connection string");
    }

    public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);
}