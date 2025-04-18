using Dapper;
using VideoTag.Server.Contexts;

namespace VideoTag.Server.OneTimeCommands;

public class UpdateMigrationVersionCommand(ILogger<UpdateMigrationVersionCommand> logger, DapperContext dapperContext)
{
    public void Run()
    {
        try
        {
            using var connection = dapperContext.CreateConnection();
            const string sql = """
                               UPDATE Migrations
                               SET version = '1', name = 'V1__create_tables.sql'
                               WHERE version = '0.0.1'
                               """;
            var rowsAffected = connection.Execute(sql);
            if (rowsAffected > 0)
            {
                logger.LogInformation("Migration version format updated.");
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}