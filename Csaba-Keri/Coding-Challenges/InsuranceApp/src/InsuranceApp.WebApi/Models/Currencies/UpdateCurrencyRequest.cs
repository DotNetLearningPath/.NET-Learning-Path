using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Currencies;

public record UpdateCurrencyRequest(
    [Required]
    string Name,

    [Required]
    decimal? ExchangeRateToBase,

    [Required]
    bool? IsActive
);
