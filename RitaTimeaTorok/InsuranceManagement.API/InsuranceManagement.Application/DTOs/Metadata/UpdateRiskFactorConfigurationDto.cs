using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class UpdateRiskFactorConfigurationDto
    {
        public RiskFactorLevel Level { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CountyId { get; set; }
        public Guid? CityId { get; set; }
        public BuildingType? BuildingType { get; set; }

        [Range(RiskFactorLimits.MinimumAdjustmentPercentage, RiskFactorLimits.MaximumAdjustmentPercentage)]
        public required decimal AdjustmentPercentage { get; set; }

        public required bool IsActive { get; set; }
    }
}
