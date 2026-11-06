using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Building
{
    public class UpdateBuildingDto
    {
        [Required]
        [MaxLength(250)]
        public required string Street { get; set; }
        [Range(1, int.MaxValue)]
        public int StreetNumber { get; set; }
        public required Guid CityId { get; set; }
        public int ConstructionYear { get; set; }
        [EnumDataType(typeof(BuildingType))]
        public required BuildingType Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}
