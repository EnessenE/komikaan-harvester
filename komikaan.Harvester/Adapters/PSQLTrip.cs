using komikaan.GTFS.Models.Static.Enums;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// PostgreSQL version of Trip with direct mappings for DB columns.
    /// </summary>
    public class PSQLTrip : Trip
    {

        public string Id { get => TripId; set => TripId = value; }
        public string? Headsign { get => TripHeadsign; set => TripHeadsign = value; }
        public string? ShortName { get => TripShortName; set => TripShortName = value; }

        /// <summary>
        /// Maps to trips_type.direction_type
        /// </summary>
        public int? DirectionType { get => (int?)DirectionId; set => DirectionId = (Direction?)value; }

        /// <summary>
        /// Maps to trips_type.accessibility_type_data
        /// </summary>
        public int? AccessibilityTypeData { get => (int?)WheelchairAccessible; set => WheelchairAccessible = (WheelchairAccessible?)value; }

        /// <summary>
        /// Optional: If you want to store bikes_allowed as int
        /// </summary>
        public int? BikesAllowedData { get => (int?)BikesAllowed; set => BikesAllowed = (BikesAllowed?)value; }

        /// <summary>
        /// Maps to trips_type.data_origin
        /// </summary>
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;

        /// <summary>
        /// Maps to trips_type.internal_id
        /// </summary>
        public Guid InternalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Maps to trips_type.last_updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Maps to trips_type.import_id
        /// </summary>
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;
    }
}
