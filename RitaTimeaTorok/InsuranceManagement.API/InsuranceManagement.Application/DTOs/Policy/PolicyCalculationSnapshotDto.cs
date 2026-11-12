using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Policy
{
    public class PolicyCalculationSnapshotDto
    {
        public Guid Id { get; set; }
        public PolicyCalculationSnapshotType SnapshotType { get; set; }
        public Guid OriginalConfigurationId { get; set; }
        public string AppliedConfigurationName { get; set; } = string.Empty;
        public string? AppliedFeeConfigurationType { get; set; }
        public RiskFactorLevel? AppliedRiskFactorLevel { get; set; }
        public string? AppliedRiskFactorTargetName { get; set; }
        public decimal AppliedPercentage { get; set; }
        public DateTimeOffset CapturedAt { get; set; }
    }
}
