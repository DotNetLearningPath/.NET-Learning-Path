using Insurance.Domain.Enums;

namespace Insurance.Application.DTO.Policies;

public sealed class PolicyDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid BuildingId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid CurrencyId { get; set; }
    public PolicyStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
}
