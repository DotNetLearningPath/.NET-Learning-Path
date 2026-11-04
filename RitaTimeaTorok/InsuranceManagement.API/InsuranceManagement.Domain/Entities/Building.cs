using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities
{
    public class Building : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public required string Address { get; set; }
        public Guid CityId { get; set; }
        public int ConstructionYear { get; set; }
        public BuildingType Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}
