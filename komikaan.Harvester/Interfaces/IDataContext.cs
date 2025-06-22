using komikaan.Common.Models;
using komikaan.Harvester.Suppliers;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    public Task ImportAsync(GTFSFeed feed);
    Task MarkDownload(SupplierConfiguration config, bool success);
    Task CleanOldStopData(SupplierConfiguration config);
    Task DeleteOldDataAsync(SupplierConfiguration config);
    Task<List<SupplierTypeMapping>?> GetTypeMappingsAsync(SupplierConfiguration config);
}