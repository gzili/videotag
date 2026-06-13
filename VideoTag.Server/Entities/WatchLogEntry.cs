namespace VideoTag.Server.Entities;

public class WatchLogEntry
{
    public long Id { get; set; }
    
    public Guid VideoId { get; set; }
    
    public DateTime TimeUtc { get; set; }
}
