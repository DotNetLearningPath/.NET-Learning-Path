using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities
{
    public class PolicyCalculationSnapshot : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PolicyId { get; set; }
        public PolicyCalculationSnapshotType SnapshotType { get; set; }
        public Guid OriginalConfigurationId { get; set; }
        public required string AppliedConfigurationName { get; set; }
        public string? AppliedFeeConfigurationType { get; set; }
        public RiskFactorLevel? AppliedRiskFactorLevel { get; set; }
        public string? AppliedRiskFactorTargetName { get; set; }
        public decimal AppliedPercentage { get; set; }
        public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
