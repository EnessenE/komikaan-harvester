using CsvHelper.Configuration;
using komikaan.Harvester.Adapters;
namespace komikaan.Harvester.Suppliers;


public partial class GenericGTFSSupplier
{
    public class StopMap : ClassMap<PSQLStop>
    {
        public StopMap()
        {
            Map(m => m.StopId).Name("stop_id");
            Map(m => m.Code).Name("stop_code");
            Map(m => m.Name).Name("stop_name");
            Map(m => m.Description).Name("stop_desc");
            Map(m => m.Latitude).Name("stop_lat");
            Map(m => m.Longitude).Name("stop_lon");
            Map(m => m.Zone).Name("zone_id");
            Map(m => m.Url).Name("stop_url");
            Map(m => m.LocationType).Name("location_type");
            Map(m => m.ParentStation).Name("parent_station");
            Map(m => m.Timezone).Name("stop_timezone");
            Map(m => m.WheelchairBoarding).Name("wheelchair_boarding");
            Map(m => m.LevelId).Name("level_id");
            Map(m => m.PlatformCode).Name("platform_code");
        }
    }
}