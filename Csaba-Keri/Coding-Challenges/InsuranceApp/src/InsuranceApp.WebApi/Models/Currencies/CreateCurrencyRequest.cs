using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Currencies;

public record CreateCurrencyRequest(
    [Required]
    string Code,

    [Required]
    string Name,
    
    [Required]
    decimal? ExchangeRateToBase,
    
    [Required]
    bool? IsActive
);
