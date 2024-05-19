using GTFS;

namespace komikaan.Harvester.Interfaces;

public interface IDataContext
{
    public Task ImportAsync(GTFSFeed feed);
}