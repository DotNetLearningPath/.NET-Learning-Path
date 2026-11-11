using System.ComponentModel.DataAnnotations;
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

        [Range(typeof(decimal), "-50", "300")]
        public required decimal AdjustmentPercentage { get; set; }

        public required bool IsActive { get; set; }
    }
}
