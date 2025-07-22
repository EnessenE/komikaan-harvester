using CsvHelper.Configuration;
using komikaan.Harvester.Adapters;

namespace komikaan.Harvester.Suppliers;

public class AgencyMap : ClassMap<PSQLAgency>
{
    public AgencyMap()
    {
        // Maps for base fields using PostgreSQL names
        Map(m => m.Id).Name("agency_id");
        Map(m => m.Name).Name("agency_name");
        Map(m => m.Url).Name("agency_url");
        Map(m => m.Timezone).Name("agency_timezone");
        Map(m => m.LanguageCode).Name("agency_lang");
        Map(m => m.Phone).Name("agency_phone");
        Map(m => m.FareUrl).Name("agency_fare_url");
        Map(m => m.Email).Name("agency_email");
    }
}