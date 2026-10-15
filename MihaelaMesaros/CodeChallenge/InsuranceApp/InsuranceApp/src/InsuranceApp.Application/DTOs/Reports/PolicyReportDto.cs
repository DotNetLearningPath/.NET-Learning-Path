namespace InsuranceApp.Application.DTOs.Reports;

public sealed record PolicyReportDto(
    Guid GroupId,
    string GroupType,
    string GroupName,

    Guid CurrencyId,
    string CurrencyCode,
    
    int PolicyCount,
    decimal TotalFinalPremium,
    decimal TotalFinalPremiumInBaseCurrency
);

