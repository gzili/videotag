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
    IVideoRepository videoRepository,
    VideoService videoService,
    IHubContext<SyncHub> hubContext) : IHostedService
{
    private readonly SyncOptions _syncOptions = options.Value;
    private SpinLock _lock = new(false);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting video library sync background task");
        trigger.Triggered += HandleSyncTriggered;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping video library sync background task");
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private void HandleSyncTriggered(object? sender, EventArgs args)
    {
        Task.Run(async () => await Sync());
    }

    private async Task Sync()
    {
        var entered = false;
        _lock.Enter(ref entered);
        if (!entered)
        {
            logger.LogInformation("Could not acquire lock");
            return;
        }

        try
        {
            logger.LogInformation("Acquired lock");
            await hubContext.Clients.All.SendAsync("syncStarted");

            var videoPaths = Directory
                .EnumerateFiles(_syncOptions.LibraryPath, "*", SearchOption.AllDirectories)
                .Where(IsAllowedFileExtension);

            if (_syncOptions.ExcludePattern != null)
            {
                videoPaths = videoPaths.Where(path => !path.Contains(_syncOptions.ExcludePattern));
            }

            var missingPaths = new List<string>();
            
            logger.LogInformation("Scanning for video files in {LibraryPath}", _syncOptions.LibraryPath);

            foreach (var path in videoPaths)
            {
                if (!await videoRepository.ExistsByFullPath(path))
                {
                    missingPaths.Add(path);
                }
            }
            
            logger.LogInformation("Found {Count} files missing from the library", missingPaths.Count);

            var numAdded = 0;
            var numUpdated = 0;
            var numRemoved = 0;

            for (var i = 0; i < missingPaths.Count && !_cancellationTokenSource.IsCancellationRequested; i++)
            {
                var fullPath = missingPaths[i];

                logger.LogInformation("Processing file {Index}/{Count}. Path: {Path}", i + 1, missingPaths.Count, fullPath);
                await hubContext.Clients.All.SendAsync("syncProgress", fullPath, i + 1, missingPaths.Count);
                
                var fileInfo = new FileInfo(fullPath);
                fileInfo.Refresh();
                
                var matchingVideos = (await videoRepository.GetVideosByFileSizeAndDateModified(fileInfo.Length, fileInfo.LastWriteTimeUtc)).ToList();

                if (matchingVideos.Count == 1)
                {
                    logger.LogInformation("Updating path from {ExistingPath} to {NewPath}", matchingVideos[0].FullPath, fullPath);
                    await videoRepository.UpdateFullPath(matchingVideos[0].VideoId, fullPath);
                    numUpdated++;
                    continue;
                }

                var duration = await Ffprobe.GetVideoDurationInSecondsAsync(fullPath);
                int thumbnailSeek;
                if (_syncOptions.DefaultThumbnailSeek > 1)
                {
                    thumbnailSeek = Math.Min((int) _syncOptions.DefaultThumbnailSeek, duration);
                }
                else
                {
                    thumbnailSeek = (int)(duration * _syncOptions.DefaultThumbnailSeek);
                }

                var resolution = await Ffprobe.GetVideoResolutionAsync(fullPath);
                
                var video = new Video
                {
                    VideoId = Guid.NewGuid(),
                    FullPath = fullPath,
                    Duration = duration,
                    Resolution = resolution,
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
                    logger.LogInformation("Deleting entry for {Path} from the library because the file no longer exists", video.FullPath);
                    await videoService.DeleteVideo(video.VideoId, true);
                    numRemoved++;
                }
            }

            await hubContext.Clients.All.SendAsync("syncFinished", numAdded, numUpdated, numRemoved);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to sync video library");
            await hubContext.Clients.All.SendAsync("syncFailed");
        }
        finally
        {
            try
            {
                _lock.Exit();
                logger.LogInformation("Released lock");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not release lock");
            }
        }
    }

    private bool IsAllowedFileExtension(string path)
    {
        var extension = Path.GetExtension(path)[1..];
        return _syncOptions.AllowedFileExtensions.Any(allowedExtension => extension == allowedExtension);
    }
}