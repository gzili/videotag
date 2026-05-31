using VideoTag.Server.Entities;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public class WatchLogService(WatchLogRepository watchLogRepository)
{
    public async Task TrackVideoWatched(Guid videoId)
    {
        var entry = new WatchLogEntry
        {
            VideoId = videoId,
            TimeUtc = DateTime.UtcNow
        };

        await watchLogRepository.InsertEntry(entry);
    }
}
