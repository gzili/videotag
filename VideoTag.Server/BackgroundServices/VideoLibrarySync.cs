using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using VideoTag.Server.Configuration;
using VideoTag.Server.Entities;
using VideoTag.Server.Helpers;
using VideoTag.Server.Hubs;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;

namespace VideoTag.Server.BackgroundServices;

public class VideoLibrarySync(
    ILogger<VideoLibrarySync> logger,
    VideoLibrarySyncTrigger trigger,
    IOptions<SyncOptions> options,
    ILibraryService libraryService,
    IVideoRepository videoRepository,
    IVideoService videoService,
    IHubContext<SyncHub> hubContext) : IHostedService
{
    private readonly SyncOptions _syncOptions = options.Value;
    private SpinLock _lock = new(false);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting video library sync background task...");
        trigger.Triggered += HandleSyncTriggered;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping video library sync background task...");
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private void HandleSyncTriggered(object? sender, EventArgs args)
    {
        Task.Run(Sync);
    }

    private async Task Sync()
    {
        var lockAcquired = false;
        _lock.Enter(ref lockAcquired);
        if (!lockAcquired)
        {
            logger.LogInformation("Could not acquire lock.");
            return;
        }

        try
        {
            logger.LogInformation("Acquired lock.");
            await hubContext.Clients.All.SendAsync("syncStarted");
            
            logger.LogInformation("Scanning for missing files in {Count} directories", _syncOptions.Folders.Count);

            var missingFiles = await libraryService.FindFilesMissingFromTheLibrary();
            
            logger.LogInformation("Found {Count} files missing from the library.", missingFiles.Count);

            var numAdded = 0;
            var numUpdated = 0;
            var numRemoved = 0;

            for (var i = 0; i < missingFiles.Count && !_cancellationTokenSource.IsCancellationRequested; i++)
            {
                var fullPath = missingFiles[i];

                logger.LogInformation("Processing file {Index}/{Count}. Path: {Path}.", i + 1, missingFiles.Count, fullPath);
                await hubContext.Clients.All.SendAsync("syncProgress", fullPath, i + 1, missingFiles.Count);
                
                var fileInfo = new FileInfo(fullPath);
                fileInfo.Refresh();
                
                var matchingVideos = (await videoRepository.GetVideosByFileSizeAndDateModified(fileInfo.Length, fileInfo.LastWriteTimeUtc)).ToList();

                if (matchingVideos.Count == 1)
                {
                    logger.LogInformation("Updating path from {ExistingPath} to {NewPath}.", matchingVideos[0].FullPath, fullPath);
                    await videoRepository.UpdateFullPath(matchingVideos[0].VideoId, fullPath);
                    numUpdated++;
                    continue;
                }

                var properties = await Ffprobe.GetVideoPropertiesAsync(fullPath);
                
                double thumbnailSeek;
                if (_syncOptions.DefaultThumbnailSeek > 1)
                {
                    thumbnailSeek = Math.Min((double)_syncOptions.DefaultThumbnailSeek, properties.DurationInSeconds);
                }
                else
                {
                    thumbnailSeek = properties.DurationInSeconds * (double)_syncOptions.DefaultThumbnailSeek;
                }
                
                var video = new Video
                {
                    VideoId = Guid.NewGuid(),
                    FullPath = fullPath,
                    Width = properties.Width,
                    Height = properties.Height,
                    Framerate = properties.Framerate,
                    DurationInSeconds = properties.DurationInSeconds,
                    Bitrate = properties.Bitrate,
                    Size = fileInfo.Length,
                    LastModifiedTimeUtc = fileInfo.LastWriteTimeUtc,
                    ThumbnailSeek = thumbnailSeek
                };

                await videoService.CreateVideo(video);
                numAdded++;
            }

            var videos = await videoRepository.GetVideos();
            foreach (var video in videos)
            {
                if (!Path.Exists(video.FullPath))
                {
                    logger.LogInformation("File {Path} no longer exists. Deleting library entry.", video.FullPath);
                    await videoService.DeleteVideo(video.VideoId, true);
                    numRemoved++;
                }
            }

            await hubContext.Clients.All.SendAsync("syncFinished", numAdded, numUpdated, numRemoved);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sync video library.");
            await hubContext.Clients.All.SendAsync("syncFailed");
        }
        finally
        {
            try
            {
                _lock.Exit();
                logger.LogInformation("Released lock.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not release lock.");
            }
        }
    }
}