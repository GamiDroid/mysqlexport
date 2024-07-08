using MySql.Data.MySqlClient;
using mysqlexport.SchemaRetriever;

namespace mysqlexport;
internal class SqlSchemaExporter(string outputDirectory)
{
    private readonly string _outputDirectory = outputDirectory;

    public async Task ExportAsync(
        string folderName,
        ISchemaRetriever schemaRetriever)
    {
        Console.WriteLine($"start exporting {folderName}...");
        var objectsFolder = Path.Combine(_outputDirectory, folderName);
        if (!Directory.Exists(objectsFolder))
        {
            Directory.CreateDirectory(objectsFolder);
        }

        var objectNames = await schemaRetriever.GetListAsync();
        foreach (var objectName in objectNames)
        {
            var createStatement = await schemaRetriever.GetCreateStatementAsync(objectName);

            var fileName = $"{objectName}.sql";
            var filePath = Path.GetFullPath(Path.Combine(objectsFolder, fileName));

            await File.WriteAllTextAsync(filePath, createStatement);
        }
        Console.WriteLine($"exported {objectNames.Count} {folderName} to folder {objectsFolder}");
    }

    public void PruneFolder(string folderName)
    {
        var directoryPath = Path.Combine(_outputDirectory, folderName);

        if (Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Deleting '{folderName}' folder...");
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
