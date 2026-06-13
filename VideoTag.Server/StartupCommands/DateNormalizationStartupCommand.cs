using Dapper;
using VideoTag.Server.Constants;
using VideoTag.Server.Contexts;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.StartupCommands;

public class DateNormalizationStartupCommand(
    ILogger<DateNormalizationStartupCommand> logger,
    DapperContext dapperContext,
    IMetaRepository metaRepository) : IStartupCommand
{
    private record Entry(Guid VideoId, DateTime LastModifiedTimeUtc);
    
    public async Task Run()
    {
        var shouldRun = await metaRepository.IsFlagSet(MetaFlag.DateNormalizationNeeded);
        if (!shouldRun) return;
        
        logger.LogInformation("Starting date normalization...");
        
        var entries = await GetDenormalizedDateEntries();
        await UpdateDenormalizedEntries(entries);
        await metaRepository.ClearFlag(MetaFlag.DateNormalizationNeeded);
        
        logger.LogInformation("Date normalization completed");
    }

    private async Task<IEnumerable<Entry>> GetDenormalizedDateEntries()
    {
        const string sql = """
SELECT VideoId, LastModifiedTimeUtc
FROM Videos
WHERE LastModifiedTimeUtc NOT LIKE '%._______Z';
""";
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Entry>(sql);
    }

    private async Task UpdateDenormalizedEntries(IEnumerable<Entry> entries)
    {
        const string sql = "UPDATE Videos SET LastModifiedTimeUtc = @LastModifiedTimeUtc WHERE VideoId = @VideoId";
        using var connection = dapperContext.CreateConnection();

        foreach (var entry in entries)
        {
            await connection.ExecuteAsync(sql, entry);
        }
    }
}
