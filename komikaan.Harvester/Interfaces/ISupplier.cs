using GTFS;
using komikaan.Common.Models;

namespace komikaan.Harvester.Interfaces
{
    public interface ISupplier
    {
        public Task<GTFSFeed> GetFeedAsync();
        public SupplierConfiguration GetConfiguration();
    }
}
