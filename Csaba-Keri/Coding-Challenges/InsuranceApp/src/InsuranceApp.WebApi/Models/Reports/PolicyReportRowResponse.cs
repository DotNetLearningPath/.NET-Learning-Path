namespace InsuranceApp.WebApi.Models.Reports;

public record PolicyReportRowResponse(
    Guid GroupId,
    string GroupName,
    Guid CurrencyId,
    string CurrencyCode,
    long PolicyCount,
    decimal TotalFinalPremium,
    decimal TotalFinalPremiumInBaseCurrency
);
