using GTFS.Entities.Enumerations;
using StopTime = GTFS.Entities.StopTime;

namespace komikaan.Harvester.Contexts.ORM
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
            get => StopSequence;
            set => StopSequence = (uint)value;
        }

        public int PickupTypeData
        {
            get => (int)PickupType.GetValueOrDefault();
            set => PickupType = (PickupType)value;
        }

        public int DropOffTypeData
        {
            get => (int)DropOffType.GetValueOrDefault();
            set => DropOffType = (DropOffType)value;
        }

        public int TimepointTypeData
        {
            get => (int)TimepointType;
            set => TimepointType = (TimePointType)value;
        }
    }
}
