using komikaan.Common.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    Task MarkDownloadFailure(ImportRequest config);
    Task DeleteOldDataAsync(ImportRequest config);
    Task<List<SupplierTypeMapping>?> GetTypeMappingsAsync(ImportRequest config);
    Task MarkStartImportAsync(ImportRequest config);
    Task MarkSuccessImportAsync(ImportRequest config);
    Task UpdateImportStatusAsync(ImportRequest config, string importStatus);
    Task BuildGTFSDatesAsync(ImportRequest config);
}