namespace VideoTag.Server.Entities;

public class Tag
{
    public Guid TagId { get; set; }
    public string Label { get; set; }
    public Guid CategoryId { get; set; }
    
    public int VideoCount { get; set; }
    public Category? Category { get; set; }
}