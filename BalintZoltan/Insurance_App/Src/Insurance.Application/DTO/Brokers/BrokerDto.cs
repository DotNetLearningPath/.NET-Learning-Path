using Insurance.Domain.Enums;

namespace Insurance.Application.DTO.Brokers;

public sealed class BrokerDto
{
    public Guid Id { get; set; }
    public string BrokerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public BrokerStatus Status { get; set; }
    public decimal? CommissionPercentage { get; set; }
}
