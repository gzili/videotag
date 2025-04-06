using Dapper;
using VideoTag.Server.Contexts;

namespace VideoTag.Server.Repositories;

public interface IMetaRepository
{
    Task<bool> IsRebuildNeeded();
    Task ClearRebuildNeeded();
}

public class MetaRepository(DapperContext dapperContext) : IMetaRepository
{
    private const string RebuildNeeded = "RebuildNeeded";
    
    public async Task<bool> IsRebuildNeeded()
    {
        var value = await GetMetaValue(RebuildNeeded);
        return value == "1";
    }

    public async Task ClearRebuildNeeded()
    {
        await UpdateMetaValue(RebuildNeeded, "0");
    }

    private async Task<string> GetMetaValue(string name)
    {
        const string sql = "SELECT Value FROM Meta WHERE Name = @name";
        using var connection = dapperContext.CreateConnection();
        var value = await connection.QueryFirstAsync<string>(sql, new { name });
        return value;
    }

    private async Task UpdateMetaValue(string name, string value)
    {
        const string sql = "Update Meta SET Value = @value WHERE Name = @name";
        using var connection = dapperContext.CreateConnection();
        await connection.ExecuteAsync(sql, new { name, value });
    }
}