using CsvHelper.Configuration;
using komikaan.Harvester.Adapters;

namespace komikaan.Harvester.Suppliers;

public class FrequencyMap : ClassMap<PSQLFrequency>
{
    public FrequencyMap()
    {
        Map(m => m.TripId).Name("trip_id");
        Map(m => m.StartTimeData).Name("start_time").TypeConverter<GtfsTimeConverter>();
        Map(m => m.EndTimeData).Name("end_time").TypeConverter<GtfsTimeConverter>();
        Map(m => m.HeadwaySecs).Name("headway_secs");
        Map(m => m.ExactTimesData).Name("exact_times");
    }
}
