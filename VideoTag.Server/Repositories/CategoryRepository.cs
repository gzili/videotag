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

    public async Task<IEnumerable<Category>> GetCategories()
    {
        const string query = """
                             SELECT C.CategoryId, C.Label, T.TagId, T.Label
                             FROM Categories C
                                 JOIN Tags T on C.CategoryId = T.CategoryId
                             ORDER BY C.Label, T.Label
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            var categories = await connection.QueryAsync<Category, Tag?, Category>(query, (category, tag) =>
            {
                if (tag != null)
                {
                    category.Tags.Add(tag);
                }
                return category;
            }, splitOn: "TagId");

            return categories.GroupBy(c => c.CategoryId).Select(g =>
            {
                var category = g.First();
                category.Tags = g.Select(c => c.Tags.Single()).ToList();
                return category;
            });
        }
    }

    public async Task<Category> GetCategory(Guid categoryId)
    {
        const string query = "SELECT CategoryId, Label FROM Categories WHERE CategoryId = @CategoryId";
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QuerySingleAsync<Category>(query, new { CategoryId = categoryId });
        }
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
}