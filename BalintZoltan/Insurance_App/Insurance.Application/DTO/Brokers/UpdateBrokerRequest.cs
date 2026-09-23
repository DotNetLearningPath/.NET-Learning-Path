namespace Application.DTO.Brokers;

public sealed class UpdateBrokerRequest
{
    public string BrokerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal? CommissionPercentage { get; set; }
}
