using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Policy
{
    public class CreatePolicyDto
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid BuildingId { get; set; }

        [Required]
        public Guid BrokerId { get; set; }

        [Required]
        public Guid CurrencyId { get; set; }

        [Required]
        public decimal BasePremiumAmount { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
