using GTFS;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Interfaces
{
    public interface ISupplier
    {
        public Task<GTFSFeed> GetFeedAsync();
        public SupplierConfiguration GetConfiguration();
    }
}
