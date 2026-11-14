using komikaan.GTFS.Models.Static.Models;

namespace komikaan.Harvester.Managers
{
    internal class GardernerNotification
    {
        public string Supplier { get; set; } = null!;
        public Stop Stop { get;  set; } = null!;
    }
}