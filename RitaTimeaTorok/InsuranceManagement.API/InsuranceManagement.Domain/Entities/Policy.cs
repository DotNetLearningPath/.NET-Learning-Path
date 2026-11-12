using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities
{
    public class Policy : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string PolicyNumber { get; set; }
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid BrokerId { get; set; }
        public Guid CurrencyId { get; set; }
        public PolicyStatus Status { get; set; } = PolicyStatus.Draft;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal BasePremiumAmount { get; set; }
        public decimal FinalPremiumAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? ActivatedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateOnly? CancellationEffectiveDate { get; set; }
        public ICollection<PolicyCalculationSnapshot> CalculationSnapshotEntries { get; set; } =
            new List<PolicyCalculationSnapshot>();
    }
}
