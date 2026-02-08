using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class CategoryListItemDto
{
    public class TagDto
    {
        public Guid TagId { get; set; }
        public string Label { get; set; }
        public int VideoCount { get; set; }
    }
    
    public Guid CategoryId { get; set; }
    public string Label { get; set; }
    public List<TagDto> Tags { get; set; }

    public static CategoryListItemDto FromCategory(Category category)
    {
        return new CategoryListItemDto
        {
            CategoryId = category.CategoryId,
            Label = category.Label,
            Tags = category.Tags.Select(tag => new TagDto
            {
                TagId = tag.TagId,
                Label = tag.Label,
                VideoCount = tag.VideoCount
            }).ToList()
        };
    }
}