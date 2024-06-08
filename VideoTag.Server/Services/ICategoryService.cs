using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Services;

public interface ICategoryService
{
    Task<Category> CreateCategory(CategoryCreateOrUpdateDto dto);

    Task<IEnumerable<Category>> GetCategories();

    Task DeleteCategory(Guid categoryId);
}