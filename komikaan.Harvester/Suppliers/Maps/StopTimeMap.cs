using CsvHelper.Configuration;
using komikaan.GTFS.Models.Static.Models;
namespace komikaan.Harvester.Suppliers;


public partial class GenericGTFSSupplier
{
    public class StopTimeMap : ClassMap<StopTime>
    {
        public StopTimeMap()
        {
            Map(m => m.TripId).Name("trip_id");
            Map(m => m.ArrivalTime).Name("arrival_time").TypeConverter<GtfsTimeConverter>();
            Map(m => m.DepartureTime).Name("departure_time").TypeConverter<GtfsTimeConverter>();
            Map(m => m.StopId).Name("stop_id");
            Map(m => m.StopSequence).Name("stop_sequence");
            Map(m => m.StopHeadsign).Name("stop_headsign");
            Map(m => m.PickupType).Name("pickup_type");
            Map(m => m.DropOffType).Name("drop_off_type");
            Map(m => m.ContinuousPickup).Name("continuous_pickup");
            Map(m => m.ContinuousDropOff).Name("continuous_drop_off");
            Map(m => m.ShapeDistTraveled).Name("shape_dist_traveled");
            Map(m => m.Timepoint).Name("timepoint");
        }
    }
}