using CsvHelper.Configuration;
using komikaan.Harvester.Adapters;

namespace komikaan.Harvester.Suppliers;
public class ShapeMap : ClassMap<PSQLShape>
{
    public ShapeMap()
    {
        Map(m => m.ShapeId).Name("shape_id");
        Map(m => m.Id).Name("shape_id");
        Map(m => m.ShapePtSequence).Name("shape_pt_sequence");
        Map(m => m.ShapePtLat).Name("shape_pt_lat");
        Map(m => m.ShapePtLon).Name("shape_pt_lon");
        Map(m => m.ShapeDistTraveled).Name("shape_dist_traveled");
    }
}
