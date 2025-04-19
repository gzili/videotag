using VideoTag.Server.Helpers;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;

namespace VideoTag.Server.BackgroundServices;

public class RebuildJob(
    ILogger<RebuildJob> logger,
    IMetaRepository metaRepository,
    ILibraryService libraryService,
    IVideoService videoService,
    IVideoRepository videoRepository,
    IEnvironmentService environmentService) : IHostedService
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var isRebuildNeeded = await metaRepository.IsRebuildNeeded();
        if (isRebuildNeeded)
        {
            RunRebuild();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private void RunRebuild()
    {
        Task.Run(RunRebuildWithExceptionLogging);
    }

    private async Task RunRebuildWithExceptionLogging()
    {
        try
        {
            await RebuildLibrary();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Rebuild job failed.");
        }
    }

    private async Task RebuildLibrary()
    {
        logger.LogInformation("Library rebuild needed. Starting...");

        logger.LogInformation("Detecting moved files...");
        await UpdateMovedFiles();
        
        logger.LogInformation("Clearing thumbnails...");
        ClearThumbnails();
        
        logger.LogInformation("Processing videos...");
        var videoIds = (await videoRepository.GetVideoIds()).ToList();

        for (var i = 0; i < videoIds.Count; ++i)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                logger.LogInformation("Rebuild job cancelled.");
                return;
            }
            
            var videoId = videoIds[i];
            var video = await videoService.GetVideo(videoId);
            
            logger.LogInformation("Processing video {Index}/{Count}. Path: {Path}.", i + 1, videoIds.Count, video.FullPath);
            
            if (!Path.Exists(video.FullPath))
            {
                logger.LogInformation("File {Path} no longer exists. Deleting library entry.", video.FullPath);
                await videoService.DeleteVideo(video.VideoId, true);
                continue;
            }

            var properties = await Ffprobe.GetVideoPropertiesAsync(video.FullPath);

            video.Width = properties.Width;
            video.Height = properties.Height;
            video.Framerate = properties.Framerate;
            video.DurationInSeconds = properties.DurationInSeconds;
            video.Bitrate = properties.Bitrate;

            await videoRepository.UpdateVideo(video);

            await videoService.SaveThumbnails(video);
        }

        await metaRepository.ClearRebuildNeeded();
        
        logger.LogInformation("Rebuild job completed.");
    }

    private async Task UpdateMovedFiles()
    {
        var missingFiles = await libraryService.FindFilesMissingFromTheLibrary();

        foreach (var path in missingFiles)
        {
            var fileInfo = new FileInfo(path);
            fileInfo.Refresh();

            var matchingVideos = (await videoRepository.GetVideosByFileSizeAndDateModified(fileInfo.Length, fileInfo.LastWriteTimeUtc)).ToList();

            if (matchingVideos.Count != 1) continue;
            
            logger.LogInformation("Updating path from {ExistingPath} to {NewPath}.", matchingVideos[0].FullPath, path);
            await videoRepository.UpdateFullPath(matchingVideos[0].VideoId, path);
        }
    }

    private void ClearThumbnails()
    {
        Directory.Delete(environmentService.ThumbnailsDirectoryPath, true);
        Directory.CreateDirectory(environmentService.ThumbnailsDirectoryPath);
    }
}