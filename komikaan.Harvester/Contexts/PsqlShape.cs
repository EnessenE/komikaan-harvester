using GTFS.Entities;

namespace komikaan.Harvester.Contexts
{
    internal class PsqlShape: Shape
    {
        public PsqlShape() { }

        public PsqlShape(Shape item)
        {
            Id = item.Id;
            Latitude = item.Latitude;
            Longitude = item.Longitude;
            DistanceTravelled = item.DistanceTravelled;
            Sequence = item.Sequence;
        }

        public double SequenceData
        {
            get => (double)base.Sequence;
            set => base.Sequence = (uint)value;
        }
    }
}
