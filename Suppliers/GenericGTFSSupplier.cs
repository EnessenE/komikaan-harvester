using GTFS;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Suppliers;

public class GenericGTFSSupplier : ISupplier
{
    private readonly SupplierConfiguration _supplierConfig;

    public GenericGTFSSupplier(SupplierConfiguration supplierConfig)
    {
        _supplierConfig = supplierConfig;
    }
    public Task<GTFSFeed> GetFeedAsync()
    {
        var reader = new GTFSReader<GTFSFeed>(false);
        var feed = reader.Read(_supplierConfig.Url);

        foreach (var agency in feed.Agencies)
        {
            Console.WriteLine(agency.Name);
        }
        Console.WriteLine($"Found a feed with {feed.Agencies.Count} agencies");
        return Task.FromResult(feed);
    }

    public SupplierConfiguration GetConfiguration()
    {
        return _supplierConfig;
    }
}