using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public interface ICategoryService
{
    Task<Category> CreateCategory(CategoryCreateOrUpdateDto dto);
    Task<IEnumerable<Category>> GetCategories(bool includeTags = false);
    Task<Category> UpdateCategory(Guid categoryId, CategoryCreateOrUpdateDto dto);
    Task DeleteCategory(Guid categoryId);
}

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

    public async Task<IEnumerable<Category>> GetCategories(bool includeTags = false)
    {
        return await categoryRepository.GetCategories(includeTags);
    }

    public async Task<Category> UpdateCategory(Guid categoryId, CategoryCreateOrUpdateDto dto)
    {
        var category = await categoryRepository.GetCategory(categoryId);

        category.Label = dto.Label;

        await categoryRepository.UpdateCategory(category);

        return category;
    }

    public async Task DeleteCategory(Guid categoryId)
    {
        await categoryRepository.DeleteCategory(categoryId);
    }
}