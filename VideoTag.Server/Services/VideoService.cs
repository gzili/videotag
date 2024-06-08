using System.Diagnostics;
using VideoTag.Server.Entities;
using VideoTag.Server.Helpers;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public class VideoService(LibraryConfiguration libraryConfiguration, IVideoRepository videoRepository)
{
    public async Task CreateVideo(Video video)
    {
        await CreateOrReplaceThumbnail(video);
        await videoRepository.InsertVideo(video);
    }

    public async Task<IEnumerable<Video>> GetVideos()
    {
        return await videoRepository.GetVideos();
    }

    public async Task<IEnumerable<Video>> GetVideos(Guid[] tagIds)
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
        Process.Start("explorer", video.FullPath).Dispose();
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
        
        var thumbnailFileName = Path.Combine(libraryConfiguration.ThumbnailDirectoryPath, $"{video.VideoId:N}.jpg");
        File.Delete(thumbnailFileName);

        if (!keepFileOnDisk)
        {
            File.Delete(video.FullPath);
        }
    }
    
    private async Task CreateOrReplaceThumbnail(Video video)
    {
        var thumbnailFileName = Path.Combine(libraryConfiguration.ThumbnailDirectoryPath, $"{video.VideoId:N}.jpg");
        await Ffmpeg.ExtractStillOnDisk(
            video.FullPath,
            thumbnailFileName,
            video.ThumbnailSeek,
            640,
            360);
    }
}