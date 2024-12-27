using GTFS.Entities;
using GTFS.Entities.Enumerations;

namespace komikaan.Harvester.Contexts.ORM
{
    internal class PsqlStop : Stop
    {
        public PsqlStop() { }

        public PsqlStop(Stop item)
        {
            InternalId = item.InternalId;
            DataOrigin = item.DataOrigin;
            Id = item.Id;
            Code = item.Code;
            Name = item.Name;
            Description = item.Description;
            Latitude = item.Latitude;
            Longitude = item.Longitude;
            Zone = item.Zone;
            Url = item.Url;
            LocationType = item.LocationType;
            ParentStation = item.ParentStation;
            Timezone = item.Timezone;
            WheelchairBoarding = item.WheelchairBoarding;
            LevelId = item.LevelId;
            PlatformCode = item.PlatformCode;
            StopType = item.StopType;
        }

        public int LocationTypeData
        {
            get => (int)(LocationType ?? GTFS.Entities.Enumerations.LocationType.Stop); // Default to Unknown if null
            set => LocationType = (LocationType)value;
        }

        // Enum mapping for StopType (if StopType is an enum)
        public int StopTypeData
        {
            get => (int)StopType;
            set => StopType = (StopType)value;
        }

        // WheelchairBoarding is a string (nullable), we can map it to an integer (e.g., 0 = No, 1 = Yes)
        public int WheelchairBoardingData
        {
            get => string.IsNullOrEmpty(WheelchairBoarding) ? 0 : WheelchairBoarding.ToLower() == "yes" ? 1 : 0;
            set => WheelchairBoarding = value == 1 ? "Yes" : "No";
        }
    }
}