using CsvHelper.Configuration;
using komikaan.GTFS.Models.Static.Models;
namespace komikaan.Harvester.Suppliers;


public partial class GenericGTFSSupplier
{
    public class CalendarDateMap : ClassMap<PSQLCalendarDate>
    {
        public CalendarDateMap()
        {
            Map(m => m.ServiceId).Name("service_id");
            Map(m => m.Date).Name("date").TypeConverterOption.Format("yyyyMMdd");
            Map(m => m.ExceptionType).Name("exception_type");
        }
    }
}