using System.Data.Common;
using EvolveDb;
using VideoTag.Server.Contexts;

namespace VideoTag.Server.StartupCommands;

/**
 * Runs DB schema migrations using Evolve
 */
public class DbMigrationStartupCommand(DapperContext dapperContext, ILogger<DbMigrationStartupCommand> logger) : IStartupCommand
{
    public Task Run()
    {
        using var connection = dapperContext.CreateConnection();
        
        var evolve = new Evolve(
            (DbConnection)connection,
            msg => logger.LogInformation("{Message}", msg)
        )
        {
            Locations = ["Migrations"],
            MetadataTableName = "Migrations",
            IsEraseDisabled = true
        };
        
        evolve.Migrate();
        
        return Task.CompletedTask;
    }
}
