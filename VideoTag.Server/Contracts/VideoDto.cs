using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class VideoDto
{
    public Guid VideoId { get; set; }
    
    public string Title { get; set; }
    
    public string FullPath { get; set; }
    
    public int Duration { get; set; }
    
    public string Resolution { get; set; }
    
    public long Size { get; set; }
    
    public DateTime LastModifiedTimeUtc { get; set; }
    
    public string ThumbnailUrl { get; set; }
    
    public int ThumbnailSeek { get; set; }

    public static VideoDto FromVideo(Video video)
    {
        return new VideoDto
        {
            VideoId = video.VideoId,
            Title = Path.GetFileName(video.FullPath),
            FullPath = video.FullPath,
            Duration = video.Duration,
            Resolution = video.Resolution,
            Size = video.Size,
            LastModifiedTimeUtc = video.LastModifiedTimeUtc,
            ThumbnailUrl = $"/images/{video.VideoId:N}.jpg",
            ThumbnailSeek = video.ThumbnailSeek
        };
    }
}