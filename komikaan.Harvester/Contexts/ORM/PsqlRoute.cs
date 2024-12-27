using GTFS.Entities.Enumerations;
using Route = GTFS.Entities.Route;

namespace komikaan.Harvester.Contexts.ORM
{
    internal class PsqlRoute : Route
    {
        public PsqlRoute() { }

        public PsqlRoute(Route item)
        {
            InternalId = item.InternalId;
            DataOrigin = item.DataOrigin;
            Id = item.Id;
            AgencyId = item.AgencyId;
            ShortName = item.ShortName;
            LongName = item.LongName;
            Description = item.Description;
            Type = item.Type;
            Url = item.Url;
            Color = item.Color;
            TextColor = item.TextColor;
        }

        public int RouteType { get => (int)Type; set => Type = (RouteTypeExtended)value; }
    }
}