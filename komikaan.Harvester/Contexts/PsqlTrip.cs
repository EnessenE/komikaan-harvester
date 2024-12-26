using GTFS.Entities;
using GTFS.Entities.Enumerations;

namespace komikaan.Harvester.Contexts
{
    internal class PsqlTrip : Trip
    {
        public PsqlTrip() { }

        public PsqlTrip(Trip item)
        {
            // Copying properties from the original Trip object
            Id = item.Id;
            RouteId = item.RouteId;
            ServiceId = item.ServiceId;
            Headsign = item.Headsign;
            ShortName = item.ShortName;
            Direction = item.Direction;
            BlockId = item.BlockId;
            ShapeId = item.ShapeId;
            AccessibilityType = item.AccessibilityType;
            InternalId = item.InternalId;
            DataOrigin = item.DataOrigin;
        }

        // Mapping for DirectionType enum (e.g., 0 = North, 1 = South)
        public int DirectionType
        {
            get => (int)(base.Direction ?? GTFS.Entities.Enumerations.DirectionType.OneDirection); // Default to 'Unknown' if null
            set => base.Direction = (DirectionType)value;
        }

        // Mapping for WheelchairAccessibilityType enum (e.g., 0 = Unknown, 1 = Accessible, 2 = Not Accessible)
        public int AccessibilityTypeData
        {
            get => (int)(base.AccessibilityType ?? WheelchairAccessibilityType.NoInformation); // Default to 'Unknown' if null
            set => base.AccessibilityType = (WheelchairAccessibilityType)value;
        }
    }
}