using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class VideoListItemDto
{
    public Guid VideoId { get; set; }
    
    public string Title { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public double DurationInSeconds { get; set; }
    
    public long Size { get; set; }
    
    public long LastModifiedUnixSeconds { get; set; }
    
    public string ThumbnailUrl { get; set; }

    public static VideoListItemDto FromVideo(Video video)
    {
        return new VideoListItemDto
        {
            VideoId = video.VideoId,
            Title = Path.GetFileName(video.FullPath),
            Width = video.Width,
            Height = video.Height,
            DurationInSeconds = video.DurationInSeconds,
            Size = video.Size,
            LastModifiedUnixSeconds = ((DateTimeOffset)video.LastModifiedTimeUtc).ToUnixTimeSeconds(),
            ThumbnailUrl = $"/images/{video.VideoId:N}_small.jpg?t={video.ThumbnailTimestamp}"
        };
    }
}