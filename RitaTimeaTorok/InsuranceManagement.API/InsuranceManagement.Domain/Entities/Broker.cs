using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Domain.Entities
{
    public class Broker : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string BrokerCode { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public BrokerStatus Status { get; set; } = BrokerStatus.Active;
        public decimal? CommissionPercentage { get; set; }
    }
}
