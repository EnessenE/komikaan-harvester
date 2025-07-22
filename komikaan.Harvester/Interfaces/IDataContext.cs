using komikaan.Common.Models;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    Task MarkDownloadFailure(SupplierConfiguration config);
    Task CleanOldStopDataAsync(SupplierConfiguration config);
    Task DeleteOldDataAsync(SupplierConfiguration config);
    Task<List<SupplierTypeMapping>?> GetTypeMappingsAsync(SupplierConfiguration config);
    Task MarkStartImportAsync(SupplierConfiguration config);
    Task MarkSuccessImportAsync(SupplierConfiguration config);
    Task UpdateImportStatusAsync(SupplierConfiguration config, string importStatus);
}