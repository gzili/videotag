using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Services;

public interface ICategoryService
{
    Task<Category> CreateCategory(CategoryCreateOrUpdateDto dto);

    Task<IEnumerable<Category>> GetCategories(bool includeTags = false);

    Task<Category> UpdateCategory(Guid categoryId, CategoryCreateOrUpdateDto dto);

    Task DeleteCategory(Guid categoryId);
}