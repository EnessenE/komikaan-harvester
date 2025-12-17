using komikaan.GTFS.Models.Static.Enums;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// PostgreSQL version of Stop with direct mappings for DB columns.
    /// </summary>
    public class PSQLStop : Stop
    {
        /// <summary>
        /// Maps to stops_type.id
        /// </summary>
        public string Id { get => StopId; set => StopId = value; }

        /// <summary>
        /// Maps to stops_type.code
        /// </summary>
        public string? Code { get => StopCode; set => StopCode = value; }

        /// <summary>
        /// Maps to stops_type.name
        /// </summary>
        public string Name { get => StopName; set => StopName = value; }

        /// <summary>
        /// Maps to stops_type.description
        /// </summary>
        public string? Description { get => StopDesc; set => StopDesc = value; }

        /// <summary>
        /// Maps to stops_type.latitude
        /// </summary>
        public double Latitude { get => StopLat; set => StopLat = value; }

        /// <summary>
        /// Maps to stops_type.longitude
        /// </summary>
        public double Longitude { get => StopLon; set => StopLon = value; }

        /// <summary>
        /// Maps to stops_type.zone
        /// </summary>
        public string? Zone { get => ZoneId; set => ZoneId = value; }

        /// <summary>
        /// Maps to stops_type.url
        /// </summary>
        public string? Url { get => StopUrl; set => StopUrl = value; }

        /// <summary>
        /// Maps to stops_type.location_type_data
        /// </summary>
        public int? LocationTypeData
        {
            get => (int?)LocationType;
            set => LocationType = value.HasValue ? (LocationType?)value : null;
        }

        /// <summary>
        /// Maps to stops_type.parent_station
        /// </summary>
        public string? ParentStationData { get => ParentStation; set => ParentStation = value; }

        /// <summary>
        /// Maps to stops_type.timezone
        /// </summary>
        public string? Timezone { get => StopTimezone; set => StopTimezone = value; }

        /// <summary>
        /// Maps to stops_type.wheelchair_boarding_data
        /// </summary>
        public int? WheelchairBoardingData
        {
            get => (int?)WheelchairBoarding;
            set => WheelchairBoarding = value.HasValue ? (WheelchairBoarding?)value : null;
        }

        /// <summary>
        /// Maps to stops_type.level_id
        /// </summary>
        public string? LevelIdData { get => LevelId; set => LevelId = value; }

        /// <summary>
        /// Maps to stops_type.platform_code
        /// </summary>
        public string? PlatformCodeData { get => PlatformCode; set => PlatformCode = value; }

        /// <summary>
        /// Maps to stops_type.stop_type_data
        /// </summary>
        public int StopTypeData { get=> (int)StopType ; set=>StopType = (RouteType)value; }

        /// <summary>
        /// Maps to stops_type.data_origin
        /// </summary>
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;

        /// <summary>
        /// Maps to stops_type.last_updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Maps to stops_type.import_id
        /// </summary>
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;


        public RouteType StopType { get; set; } = RouteType.Funicular;
    }
}
