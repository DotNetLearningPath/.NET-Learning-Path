namespace InsuranceManagement.Domain.Entities.Metadata
{
    public class Currency : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Code { get; set; }
        public required string Name { get; set; }
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
