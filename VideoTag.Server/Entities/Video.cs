namespace VideoTag.Server.Entities;

public class Video
{
    public Guid VideoId { get; set; }
    public string FullPath { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public double Framerate { get; set; }
    
    public double DurationInSeconds { get; set; }
    
    public long Bitrate { get; set; }
    
    public long Size { get; set; }
    
    public DateTime LastModifiedTimeUtc { get; set; }
    
    // Thumbnail seek in seconds from the start of the video
    public double ThumbnailSeek { get; set; }
    
    public long ThumbnailCacheKey { get; set; }
}