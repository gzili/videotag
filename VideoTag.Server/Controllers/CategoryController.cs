using Microsoft.AspNetCore.Mvc;
using VideoTag.Server.Contracts;
using VideoTag.Server.Services;

namespace VideoTag.Server.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateOrUpdateDto dto)
    {
        var category = await categoryService.CreateCategory(dto);
        return Ok(CategoryDto.FromCategory(category));
    }

    [HttpGet]
    public async Task<IEnumerable<CategoryListItemDto>> GetCategories(bool includeTags)
    {
        var categories = await categoryService.GetCategories(includeTags);
        return categories.Select(CategoryListItemDto.FromCategory);
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid categoryId, CategoryCreateOrUpdateDto dto)
    {
        try
        {
            var category = await categoryService.UpdateCategory(categoryId, dto);
            return Ok(CategoryDto.FromCategory(category));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        try
        {
            await categoryService.DeleteCategory(categoryId);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return Ok();
    }
}