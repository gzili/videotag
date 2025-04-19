using Dapper;
using VideoTag.Server.Contexts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public interface IVideoRepository
{
    Task InsertVideo(Video video);
    Task<IEnumerable<Video>> GetVideos();
    Task<IEnumerable<Video>> GetVideos(Guid[] tagIds);
    Task<IEnumerable<Video>> GetVideosByFileSizeAndDateModified(long size, DateTime lastModifiedTimeUtc);
    Task<IEnumerable<Guid>> GetVideoIds();
    Task<Video> GetVideo(Guid videoId);
    Task<bool> ExistsByFullPath(string fullPath);
    Task AddTag(Guid videoId, Guid tagId);
    Task<IEnumerable<Tag>> GetTags(Guid videoId);
    Task RemoveTag(Guid videoId, Guid tagId);
    Task UpdateFullPath(Guid videoId, string fullPath);
    Task UpdateVideo(Video video);
    Task DeleteVideo(Guid videoId);
}

public class VideoRepository(DapperContext dapperContext) : IVideoRepository
{
    public async Task InsertVideo(Video video)
    {
        const string sql = """
                             INSERT INTO Videos(VideoId, FullPath, Width, Height, Framerate, DurationInSeconds, Bitrate, Size, LastModifiedTimeUtc, ThumbnailSeek, ThumbnailTimestamp)
                             VALUES (@VideoId, @FullPath, @Width, @Height, @Framerate, @DurationInSeconds, @Bitrate, @Size, @LastModifiedTimeUtc, @ThumbnailSeek, @ThumbnailTimestamp)
                             """;
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, video);
    }

    public async Task<IEnumerable<Video>> GetVideos()
    {
        const string sql = """
                             SELECT V.VideoId, V.FullPath, V.Width, V.Height, V.Framerate, V.DurationInSeconds, V.Bitrate, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek, V.ThumbnailTimestamp
                             FROM Videos V
                                 LEFT JOIN VideoTags VT on V.VideoId = VT.VideoId
                             WHERE VT.VideoId IS NULL
                             """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Video>(sql);
    }

    public async Task<IEnumerable<Video>> GetVideos(Guid[] tagIds)
    {
        const string sql = """
                           SELECT V.VideoId, V.FullPath, V.Width, V.Height, V.Framerate, V.DurationInSeconds, V.Bitrate, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek, V.ThumbnailTimestamp
                           FROM Videos V
                               JOIN VideoTags VT ON V.VideoId = VT.VideoId AND VT.TagId IN @tagIds
                           GROUP BY V.VideoId, V.FullPath, V.Width, V.Height, V.Framerate, V.DurationInSeconds, V.Bitrate, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek, V.ThumbnailTimestamp
                           HAVING COUNT(V.VideoId) = @Count
                           """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Video>(sql, new { tagIds, Count = tagIds.Length });
    }

    public async Task<IEnumerable<Video>> GetVideosByFileSizeAndDateModified(long size, DateTime lastModifiedTimeUtc)
    {
        const string sql = """
                           SELECT VideoId, FullPath, Width, Height, Framerate, DurationInSeconds, Bitrate, Size, LastModifiedTimeUtc, ThumbnailSeek, ThumbnailTimestamp
                           FROM Videos
                           WHERE Size = @size AND LastModifiedTimeUtc = @lastModifiedTimeUtc
                           """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Video>(sql, new { size, lastModifiedTimeUtc });
    }

    public async Task<IEnumerable<Guid>> GetVideoIds()
    {
        const string sql = "SELECT VideoId FROM Videos";
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Guid>(sql);
    }

    public async Task<Video> GetVideo(Guid videoId)
    {
        const string sql = """
                             SELECT VideoId, FullPath, Width, Height, Framerate, DurationInSeconds, Bitrate, Size, LastModifiedTimeUtc, ThumbnailSeek, ThumbnailTimestamp
                             FROM Videos
                             WHERE VideoId = @videoId
                             """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QuerySingleAsync<Video>(sql, new { videoId });
    }

    public async Task<bool> ExistsByFullPath(string fullPath)
    {
        const string sql = "SELECT 1 FROM Videos WHERE FullPath = @fullPath";
        using var connection = dapperContext.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { fullPath });
    }

    public async Task AddTag(Guid videoId, Guid tagId)
    {
        const string sql = "INSERT INTO VideoTags(VideoId, TagId) VALUES (@videoId, @tagId)";
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { videoId, tagId });
    }

    public async Task<IEnumerable<Tag>> GetTags(Guid videoId)
    {
        const string sql = """
                           SELECT T.TagId, T.Label, T.CategoryId, C.Label
                           FROM VideoTags VT
                               JOIN Tags T on T.TagId = VT.TagId
                               JOIN Categories C on C.CategoryId = T.CategoryId
                           WHERE VT.VideoId = @videoId
                           ORDER BY T.Label
                           """;
        using var connection = dapperContext.CreateConnection();
        return await connection.QueryAsync<Tag, Category, Tag>(sql, (tag, category) =>
        {
            tag.Category = category;
            return tag;
        }, new { videoId }, splitOn: "CategoryId");
    }

    public async Task RemoveTag(Guid videoId, Guid tagId)
    {
        const string sql = "DELETE FROM VideoTags WHERE VideoId = @videoId AND TagId = @tagId";
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { videoId, tagId });
    }

    public async Task UpdateFullPath(Guid videoId, string fullPath)
    {
        const string sql = "UPDATE Videos SET FullPath = @fullPath WHERE VideoId = @videoId";
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { videoId, fullPath });
    }

    public async Task UpdateVideo(Video video)
    {
        const string sql = """
                           UPDATE Videos
                           SET FullPath = @FullPath,
                               Width = @Width,
                               Height = @Height,
                               Framerate = @Framerate,
                               DurationInSeconds = @DurationInSeconds,
                               Bitrate = @Bitrate,
                               Size = @Size,
                               LastModifiedTimeUtc = @LastModifiedTimeUtc,
                               ThumbnailSeek = @ThumbnailSeek,
                               ThumbnailTimestamp = @ThumbnailTimestamp
                           WHERE VideoId = @VideoId;
                           """;
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, video);
    }

    public async Task DeleteVideo(Guid videoId)
    {
        const string sql = "DELETE FROM Videos WHERE VideoId = @videoId";
        using var connection = dapperContext.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { videoId });
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException("No rows affected");
        }
    }
}