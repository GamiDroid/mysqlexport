using Cocona;
using MySql.Data.MySqlClient;
using mysqlexport;
using mysqlexport.SchemaRetriever;

var app = CoconaApp.Create();

// primary command:
app.AddCommand(async (
    [Option('c', Description = "Connection string to MySQL database")] string connectionString,
    [Option('o', Description = "Output directory (Default: Current working directory)")] string? output,
    [Option('p', Description = "Delete all files inside the output directory before exporting")] bool prune) =>
{
    using var connection = new MySqlConnection(connectionString);

    var outputDirectory = !string.IsNullOrWhiteSpace(output) ? output : ".";

    var exporter = new SqlSchemaExporter(outputDirectory);

    if (Directory.Exists(outputDirectory) && prune)
    {
        Console.WriteLine("Prune option active. Delete existing folders:");
        exporter.PruneFolder("tables");
        exporter.PruneFolder("views");
        exporter.PruneFolder("functions");
        exporter.PruneFolder("procedures");
        exporter.PruneFolder("triggers");
    }

    await exporter.ExportAsync("tables", new TableSchemaRetriever(connection));
    await exporter.ExportAsync("views", new ViewSchemaRetriever(connection));
    await exporter.ExportAsync("functions", new FunctionSchemaRetriever(connection));
    await exporter.ExportAsync("procedures", new ProcedureSchemaRetriever(connection));
    await exporter.ExportAsync("triggers", new TriggerSchemaRetriever(connection));
})
.WithDescription("Export MySql schema to *.sql files");

//todo: add subcommands for setting configuration 
//app.AddSubCommand("config", x =>
//{
//    x.AddCommand("set", () => { Console.WriteLine("config-set"); });
//    x.AddCommand("get", () => { Console.WriteLine("config-get"); });
//});

app.Run();
