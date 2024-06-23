using Dapper;
using VideoTag.Server.Contexts;
using VideoTag.Server.Entities;

namespace VideoTag.Server.Repositories;

public class CategoryRepository(DapperContext dapperContext) : ICategoryRepository
{
    public async Task InsertCategory(Category category)
    {
        const string query = "INSERT INTO Categories(CategoryId, Label) VALUES (@CategoryId, @Label)";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(query, category);
        }
    }

    public Task<IEnumerable<Category>> GetCategories(bool includeTags = false)
    {
        return includeTags ? GetCategoriesWithTags() : GetAllCategories();
    }

    public async Task<Category> GetCategory(Guid categoryId)
    {
        const string query = "SELECT CategoryId, Label FROM Categories WHERE CategoryId = @CategoryId";
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QuerySingleAsync<Category>(query, new { CategoryId = categoryId });
        }
    }

    public async Task UpdateCategory(Category category)
    {
        const string sql = """
                           UPDATE Categories
                           SET Label = @Label
                           WHERE CategoryId = @CategoryId
                           """;

        using var connection = dapperContext.CreateConnection();

        await connection.ExecuteAsync(sql, category);
    }

    public async Task DeleteCategory(Guid categoryId)
    {
        const string query = "DELETE FROM Categories WHERE CategoryId = @CategoryId";
        using (var connection = dapperContext.CreateConnection())
        {
            var rowsAffected = await connection.ExecuteAsync(query, new { CategoryId = categoryId });
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("No rows affected");
            }
        }
    }

    private async Task<IEnumerable<Category>> GetCategoriesWithTags()
    {
        const string query = """
                             SELECT C.CategoryId, C.Label, T.TagId, T.Label
                             FROM Categories C
                                 LEFT JOIN Tags T on C.CategoryId = T.CategoryId
                             ORDER BY C.Label, T.Label
                             """;

        var categories = new Dictionary<Guid, Category>();
        
        using var connection = dapperContext.CreateConnection();
        
        await connection.QueryAsync<Category, Tag?, Category>(query, (category, tag) =>
        {
            if (!categories.TryAdd(category.CategoryId, category))
            {
                category = categories[category.CategoryId];
            }
                
            if (tag != null)
            {
                category.Tags.Add(tag);
            }
                
            return category;
        }, splitOn: "TagId");

        return categories.Values;
    }

    private async Task<IEnumerable<Category>> GetAllCategories()
    {
        const string query = """
                             SELECT CategoryId, Label
                             FROM Categories
                             ORDER BY Label
                             """;
        
        using var connection = dapperContext.CreateConnection();

        return await connection.QueryAsync<Category>(query);
    }
}