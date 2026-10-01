using Insurance.Application.Abstractions;

namespace Insurance.Application.DTO.Brokers;

public sealed class CreateBrokerRequest : IBrokerRequest
{
    public string BrokerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal? CommissionPercentage { get; set; }
}
