using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.DTOs.Policy
{
    public class PolicyListItemDto
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid BrokerId { get; set; }
        public Guid CurrencyId { get; set; }
        public PolicyStatus Status { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal BasePremiumAmount { get; set; }
        public decimal FinalPremiumAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
