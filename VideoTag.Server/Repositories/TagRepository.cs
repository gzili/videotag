using Dapper;
using VideoTag.Server.Contexts;
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

public class TagRepository(DapperContext dapperContext) : ITagRepository
{
    public async Task InsertTag(Tag tag)
    {
        const string query = "INSERT INTO Tags(TagId, Label, CategoryId) VALUES (@TagId, @Label, @CategoryId)";
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(query, new { tag.TagId, tag.Label, tag.CategoryId });
        }
    }

    public async Task<IEnumerable<Tag>> GetTags()
    {
        const string query = """
                             SELECT T.TagId, T.Label, T.CategoryId, C.Label
                             FROM Tags T
                                 JOIN Categories C on C.CategoryId = T.CategoryId
                             ORDER BY T.Label
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QueryAsync<Tag, Category, Tag>(query, (tag, category) =>
            {
                tag.Category = category;
                return tag;
            }, splitOn: "CategoryId");
        }
    }

    public async Task<Tag> GetTag(Guid tagId)
    {
        const string query = """
                             SELECT TagId, Label, CategoryId
                             FROM Tags
                             WHERE TagId = @TagId
                             """;
        using (var connection = dapperContext.CreateConnection())
        {
            return await connection.QuerySingleAsync<Tag>(query, new { TagId = tagId });
        }
    }

    public async Task UpdateTag(Tag tag)
    {
        const string sql = """
                           UPDATE Tags
                           SET Label = @Label, CategoryId = @CategoryId
                           WHERE TagId = @TagId
                           """;
        using (var connection = dapperContext.CreateConnection())
        {
            await connection.ExecuteAsync(sql, tag);
        }
    }

    public async Task DeleteTag(Guid tagId)
    {
        const string query = "DELETE FROM Tags WHERE TagId = @TagId";
        using (var connection = dapperContext.CreateConnection())
        {
            var rowsAffected = await connection.ExecuteAsync(query, new { TagId = tagId });
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("No rows affected");
            }
        }
    }
}