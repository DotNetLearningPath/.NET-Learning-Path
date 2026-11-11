using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class CreateCurrencyDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public required string Code { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        public required decimal ExchangeRateToBase { get; set; }
    }
}
