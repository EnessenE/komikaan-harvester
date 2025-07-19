using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Adapters
{
    /// <summary>
    /// Adapter class for PostgreSQL upsert that maps C# names to DB field names.
    /// </summary>
    public class PSQLAgency : Agency
    {
        // New fields in PostgreSQL only
        public string DataOrigin { get; set; } = StaticImportData.CurrentDataOrigin;
        public Guid InternalId { get; set; } = Guid.NewGuid();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid ImportId { get; set; } = StaticImportData.CurrentImportId;

        // Aliases that match PostgreSQL column names
        public string Id
        {
            get => AgencyId;
            set => AgencyId = value;
        }

        public string Name
        {
            get => AgencyName;
            set => AgencyName = value;
        }

        public string Url
        {
            get => AgencyUrl;
            set => AgencyUrl = value;
        }

        public string Timezone
        {
            get => AgencyTimezone;
            set => AgencyTimezone = value;
        }

        public string LanguageCode
        {
            get => AgencyLang;
            set => AgencyLang = value;
        }

        public string Phone
        {
            get => AgencyPhone;
            set => AgencyPhone = value;
        }

        public string FareUrl
        {
            get => AgencyFareUrl;
            set => AgencyFareUrl = value;
        }

        public string Email
        {
            get => AgencyEmail;
            set => AgencyEmail = value;
        }
    }
}
