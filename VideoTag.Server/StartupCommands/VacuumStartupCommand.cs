using Dapper;
using VideoTag.Server.Contexts;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.StartupCommands;

/**
 * Runs SQLite `VACUUM` based on the flag `VacuumNeeded` in the `Meta` table.
 * The flag should be set after large migrations to shrink the DB file size.
 */
public class VacuumStartupCommand(
    ILogger<VacuumStartupCommand> logger,
    DapperContext dapperContext,
    IMetaRepository metaRepository
) : IStartupCommand
{
    public async Task Run()
    {
        var isVacuumNeeded = await metaRepository.IsVacuumNeeded();
        if (!isVacuumNeeded) return;
        logger.LogInformation("Optimizing the database...");
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync("VACUUM;");
        await metaRepository.ClearVacuumNeeded();
        logger.LogInformation("Database optimized");
    }
}
