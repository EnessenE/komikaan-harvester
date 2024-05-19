using komikaan.Harvester.Enums;

namespace komikaan.Harvester.Models;

public class SupplierConfiguration
{
    public required RetrievalType RetrievalType { get; set; }
    public required SupplierType DataType { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
}