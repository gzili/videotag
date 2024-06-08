using VideoTag.Server.Entities;

namespace VideoTag.Server.Contracts;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string Label { get; set; }

    public static CategoryDto FromCategory(Category category)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Label = category.Label
        };
    }
}