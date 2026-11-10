namespace InsuranceManagement.Domain.Entities.Metadata
{
    public class FeeConfiguration : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Type { get; set; }
        public decimal Percentage { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
