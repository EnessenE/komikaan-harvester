using CsvHelper.Configuration;
using komikaan.Harvester.Adapters;

namespace komikaan.Harvester.Suppliers;

public class TripMap : ClassMap<PSQLTrip>
{
    public TripMap()
    {
        Map(m => m.TripId).Name("trip_id");
        Map(m => m.RouteId).Name("route_id");
        Map(m => m.ServiceId).Name("service_id");
        Map(m => m.TripHeadsign).Name("trip_headsign");
        Map(m => m.TripShortName).Name("trip_short_name");
        Map(m => m.DirectionType).Name("direction_id");
        Map(m => m.BlockId).Name("block_id");
        Map(m => m.ShapeId).Name("shape_id");
        Map(m => m.AccessibilityTypeData).Name("wheelchair_accessible");
        Map(m => m.BikesAllowedData).Name("bikes_allowed");
    }
}
