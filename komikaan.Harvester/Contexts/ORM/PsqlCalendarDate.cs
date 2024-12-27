using GTFS.Entities;
using GTFS.Entities.Enumerations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace komikaan.Harvester.Contexts.ORM
{
    public class PsqlCalendarDate : CalendarDate
    {
        public PsqlCalendarDate() { }

        public PsqlCalendarDate(CalendarDate item)
        {
            ServiceId = item.ServiceId;
            Date = item.Date;
            ExceptionType = item.ExceptionType;
        }

        // Enum mapping for ExceptionType (assuming it's an integer representation of the enum)
        public int ExceptionTypeData
        {
            get => (int)ExceptionType;
            set => ExceptionType = (ExceptionType)value;
        }
    }
}
