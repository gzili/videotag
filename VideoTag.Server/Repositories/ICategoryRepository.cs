using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public interface ICategoryRepository
{
    Task InsertCategory(Category category);

    Task<IEnumerable<Category>> GetCategories();

    Task<Category> GetCategory(Guid categoryId);
    
    Task DeleteCategory(Guid categoryId);
}