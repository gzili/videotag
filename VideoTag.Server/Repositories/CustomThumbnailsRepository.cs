using Dapper;
using VideoTag.Server.Contexts;

namespace VideoTag.Server.Repositories;

public interface ICustomThumbnailsRepository
{
    Task SaveForVideo(Guid videoId, byte[] thumbnail);

    Task<byte[]?> GetForVideo(Guid videoId);
}

public class CustomThumbnailsRepository(DapperContext dapperContext): ICustomThumbnailsRepository
{
    public async Task SaveForVideo(Guid videoId, byte[] thumbnail)
    {
        const string sql = """
                           INSERT INTO CustomThumbnails(VideoId, Thumbnail)
                           VALUES (@videoId, @thumbnail)
                           ON CONFLICT(VideoId)
                               DO UPDATE SET Thumbnail = @thumbnail
                           """;
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { videoId, thumbnail });
    }

    public async Task<byte[]?> GetForVideo(Guid videoId)
    {
        const string sql = """
                           SELECT Thumbnail
                           FROM CustomThumbnails
                           WHERE VideoId = @videoId
                           """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<byte[]>(sql, new { videoId });
    }
}