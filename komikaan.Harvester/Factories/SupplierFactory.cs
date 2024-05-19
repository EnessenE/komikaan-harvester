using komikaan.Harvester.Enums;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Models;
using komikaan.Harvester.Suppliers;

namespace komikaan.Harvester.Factories
{
    public class SupplierFactory
    {
        private readonly List<SupplierConfiguration> _supplierConfigurations;
        private readonly IServiceCollection _services;

        //As we have no way for easy config, we are hard coding it for now
        public SupplierFactory(IServiceCollection services)
        {
            _services = services;
            _supplierConfigurations = new List<SupplierConfiguration>();
            _supplierConfigurations.Add(new SupplierConfiguration()
            {
                DataType = SupplierType.GTFS,
                RetrievalType = RetrievalType.REST,
                Name = "OpenOV",
                Url = "C:\\Users\\maile\\Downloads\\gtfs-nl.zip"
        });
        }

        public void AddSuppliers()
        {
            foreach (var supplierConfiguration in _supplierConfigurations)
            {
                var supplier = new GenericGTFSSupplier(supplierConfiguration);
                _services.AddSingleton<ISupplier>(supplier);
            }
        }

    }
}


