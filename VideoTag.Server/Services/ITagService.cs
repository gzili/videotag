using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Services;

public interface ITagService
{
    Task<Tag> CreateTag(TagCreateOrUpdateDto dto);

    Task<IEnumerable<Tag>> GetTags();
    
    Task<Tag> GetTag(Guid tagId);

    Task<Tag> UpdateTag(Guid tagId, TagCreateOrUpdateDto dto);

    Task DeleteTag(Guid tagId);
}