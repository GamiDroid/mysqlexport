using Dapper;
using MySql.Data.MySqlClient;

namespace mysqlexport.SchemaRetriever;
internal class FunctionSchemaRetriever(MySqlConnection connection) : ISchemaRetriever
{
    private readonly MySqlConnection _connection = connection;

    public async Task<string> GetCreateStatementAsync(string objectName)
    {
        var sql = $"SHOW CREATE FUNCTION `{objectName}`";
        var reader = await _connection.ExecuteReaderAsync(new CommandDefinition(sql));

        await reader.ReadAsync();
        var result = reader.GetString(reader.GetOrdinal("Create Function"));
        await reader.CloseAsync();

        return result;
    }

    public async Task<List<string>> GetListAsync()
    {
        var sql = "SHOW FUNCTION STATUS WHERE `Db`='merba'";
        var objectNames = await _connection.QueryAsync<ObjectName>(sql);
        return objectNames.Select(x => x.Name).ToList();
    }
}
