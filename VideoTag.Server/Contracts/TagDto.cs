using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class TagDto
{
    public Guid TagId { get; set; }
    public string Label { get; set; }
    public CategoryDto Category { get; set; }

    public static TagDto FromTag(Tag tag)
    {
        return new TagDto
        {
            TagId = tag.TagId,
            Label = tag.Label,
            Category = CategoryDto.FromCategory(tag.Category)
        };
    }
}