namespace mysqlexport;
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
