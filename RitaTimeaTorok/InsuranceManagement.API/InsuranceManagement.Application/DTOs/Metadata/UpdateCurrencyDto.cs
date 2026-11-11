using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class UpdateCurrencyDto
    {
        public bool? IsActive { get; set; }
        public decimal? ExchangeRateToBase { get; set; }
    }
}
