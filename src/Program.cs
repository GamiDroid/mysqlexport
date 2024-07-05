
using Dapper;
using MySql.Data.MySqlClient;

var connectionString = "";
using var connection = new MySqlConnection(connectionString);

await ExportAsync(connection, "tables", GetTableNamesAsync, GetCreateTableStatementAsync);
await ExportAsync(connection, "views", GetViewNamesAsync, GetCreateViewStatementAsync);
await ExportAsync(connection, "functions", GetFunctionsAsync, GetCreateFunctionStatementAsync);
await ExportAsync(connection, "procedures", GetProceduresAsync, GetCreateProcedureStatementAsync);

Console.WriteLine($"start exporting triggers");
var tiggersFolder = Path.Combine(".", "triggers");
if (!Directory.Exists(tiggersFolder))
{
    Directory.CreateDirectory(tiggersFolder);
}

//TODO: Clear folder before export
// prune flag

var triggers = (await GetTriggersAsync(connection)).ToList();
foreach (var trigger in triggers)
{
    var createStatement = trigger.Statement;

    var fileName = $"{trigger.Trigger}.sql";
    var filePath = Path.GetFullPath(Path.Combine(tiggersFolder, fileName));

    await File.WriteAllTextAsync(filePath, createStatement);
}
Console.WriteLine($"exported {triggers.Count} triggers to folder {tiggersFolder}");

async Task ExportAsync(
    MySqlConnection connection,
    string folderName,
    Func<MySqlConnection, Task<IEnumerable<ObjectName>>> objectsGetter, 
    Func<MySqlConnection, string, Task<string>> createStatementGetter)
{
    Console.WriteLine($"start exporting {folderName}");
    var objectsFolder = Path.Combine(".", folderName);
    if (!Directory.Exists(objectsFolder))
    {
        Directory.CreateDirectory(objectsFolder);
    }

    var objects = (await objectsGetter(connection)).ToList();
    foreach (var table in objects)
    {
        var createStatement = await createStatementGetter(connection, table.Name);

        var fileName = $"{table.Name}.sql";
        var filePath = Path.GetFullPath(Path.Combine(objectsFolder, fileName));

        await File.WriteAllTextAsync(filePath, createStatement);
    }
    Console.WriteLine($"exported {objects.Count} {folderName}s to folder {objectsFolder}");
}

async Task<string> GetCreateTableStatementAsync(MySqlConnection connection, string objectName)
{
    var sql = $"SHOW CREATE TABLE `{objectName}`";
    var reader = await connection.ExecuteReaderAsync(new CommandDefinition(sql));

    await reader.ReadAsync();
    var result = reader.GetString(reader.GetOrdinal("Create Table"));
    await reader.CloseAsync();

    return result;
}

async Task<string> GetCreateViewStatementAsync(MySqlConnection connection, string objectName)
{
    var sql = $"SHOW CREATE VIEW `{objectName}`";
    var reader = await connection.ExecuteReaderAsync(new CommandDefinition(sql));

    await reader.ReadAsync();
    var result = reader.GetString(reader.GetOrdinal("Create View"));
    await reader.CloseAsync();

    return result;
}

async Task<string> GetCreateFunctionStatementAsync(MySqlConnection connection, string objectName)
{
    var sql = $"SHOW CREATE FUNCTION `{objectName}`";
    var reader = await connection.ExecuteReaderAsync(new CommandDefinition(sql));

    await reader.ReadAsync();
    var result = reader.GetString(reader.GetOrdinal("Create Function"));
    await reader.CloseAsync();

    return result;
}

async Task<string> GetCreateProcedureStatementAsync(MySqlConnection connection, string objectName)
{
    var sql = $"SHOW CREATE PROCEDURE `{objectName}`";
    var reader = await connection.ExecuteReaderAsync(new CommandDefinition(sql));

    await reader.ReadAsync();
    var result = reader.GetString(reader.GetOrdinal("Create Procedure"));
    await reader.CloseAsync();

    return result;
}

Task<IEnumerable<ObjectName>> GetFunctionsAsync(MySqlConnection connection)
{
    var sql = "SHOW FUNCTION STATUS WHERE `Db`='merba'";
    return connection.QueryAsync<ObjectName>(sql);
}

Task<IEnumerable<ObjectName>> GetProceduresAsync(MySqlConnection connection)
{
    var sql = "SHOW PROCEDURE STATUS WHERE `Db`='merba'";
    return connection.QueryAsync<ObjectName>(sql);
}

Task<IEnumerable<TriggerDto>> GetTriggersAsync(MySqlConnection connection)
{
    var sql = "SHOW TRIGGERS";
    return connection.QueryAsync<TriggerDto>(sql);
}

Task<IEnumerable<ObjectName>> GetTableNamesAsync(MySqlConnection connection)
{
    var sql = "SHOW TABLE STATUS WHERE COMMENT != 'VIEW'";
    return connection.QueryAsync<ObjectName>(sql);
}

Task<IEnumerable<ObjectName>> GetViewNamesAsync(MySqlConnection connection)
{
    var sql = "SHOW TABLE STATUS WHERE COMMENT = 'VIEW'";
    return connection.QueryAsync<ObjectName>(sql);
}

public class TriggerDto
{
#nullable disable
    public string Trigger { get; set; }
    public string Event { get; set; }
    public string Table { get; set; }
    public string Statement { get; set; }
    public string Timing { get; set; }
#nullable restore
}

public class ObjectName
{
#nullable disable
    public string Name { get; set; }
#nullable restore

    public override string ToString() => Name;
}
