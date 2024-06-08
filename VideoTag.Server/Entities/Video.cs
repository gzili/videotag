namespace VideoTag.Server.Entities;

public class Video
{
    public Guid VideoId { get; set; }
    
    public string FullPath { get; set; }
    
    // Video duration in seconds
    public int Duration { get; set; }
    
    // Video resolution (e.g. 1920x1080)
    public string Resolution { get; set; }
    
    // The size of the file in bytes
    public long Size { get; set; }
    
    public DateTime LastModifiedTimeUtc { get; set; }
    
    // Thumbnail seek in seconds from the start of the video
    public int ThumbnailSeek { get; set; }
}