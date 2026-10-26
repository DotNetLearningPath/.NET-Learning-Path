namespace InsuranceApp.Application.Reports.Results;

public record PolicyReportRowResult(
    Guid GroupId,
    string GroupName,
    Guid CurrencyId,
    string CurrencyCode,
    long PolicyCount,
    decimal TotalFinalPremium,
    decimal TotalFinalPremiumInBaseCurrency
);
