using System.Diagnostics;
using VideoTag.Server.Entities;
using VideoTag.Server.Helpers;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public interface IVideoService
{
    Task CreateVideo(Video video);
    Task<IEnumerable<Video>> GetVideos();
    Task<IEnumerable<Video>> GetVideosContainingAllTags(Guid[] tagIds);
    Task<Video> GetVideo(Guid videoId);
    Task PlayVideo(Guid videoId);
    Task ShowInExplorer(Guid videoId);
    Task<byte[]> GetVideoThumbnailAtSeek(Guid videoId, double seekInSeconds);
    Task<Video> UpdateThumbnailSeek(Guid videoId, double seek);
    Task<Video> SaveCustomThumbnail(Guid videoId, byte[] thumbnail);
    Task CreateThumbnailsOnDisk(Video video);
    Task AddTag(Guid videoId, Guid tagId);
    Task<IEnumerable<Tag>> GetTags(Guid videoId);
    Task RemoveTag(Guid videoId, Guid tagId);
    Task DeleteVideo(Guid videoId, bool keepFileOnDisk);
}

public class VideoService(
    IEnvironmentService environmentService,
    IVideoRepository videoRepository,
    ICustomThumbnailsRepository customThumbnailsRepository) : IVideoService
{
    private const string SmallSuffix = "small";
    private const string LargeSuffix = "large";
    private const int ThumbnailWidth = 640;
    private const int ThumbnailHeight = 360;
    
    public async Task CreateVideo(Video video)
    {
        await SaveThumbnails(video);
        await videoRepository.InsertVideo(video);
    }

    public async Task<IEnumerable<Video>> GetVideos()
    {
        return await videoRepository.GetVideos();
    }

    public async Task<IEnumerable<Video>> GetVideosContainingAllTags(Guid[] tagIds)
    {
        return await videoRepository.GetVideos(tagIds);
    }

    public async Task<Video> GetVideo(Guid videoId)
    {
        return await videoRepository.GetVideo(videoId);
    }
    
    public async Task PlayVideo(Guid videoId)
    {
        var video = await videoRepository.GetVideo(videoId);

        if (!Path.Exists(video.FullPath))
        {
            throw new FileNotFoundException("Video file does not exist", video.FullPath);
        }
        
        Process.Start("explorer", $"\"{video.FullPath}\"").Dispose();
    }

    public async Task ShowInExplorer(Guid videoId)
    {
        var video = await videoRepository.GetVideo(videoId);
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "explorer",
            Arguments = $"/select, \"{video.FullPath}\""
        };

        Process.Start(processStartInfo)?.Dispose();
    }

    public async Task<byte[]> GetVideoThumbnailAtSeek(Guid videoId, double seekInSeconds)
    {
        var video = await videoRepository.GetVideo(videoId);
        seekInSeconds = Math.Min(seekInSeconds, video.DurationInSeconds);
        var thumbnailBytes = await Ffmpeg.ExtractStillInMemory(video.FullPath, seekInSeconds);
        return thumbnailBytes;
    }

    public async Task<Video> UpdateThumbnailSeek(Guid videoId, double seek)
    {
        var video = await videoRepository.GetVideo(videoId);
        video.ThumbnailSeek = seek;
        
        await SaveThumbnails(video);
        await videoRepository.UpdateVideo(video);
        
        return video;
    }

    public async Task<Video> SaveCustomThumbnail(Guid videoId, byte[] thumbnail)
    {
        var video = await videoRepository.GetVideo(videoId);
        video.ThumbnailTimestamp = GetCurrentTimestamp();
        
        await customThumbnailsRepository.SaveForVideo(videoId, thumbnail);
        await WriteCustomThumbnail(videoId, thumbnail);
        await videoRepository.UpdateVideo(video);
        
        return video;
    }

    public async Task CreateThumbnailsOnDisk(Video video)
    {
        var customThumbnail = await customThumbnailsRepository.GetForVideo(video.VideoId);
        
        if (customThumbnail != null)
        {
            await WriteCustomThumbnail(video.VideoId, customThumbnail);
        }
        else
        {
            await SaveThumbnails(video);
        }
    }

    public async Task AddTag(Guid videoId, Guid tagId)
    {
        await videoRepository.AddTag(videoId, tagId);
    }

    public async Task<IEnumerable<Tag>> GetTags(Guid videoId)
    {
        return await videoRepository.GetTags(videoId);
    }

    public async Task RemoveTag(Guid videoId, Guid tagId)
    {
        await videoRepository.RemoveTag(videoId, tagId);
    }

    public async Task DeleteVideo(Guid videoId, bool keepFileOnDisk)
    {
        var video = await videoRepository.GetVideo(videoId);
        
        await videoRepository.DeleteVideo(video.VideoId);
        DeleteThumbnails(videoId);

        if (!keepFileOnDisk)
        {
            File.Delete(video.FullPath);
        }
    }
    
    private string GetThumbnailPath(Guid videoId, string suffix) =>
        Path.Combine(environmentService.ThumbnailsDirectoryPath, $"{videoId:N}_{suffix}.jpg");
    
    private async Task SaveThumbnails(Video video)
    {
        await Ffmpeg.ExtractStillOnDisk(
            video.FullPath,
            GetThumbnailPath(video.VideoId, LargeSuffix),
            video.ThumbnailSeek);
        await Ffmpeg.ExtractStillOnDisk(
            video.FullPath,
            GetThumbnailPath(video.VideoId, SmallSuffix),
            video.ThumbnailSeek,
            ThumbnailWidth,
            ThumbnailHeight);
        
        video.ThumbnailTimestamp = GetCurrentTimestamp();
    }
    
    private async Task WriteCustomThumbnail(Guid videoId, byte[] thumbnail)
    {
        var largeThumbnailPath = GetThumbnailPath(videoId, LargeSuffix);
        await File.WriteAllBytesAsync(largeThumbnailPath, thumbnail);
        
        await Ffmpeg.ResizeImageOnDisk(
            largeThumbnailPath,
            GetThumbnailPath(videoId, SmallSuffix),
            ThumbnailWidth,
            ThumbnailHeight);
    }

    private void DeleteThumbnails(Guid videoId)
    {
        File.Delete(GetThumbnailPath(videoId, LargeSuffix));
        File.Delete(GetThumbnailPath(videoId, SmallSuffix));
    }

    private static long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}