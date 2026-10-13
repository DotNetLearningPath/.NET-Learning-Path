using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class PolicyAdjustmentEntity(
    Guid policyId,
    AdjustmentSource source,
    Guid configurationId,
    decimal percentage
)
{
    public Guid PolicyId { get; private set; } = policyId;
    public AdjustmentSource Source { get; private set; } = source;
    public Guid ConfigurationId { get; private set; } = configurationId;
    public decimal Percentage { get; private set; } = percentage;
}
