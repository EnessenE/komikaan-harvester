using komikaan.GTFS.Models.Static.Models;

namespace komikaan.Harvester.Suppliers;

public class GTFSFeed
{
    public List<Agency> Agencies { get; set; }
    public List<StopTime> StopTimes { get; internal set; }
    public List<Trip> Trips { get; internal set; }
    public IEnumerable<Stop> Stops { get; internal set; }
}
