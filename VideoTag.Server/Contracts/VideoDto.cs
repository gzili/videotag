using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class VideoDto
{
    public Guid VideoId { get; set; }
    
    public string Title { get; set; }
    
    public string FullPath { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public double Framerate { get; set; }
    
    public double DurationInSeconds { get; set; }
    
    public long Size { get; set; }
    
    public DateTime LastModifiedTimeUtc { get; set; }
    
    public string ThumbnailUrl { get; set; }
    
    public double ThumbnailSeek { get; set; }

    public static VideoDto FromVideo(Video video)
    {
        return new VideoDto
        {
            VideoId = video.VideoId,
            Title = Path.GetFileName(video.FullPath),
            FullPath = video.FullPath,
            Width = video.Width,
            Height = video.Height,
            Framerate = video.Framerate,
            DurationInSeconds = video.DurationInSeconds,
            Size = video.Size,
            LastModifiedTimeUtc = video.LastModifiedTimeUtc,
            ThumbnailUrl = $"/images/{video.VideoId:N}_large.jpg",
            ThumbnailSeek = video.ThumbnailSeek
        };
    }
}