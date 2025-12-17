using komikaan.GTFS.Models.Static.Enums;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;
using Newtonsoft.Json.Linq;

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

        public new TimeSpan? ArrivalTimeData
        {
            get
            {
                var tempValue = base.ArrivalTime;
                if (base.ArrivalTime != null && base.ArrivalTime?.Days > 0)
                {
                    DaysSinceStartArrival = base.ArrivalTime.Value.Days;
                    tempValue= tempValue?.Subtract(TimeSpan.FromDays(DaysSinceStartArrival));
                }
                return tempValue;
            }

            set
            {
                var tempValue = value;
                if (base.ArrivalTime != null && base.ArrivalTime?.Days > 0)
                {
                    DaysSinceStartArrival = base.ArrivalTime.Value.Days;
                    tempValue= tempValue?.Subtract(TimeSpan.FromDays(DaysSinceStartArrival));
                }
                base.ArrivalTime = tempValue;
            }
        }

        public new TimeSpan? DepartureTimeData
        {
            get
            {
                var tempValue = base.DepartureTime;
                if (base.ArrivalTime != null && base.DepartureTime?.Days > 0)
                {
                    DaysSinceStartDeparture = base.DepartureTime.Value.Days;
                    tempValue = tempValue?.Subtract(TimeSpan.FromDays(DaysSinceStartDeparture));
                }
                return tempValue;
            }

            set
            {
                var tempValue = value;
                if (base.DepartureTime != null && base.DepartureTime?.Days > 0)
                {
                    DaysSinceStartDeparture = base.DepartureTime.Value.Days;
                    tempValue = tempValue?.Subtract(TimeSpan.FromDays(DaysSinceStartDeparture));
                }
                base.DepartureTime = tempValue;
            }
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
