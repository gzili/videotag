using Microsoft.AspNetCore.SignalR;
using VideoTag.Server.Entities;
using VideoTag.Server.Helpers;
using VideoTag.Server.Hubs;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;

namespace VideoTag.Server.BackgroundServices;

public class VideoLibrarySync : IHostedService
{
    private SpinLock _lock;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger<VideoLibrarySync> _logger;
    private readonly VideoLibrarySyncTrigger _trigger;
    private readonly LibraryConfiguration _libraryConfiguration;
    private readonly IVideoRepository _videoRepository;
    private readonly VideoService _videoService;
    private readonly IHubContext<SyncHub> _hubContext;

    public VideoLibrarySync(ILogger<VideoLibrarySync> logger, VideoLibrarySyncTrigger trigger, LibraryConfiguration libraryConfiguration, IVideoRepository videoRepository, VideoService videoService, IHubContext<SyncHub> hubContext)
    {
        _lock = new SpinLock(false);
        _logger = logger;
        _trigger = trigger;
        _libraryConfiguration = libraryConfiguration;
        _videoRepository = videoRepository;
        _videoService = videoService;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting video library sync background task");
        _trigger.Triggered += HandleSyncTriggered;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping video library sync background task");
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
            _logger.LogInformation("Could not acquire lock");
            return;
        }

        try
        {
            _logger.LogInformation("Acquired lock");
            await _hubContext.Clients.All.SendAsync("syncStarted");

            var videoPaths = Directory
                .EnumerateFiles(_libraryConfiguration.LibraryPath, "*", SearchOption.AllDirectories)
                .Where(path => _libraryConfiguration.AllowedFileExtensions.Any(path.EndsWith));

            var missingPaths = new List<string>();
            
            _logger.LogInformation("Scanning for video files in {LibraryPath}", _libraryConfiguration.LibraryPath);

            foreach (var path in videoPaths)
            {
                if (!await _videoRepository.ExistsByFullPath(path))
                {
                    missingPaths.Add(path);
                }
            }
            
            _logger.LogInformation("Found {Count} files missing from the library", missingPaths.Count);

            for (var i = 0; i < missingPaths.Count && !_cancellationTokenSource.IsCancellationRequested; i++)
            {
                var fullPath = missingPaths[i];

                _logger.LogInformation("Processing file {Index}/{Count}. Path: {Path}", i + 1, missingPaths.Count, fullPath);
                await _hubContext.Clients.All.SendAsync("syncProgress", fullPath, i + 1, missingPaths.Count);
                
                var fileInfo = new FileInfo(fullPath);
                fileInfo.Refresh();
                
                var matchingVideos = (await _videoRepository.GetVideosByFileSizeAndDateModified(fileInfo.Length, fileInfo.LastWriteTimeUtc)).ToList();

                if (matchingVideos.Count == 1)
                {
                    await _videoRepository.UpdateFullPath(matchingVideos[0].VideoId, fullPath);
                    _logger.LogInformation("Updating path from {ExistingPath} to {NewPath}", matchingVideos[0].FullPath, fullPath);
                    continue;
                }

                var duration = await Ffprobe.GetVideoDurationInSecondsAsync(fullPath);
                var thumbnailSeek = (int)(duration * 0.2);

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

                await _videoService.CreateVideo(video);
            }

            await _hubContext.Clients.All.SendAsync("syncFinished");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to sync video library");
            await _hubContext.Clients.All.SendAsync("syncFailed");
        }
        finally
        {
            try
            {
                _lock.Exit();
                _logger.LogInformation("Released lock");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not release lock");
            }
        }
    }
}