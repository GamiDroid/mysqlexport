namespace mysqlexport.SchemaRetriever;
internal interface ISchemaRetriever
{
    Task<string> GetCreateStatementAsync(string objectName);
    Task<List<string>> GetListAsync();
}
