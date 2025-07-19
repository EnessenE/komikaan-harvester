using komikaan.GTFS.Models.Static.Enums;
using komikaan.Harvester.Models;

namespace komikaan.GTFS.Models.Static.Models
{
    /// <summary>
    /// PostgreSQL version of CalendarDate with direct mappings for DB columns and import metadata.
    /// </summary>
    public class PSQLRoute : Route
    {
        /// <summary>
        /// Maps GTFS route_id to routes_type.id
        /// </summary>
        public string Id { get => RouteId; set => RouteId = value; }

        /// <summary>
        /// Maps GTFS route_short_name to routes_type.short_name
        /// </summary>
        public string? ShortName { get => RouteShortName; set => RouteShortName = value; }

        /// <summary>
        /// Maps GTFS route_long_name to routes_type.long_name
        /// </summary>
        public string? LongName { get => RouteLongName; set => RouteLongName = value; }

        /// <summary>
        /// Maps GTFS route_desc to routes_type.description
        /// </summary>
        public string? Description { get => RouteDesc; set => RouteDesc = value; }

        /// <summary>
        /// Maps GTFS route_url to routes_type.url
        /// </summary>
        public string? Url { get => RouteUrl; set => RouteUrl = value; }

        /// <summary>
        /// Maps GTFS route_color to routes_type.color
        /// </summary>
        public string? Color { get => RouteColor; set => RouteColor = value; }

        /// <summary>
        /// Maps GTFS route_text_color to routes_type.text_color
        /// </summary>
        public string? TextColor { get => RouteTextColor; set => RouteTextColor = value; }

        /// <summary>
        /// Maps GTFS RouteType to routes_type.route_type
        /// </summary>
        public int RouteTypeData { get => (int)RouteType; set => RouteType = (RouteType)value; }

        /// <summary>
        /// Maps to routes_type.data_origin
        /// </summary>
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;

        /// <summary>
        /// Maps to routes_type.internal_id
        /// </summary>
        public Guid InternalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Maps to routes_type.last_updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Maps to routes_type.import_id
        /// </summary>
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;
    }
}
