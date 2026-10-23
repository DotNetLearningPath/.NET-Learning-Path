using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities
{
    public class Building
    {
        public int ConstructionYear { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public string Street { get; set; } = string.Empty;
        public int CityId { get; set; }
        public City City { get; set; } = null!;
        public BuildingType BuildingType { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public bool? IsFloodZone { get; set; }
        public bool? IsEarthquakeRiskZone { get; set; }
    }
}
