using GTFS;
using komikaan.Common.Models;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    public Task ImportAsync(GTFSFeed feed);
    Task MarkDownload(SupplierConfiguration config);
}