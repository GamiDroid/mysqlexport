using Dapper;
using MySql.Data.MySqlClient;

namespace mysqlexport.SchemaRetriever;
internal class ViewSchemaRetriever(MySqlConnection connection) : ISchemaRetriever
{
    private readonly MySqlConnection _connection = connection;

    public async Task<string> GetCreateStatementAsync(string objectName)
    {
        var sql = $"SHOW CREATE VIEW `{objectName}`";
        var reader = await _connection.ExecuteReaderAsync(new CommandDefinition(sql));

        await reader.ReadAsync();
        var result = reader.GetString(reader.GetOrdinal("Create View"));
        await reader.CloseAsync();

        return result;
    }

    public async Task<List<string>> GetListAsync()
    {
        var sql = "SHOW TABLE STATUS WHERE COMMENT = 'VIEW'";
        var objectNames = await _connection.QueryAsync<ObjectName>(sql);
        return objectNames.Select(x => x.Name).ToList();
    }
}
