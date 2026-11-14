using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// PostgreSQL version of Frequency with direct mappings for DB columns and import metadata.
    /// </summary>
    public class PSQLFrequency : komikaan.GTFS.Models.Static.Models.Frequency
    {
        private TimeSpan? _startTime;
        private TimeSpan? _endTime;

        /// <summary>
        /// Maps to frequencies_type.start_time_data (time of day). Whole days are wrapped off
        /// because a PG time cannot exceed 24:00:00 and frequencies_type has no days column.
        /// </summary>
        public TimeSpan? StartTimeData
        {
            get => WrapToTimeOfDay(_startTime);
            set => _startTime = value;
        }

        /// <summary>
        /// Maps to frequencies_type.end_time_data (time of day), see <see cref="StartTimeData"/>.
        /// </summary>
        public TimeSpan? EndTimeData
        {
            get => WrapToTimeOfDay(_endTime);
            set => _endTime = value;
        }

        /// <summary>
        /// Maps to frequencies_type.exact_times_data
        /// </summary>
        public int? ExactTimesData { get; set; }

        private static TimeSpan? WrapToTimeOfDay(TimeSpan? value)
        {
            if (value != null && value.Value.Days > 0)
            {
                return value.Value.Subtract(TimeSpan.FromDays(value.Value.Days));
            }
            return value;
        }

        /// <summary>
        /// Maps to frequencies_type.data_origin
        /// </summary>
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;

        /// <summary>
        /// Maps to frequencies_type.internal_id
        /// </summary>
        public Guid InternalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Maps to frequencies_type.last_updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Maps to frequencies_type.import_id
        /// </summary>
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;
    }
}
