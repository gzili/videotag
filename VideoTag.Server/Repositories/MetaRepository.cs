using Dapper;
using VideoTag.Server.Constants;
using VideoTag.Server.Contexts;

namespace VideoTag.Server.Repositories;

public interface IMetaRepository
{
    Task<bool> IsFlagSet(MetaFlag flag);
    Task ClearFlag(MetaFlag flag);
}

public class MetaRepository(DapperContext dapperContext) : IMetaRepository
{
    public async Task<bool> IsFlagSet(MetaFlag flag)
    {
        const string sql = "SELECT Value FROM Meta WHERE Name = @name";
        using var connection = dapperContext.CreateConnection();
        var value = await connection.QueryFirstOrDefaultAsync<string>(sql, new { name = flag.ToString() });
        return value == "1";
    }

    public async Task ClearFlag(MetaFlag flag)
    {
        const string sql = "DELETE FROM Meta WHERE Name = @name";
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { name = flag.ToString() });
    }
}
