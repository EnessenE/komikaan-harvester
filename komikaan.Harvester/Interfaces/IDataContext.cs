using komikaan.Common.Models;
using komikaan.Harvester.Suppliers;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    Task ImportAsync(GTFSFeed feed);
    Task MarkDownloadAsync(SupplierConfiguration config, bool success);
    Task CleanOldStopDataAsync(SupplierConfiguration config);
    Task DeleteOldDataAsync(SupplierConfiguration config);
    Task<List<SupplierTypeMapping>?> GetTypeMappingsAsync(SupplierConfiguration config);
    Task MarkStartImportAsync(SupplierConfiguration config);
    Task UpdateImportStatusAsync(SupplierConfiguration config, string importStatus);
}