using Insurance.Domain.Enums;

namespace Insurance.Api.DTOs
{
    public class BuildingResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Country { get; set; }
        public int ConstructionYear { get; set; }
        public BuildingType BuildingType { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}