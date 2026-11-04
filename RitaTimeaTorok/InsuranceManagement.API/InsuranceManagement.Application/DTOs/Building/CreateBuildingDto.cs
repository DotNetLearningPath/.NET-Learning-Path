using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Building
{
    public class CreateBuildingDto
    {
        public required Guid ClientId { get; set; }
        [Required]
        [MaxLength(250)]
        public required string Address { get; set; }
        public required Guid CityId { get; set; }
        public int ConstructionYear { get; set; }
        [EnumDataType(typeof(BuildingType))]
        public required BuildingType Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}
