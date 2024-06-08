namespace VideoTag.Server.Contracts;

public class TagCreateOrUpdateDto
{
    public string Label { get; set; }
    
    public Guid CategoryId { get; set; }
}