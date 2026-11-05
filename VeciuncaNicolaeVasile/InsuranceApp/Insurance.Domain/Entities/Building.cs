using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities
{
    public class Building
    {
        public Guid Id { get; set; }
        public int ConstructionYear { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public string Street { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public City City { get; set; } = null!;
        public BuildingType BuildingType { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}
