using komikaan.GTFS.Models.Static.Enums;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// PostgreSQL version of StopTime with direct mappings for DB columns.
    /// </summary>
    public class PSQLStopTime : StopTime
    {

        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;

        /// <summary>
        /// Maps to stop_times_type.arrival_time_data (time of day). GTFS times can exceed 24:00:00,
        /// the whole days are split off into <see cref="DaysSinceStartArrival"/> so the remainder fits in a PG time.
        /// </summary>
        public TimeSpan? ArrivalTimeData
        {
            get
            {
                var tempValue = base.ArrivalTime;
                if (tempValue != null && tempValue.Value.Days > 0)
                {
                    DaysSinceStartArrival = tempValue.Value.Days;
                    tempValue = tempValue.Value.Subtract(TimeSpan.FromDays(DaysSinceStartArrival));
                }
                return tempValue;
            }
            set => base.ArrivalTime = value;
        }

        /// <summary>
        /// Maps to stop_times_type.departure_time_data (time of day), see <see cref="ArrivalTimeData"/>.
        /// </summary>
        public TimeSpan? DepartureTimeData
        {
            get
            {
                var tempValue = base.DepartureTime;
                if (tempValue != null && tempValue.Value.Days > 0)
                {
                    DaysSinceStartDeparture = tempValue.Value.Days;
                    tempValue = tempValue.Value.Subtract(TimeSpan.FromDays(DaysSinceStartDeparture));
                }
                return tempValue;
            }
            set => base.DepartureTime = value;
        }

        public double StopSequenceData
        {
            get => StopSequence;
            set => StopSequence = value;
        }
        public double? ShapeDistTravelled
        {
            get => ShapeDistTraveled;
            set => ShapeDistTraveled = value;
        }

        public int DaysSinceStartArrival
        {
            get;
            set;
        } = 0;

        public int DaysSinceStartDeparture
        {
            get;
            set;
        } = 0;


        public int? PickupTypeData
        {
            get => (int?)PickupType;
            set => PickupType = (PickupType?)value;
        }
        public int? DropOffTypeData
        {
            get => (int?)DropOffType;
            set => DropOffType = (DropOffType?)value;
        }
        public int? TimepointTypeData
        {
            get => (int?)Timepoint;
            set => Timepoint = (Timepoint?)value;
        }
    }
}
