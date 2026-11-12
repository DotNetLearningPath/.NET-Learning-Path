using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Broker
{
    public class BrokerDto
    {
        public Guid Id { get; set; }
        public string BrokerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public BrokerStatus Status { get; set; }
        public decimal? CommissionPercentage { get; set; }
    }
}
