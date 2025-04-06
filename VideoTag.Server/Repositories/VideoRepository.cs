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
    Task UpdateThumbnailSeek(Guid videoId, double seek);
    Task SaveCustomThumbnail(Guid videoId, byte[] thumbnail);
    Task UpdateVideo(Video video);
    Task DeleteVideo(Guid videoId);
}

public class VideoRepository(DapperContext dapperContext) : IVideoRepository
{
    public async Task InsertVideo(Video video)
    {
        const string sql = """
                             INSERT INTO Videos(VideoId, FullPath, Duration, Resolution, Size, LastModifiedTimeUtc, ThumbnailSeek)
                             VALUES (@VideoId, @FullPath, @Duration, @Resolution, @Size, @LastModifiedTimeUtc, @ThumbnailSeek)
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, video);
        }
    }

    public async Task<IEnumerable<Video>> GetVideos()
    {
        const string sql = """
                             SELECT V.VideoId, V.FullPath, V.Duration, V.Resolution, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek
                             FROM Videos V
                                 LEFT JOIN VideoTags VT on V.VideoId = VT.VideoId
                             WHERE VT.VideoId IS NULL
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QueryAsync<Video>(sql);
        }
    }

    public async Task<IEnumerable<Video>> GetVideos(Guid[] tagIds)
    {
        const string sql = """
                           SELECT V.VideoId, V.FullPath, V.Duration, V.Resolution, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek
                           FROM Videos V
                               JOIN VideoTags VT ON V.VideoId = VT.VideoId AND VT.TagId IN @TagIds
                           GROUP BY V.VideoId, V.FullPath, V.Duration, V.Resolution, V.Size, V.LastModifiedTimeUtc, V.ThumbnailSeek
                           HAVING COUNT(V.VideoId) = @Count
                           """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QueryAsync<Video>(sql, new { TagIds = tagIds, Count = tagIds.Length });
        }
    }

    public async Task<IEnumerable<Video>> GetVideosByFileSizeAndDateModified(long size, DateTime lastModifiedTimeUtc)
    {
        const string sql = """
                           SELECT VideoId, FullPath, Duration, Resolution, Size, LastModifiedTimeUtc, ThumbnailSeek
                           FROM Videos
                           WHERE Size = @Size AND LastModifiedTimeUtc = @LastModified
                           """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QueryAsync<Video>(sql, new { Size = size, LastModified = lastModifiedTimeUtc });
        }
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
                             SELECT VideoId, FullPath, Duration, Resolution, Size, LastModifiedTimeUtc, ThumbnailSeek
                             FROM Videos
                             WHERE VideoId = @VideoId
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QuerySingleAsync<Video>(sql, new { VideoId = videoId });
        }
    }

    public async Task<bool> ExistsByFullPath(string fullPath)
    {
        const string sql = "SELECT 1 FROM Videos WHERE FullPath = @FullPath";
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.ExecuteScalarAsync<bool>(sql, new { FullPath = fullPath });
        }
    }

    public async Task AddTag(Guid videoId, Guid tagId)
    {
        const string sql = "INSERT INTO VideoTags(VideoId, TagId) VALUES (@VideoId, @TagId)";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, new { VideoId = videoId, TagId = tagId });
        }
    }

    public async Task<IEnumerable<Tag>> GetTags(Guid videoId)
    {
        const string sql = """
                           SELECT T.TagId, T.Label, T.CategoryId, C.Label
                           FROM VideoTags VT
                               JOIN Tags T on T.TagId = VT.TagId
                               JOIN Categories C on C.CategoryId = T.CategoryId
                           WHERE VT.VideoId = @VideoId
                           ORDER BY T.Label
                           """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QueryAsync<Tag, Category, Tag>(sql, (tag, category) =>
            {
                tag.Category = category;
                return tag;
            }, new { VideoId = videoId }, splitOn: "CategoryId");
        }
    }

    public async Task RemoveTag(Guid videoId, Guid tagId)
    {
        const string sql = "DELETE FROM VideoTags WHERE VideoId = @VideoId AND TagId = @TagId";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, new { VideoId = videoId, TagId = tagId });
        }
    }

    public async Task UpdateFullPath(Guid videoId, string fullPath)
    {
        const string sql = "UPDATE Videos SET FullPath = @FullPath WHERE VideoId = @VideoId";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, new { VideoId = videoId, FullPath = fullPath });
        }
    }

    public async Task UpdateThumbnailSeek(Guid videoId, double seek)
    {
        const string sql = "UPDATE Videos SET ThumbnailSeek = @Seek WHERE VideoId = @VideoId";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, new { VideoId = videoId, Seek = seek });
        }
    }
    
    public async Task SaveCustomThumbnail(Guid videoId, byte[] thumbnail)
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

    public async Task UpdateVideo(Video video)
    {
        const string sql = """
                           UPDATE Videos
                           SET FullPath = @fullPath,
                               Width = @width,
                               Height = @height,
                               Framerate = @framerate,
                               DurationInSeconds = @durationInSeconds,
                               Bitrate = @bitrate,
                               Size = @size,
                               LastModifiedTimeUtc = @lastModifiedTimeUtc,
                               ThumbnailSeek = @thumbnailSeek
                           WHERE VideoId = @videoId;
                           """;
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, video);
    }

    public async Task DeleteVideo(Guid videoId)
    {
        const string sql = "DELETE FROM Videos WHERE VideoId = @VideoId";
        using (var connection = dapperContext.CreateConnection())
        {
            var rowsAffected = await connection.ExecuteAsync(sql, new { VideoId = videoId });
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("No rows affected");
            }
        }
    }
}