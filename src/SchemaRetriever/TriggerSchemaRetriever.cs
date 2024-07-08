using Dapper;
using MySql.Data.MySqlClient;

namespace mysqlexport.SchemaRetriever;
internal class TriggerSchemaRetriever(MySqlConnection connection) : ISchemaRetriever
{
    private readonly MySqlConnection _connection = connection;
    private Dictionary<string, TriggerDto>? _triggers;

    public async Task<string> GetCreateStatementAsync(string objectName)
    {
        var triggers = await GetTriggersAsync();
        return triggers[objectName].Statement;
    }

    public async Task<List<string>> GetListAsync()
    {
        var triggers = await GetTriggersAsync();
        return triggers.Select(x => x.Key).ToList();
    }

    private async ValueTask<Dictionary<string, TriggerDto>> GetTriggersAsync()
    {
        if (_triggers == null)
        {
            var sql = "SHOW TRIGGERS";
            var triggers = await _connection.QueryAsync<TriggerDto>(sql);
            _triggers = triggers.ToDictionary(x => x.Trigger);
        }

        return _triggers;
    }
}
