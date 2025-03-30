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
    Task<byte[]> GetVideoThumbnailAtSeek(Guid videoId, int seekInSeconds);
    Task<Video> UpdateThumbnailSeek(Guid videoId, int seek);
    Task AddTag(Guid videoId, Guid tagId);
    Task<IEnumerable<Tag>> GetTags(Guid videoId);
    Task RemoveTag(Guid videoId, Guid tagId);
    Task DeleteVideo(Guid videoId, bool keepFileOnDisk);
}

public class VideoService(IWebHostEnvironment environment, IVideoRepository videoRepository) : IVideoService
{
    private readonly string _thumbnailDirectoryPath = Path.Combine(environment.WebRootPath, "images");
    
    public async Task CreateVideo(Video video)
    {
        await CreateOrReplaceThumbnail(video);
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

    public async Task<byte[]> GetVideoThumbnailAtSeek(Guid videoId, int seekInSeconds)
    {
        var video = await videoRepository.GetVideo(videoId);
        seekInSeconds = Math.Min(seekInSeconds, video.Duration);
        var thumbnailBytes = await Ffmpeg.ExtractStillInMemory(video.FullPath, seekInSeconds);
        return thumbnailBytes;
    }

    public async Task<Video> UpdateThumbnailSeek(Guid videoId, int seek)
    {
        var video = await videoRepository.GetVideo(videoId);
        video.ThumbnailSeek = seek;
        await CreateOrReplaceThumbnail(video);
        await videoRepository.UpdateThumbnailSeek(video.VideoId, seek);
        return video;
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
        
        var thumbnailFileName = GetThumbnailFileName(video);
        File.Delete(thumbnailFileName);

        if (!keepFileOnDisk)
        {
            File.Delete(video.FullPath);
        }
    }
    
    private async Task CreateOrReplaceThumbnail(Video video)
    {
        var thumbnailFileName = GetThumbnailFileName(video);
        await Ffmpeg.ExtractStillOnDisk(
            video.FullPath,
            thumbnailFileName,
            video.ThumbnailSeek,
            640,
            360);
    }

    private string GetThumbnailFileName(Video video) =>
        Path.Combine(_thumbnailDirectoryPath, $"{video.VideoId:N}.jpg");
}