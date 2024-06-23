using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public interface ICategoryRepository
{
    Task InsertCategory(Category category);

    Task<IEnumerable<Category>> GetCategories(bool includeTags = false);

    Task<Category> GetCategory(Guid categoryId);

    Task UpdateCategory(Category category);
    
    Task DeleteCategory(Guid categoryId);
}