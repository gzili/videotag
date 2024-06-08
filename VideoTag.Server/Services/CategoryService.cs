using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<Category> CreateCategory(CategoryCreateOrUpdateDto dto)
    {
        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Label = dto.Label
        };

        await categoryRepository.InsertCategory(category);

        return category;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await categoryRepository.GetCategories();
    }

    public async Task DeleteCategory(Guid categoryId)
    {
        await categoryRepository.DeleteCategory(categoryId);
    }
}