using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class RiskFactorConfigurationDto
    {
        public Guid Id { get; set; }
        public RiskFactorLevel Level { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CountyId { get; set; }
        public Guid? CityId { get; set; }
        public BuildingType? BuildingType { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
