using Dapper;
using VideoTag.Server.Contexts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public class WatchLogRepository(DapperContext dapperContext)
{
    public async Task InsertEntry(WatchLogEntry entry)
    {
        const string sql = """
                           INSERT INTO WatchLogs(VideoId, TimeUtc)
                           VALUES(@VideoId, @TimeUtc)
                           RETURNING Id;
                           """;
        var connection = dapperContext.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>(sql, new { entry.VideoId, entry.TimeUtc });
        entry.Id = id;
    }
}
