using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// PostgreSQL version of Shape with direct mappings for DB columns.
    /// </summary>
    public class PSQLShape : Shape
    {
        /// <summary>
        /// Maps GTFS shape_id to shapes_type.id
        /// </summary>
        public string Id { get => ShapeId; set => ShapeId = value; }

        /// <summary>
        /// Maps GTFS shape_pt_sequence to shapes_type.sequence_data
        /// </summary>
        public double SequenceData { get => ShapePtSequence; set => ShapePtSequence = (int)value; }

        /// <summary>
        /// Maps GTFS shape_pt_lat to shapes_type.latitude
        /// </summary>
        public double Latitude { get => ShapePtLat; set => ShapePtLat = value; }

        /// <summary>
        /// Maps GTFS shape_pt_lon to shapes_type.longitude
        /// </summary>
        public double Longitude { get => ShapePtLon; set => ShapePtLon = value; }

        /// <summary>
        /// Maps GTFS shape_dist_traveled to shapes_type.distance_travelled
        /// </summary>
        public double? DistanceTravelled { get => ShapeDistTraveled; set => ShapeDistTraveled = value; }

        /// <summary>
        /// Maps to shapes_type.data_origin
        /// </summary>
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;

        /// <summary>
        /// Maps to shapes_type.internal_id
        /// </summary>
        public Guid InternalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Maps to shapes_type.last_updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Maps to shapes_type.import_id
        /// </summary>
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;
    }
}
