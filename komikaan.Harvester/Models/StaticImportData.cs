namespace komikaan.Harvester.Models
{
    internal static class StaticImportData
    {
        public static string CurrentDataOrigin { get; set; } = "Unknown";
        public static Guid CurrentImportId { get; set; } = Guid.NewGuid();
    }
}
