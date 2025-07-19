using komikaan.GTFS.Models.Static.Enums;
using komikaan.Harvester.Models;

namespace komikaan.GTFS.Models.Static.Models
{
    /// <summary>
    /// PostgreSQL version of CalendarDate with direct mappings for DB columns and import metadata.
    /// </summary>
    public class PSQLCalendar : Calendar
    {

        public short  Mask { get; set; } = 0;
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
