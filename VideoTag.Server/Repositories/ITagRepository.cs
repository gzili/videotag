using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public interface ITagRepository
{
    Task InsertTag(Tag tag);

    Task<IEnumerable<Tag>> GetTags();
    
    Task<Tag> GetTag(Guid tagId);

    Task UpdateTag(Tag tag);
    
    Task DeleteTag(Guid tagId);
}