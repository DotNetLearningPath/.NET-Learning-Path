namespace InsuranceApp.Application.Common;

public static class PolicyReportErrors
{
    public static readonly Error InvalidDateRange = new("PolicyReport.InvalidDateRange", "From date cannot be greater than To date.", ErrorType.Validation);

    public static readonly Error InvalidStatus = new("PolicyReport.InvalidStatus", "Invalid policy status.", ErrorType.Validation);

    public static readonly Error InvalidCurrencyId = new("PolicyReport.InvalidCurrencyId", "CurrencyId cannot be empty.", ErrorType.Validation);

    public static readonly Error InvalidBuildingTypeId = new("PolicyReport.InvalidBuildingTypeId", "BuildingTypeId cannot be empty.", ErrorType.Validation);

    public static readonly Error BaseCurrencyCodeNotConfigured = new("PolicyReport.BaseCurrencyCodeNotConfigured", "BaseCurrencyCode is not configured.", ErrorType.Validation);
    
}
