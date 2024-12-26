using GTFS.Entities.Enumerations;
using StopTime = GTFS.Entities.StopTime;

namespace komikaan.Harvester.Contexts
{
    internal class PsqlStopTime : StopTime
    {
        public PsqlStopTime() { }

        public PsqlStopTime(StopTime item)
        {
            StopId = item.StopId;
            TripId = item.TripId;
            StopSequence = item.StopSequence;
            StopHeadsign = item.StopHeadsign;
            PickupType = item.PickupType;
            DropOffType = item.DropOffType;
            ShapeDistTravelled = item.ShapeDistTravelled;
            TimepointType = item.TimepointType;
            ArrivalTime = item.ArrivalTime;
            DepartureTime = item.DepartureTime;
            TimepointType = item.TimepointType;
        }

        public double StopSequenceData
        {
            get => (double)base.StopSequence;
            set => base.StopSequence = (uint)value;
        }

        public int PickupTypeData
        {
            get => (int)base.PickupType.GetValueOrDefault();
            set => base.PickupType = (PickupType)value;
        }

        public int DropOffTypeData
        {
            get => (int)base.DropOffType.GetValueOrDefault();
            set => base.DropOffType = (DropOffType)value;
        }

        public int TimepointTypeData
        {
            get => (int)base.TimepointType;
            set => base.TimepointType = (TimePointType)value;
        }
    }
}
