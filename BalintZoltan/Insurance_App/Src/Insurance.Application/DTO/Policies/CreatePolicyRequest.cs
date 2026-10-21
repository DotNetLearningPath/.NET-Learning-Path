namespace Insurance.Application.DTO.Policies;

public sealed class CreatePolicyRequest
{
    public Guid ClientId { get; set; }
    public Guid BuildingId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BasePremium { get; set; }
}
