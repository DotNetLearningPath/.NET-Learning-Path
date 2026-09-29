using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class Policy
{
    public Guid PolicyId { get; set; }

    public string PolicyNumber { get; set; } = null!;

    public Guid ClientId { get; set; }

    public Guid BuildingId { get; set; }

    public Guid BrokerId { get; set; }

    public Guid CurrencyId { get; set; }

    public PolicyStatus Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal BasePremium { get; set; }

    public decimal FinalPremium { get; set; }

    public DateTime? CancellationDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }


    public Client Client { get; set; } = null!;

    public Building Building { get; set; } = null!;

    public Broker Broker { get; set; } = null!;

    public Currency Currency { get; set; } = null!;

}