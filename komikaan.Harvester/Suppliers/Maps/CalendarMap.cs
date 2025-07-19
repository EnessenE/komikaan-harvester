using CsvHelper.Configuration;
using komikaan.GTFS.Models.Static.Models;
namespace komikaan.Harvester.Suppliers;


public partial class GenericGTFSSupplier
{
    public class CalendarMap : ClassMap<PSQLCalendar>
    {
        public CalendarMap()
        {
            Map(m => m.ServiceId).Name("service_id");
            Map(m => m.StartDate).Name("start_date").TypeConverterOption.Format("yyyyMMdd");
            Map(m => m.EndDate).Name("end_date").TypeConverterOption.Format("yyyyMMdd");
            Map(m => m.Monday).Name("monday");
            Map(m => m.Tuesday).Name("tuesday");
            Map(m => m.Wednesday).Name("wednesday");
            Map(m => m.Thursday).Name("thursday");
            Map(m => m.Friday).Name("friday");
            Map(m => m.Saturday).Name("saturday");
            Map(m => m.Sunday).Name("sunday");
        }
    }
}