using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Building
{
    public class BuildingDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Address { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public int ConstructionYear { get; set; }
        public BuildingType Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}
