using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public interface IVideoRepository
{
    Task InsertVideo(Video video);

    Task<IEnumerable<Video>> GetVideos();

    Task<IEnumerable<Video>> GetVideos(Guid[] tagIds);

    Task<IEnumerable<Video>> GetVideosByFileSizeAndDateModified(long size, DateTime lastModifiedTimeUtc);

    Task<Video> GetVideo(Guid videoId);

    Task<bool> ExistsByFullPath(string fullPath);

    Task AddTag(Guid videoId, Guid tagId);

    Task<IEnumerable<Tag>> GetTags(Guid videoId);

    Task RemoveTag(Guid videoId, Guid tagId);

    Task UpdateFullPath(Guid videoId, string fullPath);

    Task UpdateThumbnailSeek(Guid videoId, int seek);

    Task DeleteVideo(Guid videoId);
}