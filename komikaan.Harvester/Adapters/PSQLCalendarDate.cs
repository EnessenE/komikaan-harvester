using komikaan.GTFS.Models.Static.Enums;
using komikaan.Harvester.Models;

namespace komikaan.GTFS.Models.Static.Models
{
    /// <summary>
    /// PostgreSQL version of CalendarDate with direct mappings for DB columns and import metadata.
    /// </summary>
    public class PSQLCalendarDate : CalendarDate
    {
        public string ExceptionTypeData { get=> base.ExceptionType.ToString(); }

        // Always-present tracking fields
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;
        public Guid InternalId { get; set; } = Guid.NewGuid();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;
    }
}
