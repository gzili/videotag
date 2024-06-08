namespace VideoTag.Server.Entities;

public class Category
{
    public Guid CategoryId { get; set; }
    public string Label { get; set; }
    public List<Tag> Tags { get; set; } = [];
}