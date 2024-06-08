using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public class TagService(ITagRepository tagRepository, ICategoryRepository categoryRepository) : ITagService
{
    public async Task<Tag> CreateTag(TagCreateOrUpdateDto dto)
    {
        var category = await categoryRepository.GetCategory(dto.CategoryId);
        
        var tag = new Tag
        {
            TagId = Guid.NewGuid(),
            Label = dto.Label,
            CategoryId = dto.CategoryId,
            Category = category
        };
        
        await tagRepository.InsertTag(tag);

        return tag;
    }

    public async Task<IEnumerable<Tag>> GetTags()
    {
        return await tagRepository.GetTags();
    }

    public async Task<Tag> GetTag(Guid tagId)
    {
        var tag = await tagRepository.GetTag(tagId);
        tag.Category = await categoryRepository.GetCategory(tag.CategoryId);
        return tag;
    }

    public async Task<Tag> UpdateTag(Guid tagId, TagCreateOrUpdateDto dto)
    {
        var tag = await tagRepository.GetTag(tagId);
        tag.Label = dto.Label;
        tag.CategoryId = dto.CategoryId;

        var category = await categoryRepository.GetCategory(dto.CategoryId);
        tag.Category = category;
        
        await tagRepository.UpdateTag(tag);
        
        return tag;
    }

    public async Task DeleteTag(Guid tagId)
    {
        await tagRepository.DeleteTag(tagId);
    }
}