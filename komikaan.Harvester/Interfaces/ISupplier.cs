using komikaan.Common.Models;
using komikaan.Harvester.Suppliers;

namespace komikaan.Harvester.Interfaces
{
    public interface ISupplier
    {
        public Task<GTFSFeed> GetFeedAsync();
        public SupplierConfiguration GetConfiguration();
    }
}
