using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class Broker
{
    public Guid BrokerId { get; set; }

    public string BrokerCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public BrokerStatus Status { get; set; }

    public decimal? CommissionPercentage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
}