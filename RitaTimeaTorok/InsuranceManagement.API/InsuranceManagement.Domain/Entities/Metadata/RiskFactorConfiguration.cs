using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities.Metadata
{
    public class RiskFactorConfiguration : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public RiskFactorLevel Level { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CountyId { get; set; }
        public Guid? CityId { get; set; }
        public BuildingType? BuildingType { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
