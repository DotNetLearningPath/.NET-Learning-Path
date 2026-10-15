using InsuranceApp.Domain.Enums;
using System.Text.Json.Serialization;

namespace InsuranceApp.Application.DTOs.Reports;

public sealed record PolicyReportDto
{
    public Guid GroupId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PolicyReportGroupType GroupType { get; init; }

    public string GroupName { get; init; } = string.Empty;

    public Guid CurrencyId { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public int PolicyCount { get; init; }

    public decimal TotalFinalPremium { get; set; }

    public decimal TotalFinalPremiumInBaseCurrency { get; set; }

    public string BaseCurrencyCode { get; set; } = string.Empty;
}

