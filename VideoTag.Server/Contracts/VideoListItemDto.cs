using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class VideoListItemDto
{
    public Guid VideoId { get; set; }
    
    public string Title { get; set; }
    
    public int Duration { get; set; }
    
    public string Resolution { get; set; }
    
    public long Size { get; set; }
    
    public long LastModifiedUnixSeconds { get; set; }
    
    public string ThumbnailUrl { get; set; }

    public static VideoListItemDto FromVideo(Video video)
    {
        return new VideoListItemDto
        {
            VideoId = video.VideoId,
            Title = Path.GetFileName(video.FullPath),
            Duration = video.Duration,
            Resolution = video.Resolution,
            Size = video.Size,
            LastModifiedUnixSeconds = ((DateTimeOffset)video.LastModifiedTimeUtc).ToUnixTimeSeconds(),
            ThumbnailUrl = $"/images/{video.VideoId:N}.jpg?s=${video.ThumbnailSeek}"
        };
    }
}