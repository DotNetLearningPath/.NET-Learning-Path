using InsuranceApp.Domain.Policies;

namespace InsuranceApp.UnitTests.TestData.Policies;

internal static class PolicyTestData
{
    public const string DefaultPolicyNumber = "POL-TEST-001";
    public const PolicyStatus DefaultStatus = PolicyStatus.Draft;
    public const decimal DefaultCurrencyExchangeRateToBase = 5m;
    public const decimal DefaultBasePremium = 100m;
    public const decimal DefaultFinalPremium = DefaultBasePremium;
    public static readonly Guid DefaultPolicyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultClientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DefaultBuildingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DefaultBrokerId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid DefaultCurrencyId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly DateOnly DefaultStartDate = new(2030, 1, 1);
    public static readonly DateOnly DefaultEndDate = new(2030, 12, 31);
    public static readonly IReadOnlyList<AppliedAdjustment> DefaultAppliedAdjustments = [];
    public static readonly DateTimeOffset DefaultCreatedAt = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset DefaultUpdatedAt = DefaultCreatedAt;

    public static readonly DraftPolicyData DefaultDraftPolicyData = new()
    {
        AppliedAdjustments = DefaultAppliedAdjustments,
        BasePremium = DefaultBasePremium,
        BrokerId = DefaultBrokerId,
        BuildingId = DefaultBuildingId,
        ClientId = DefaultClientId,
        CurrencyExchangeRateToBase = DefaultCurrencyExchangeRateToBase,
        CurrencyId = DefaultCurrencyId,
        EndDate = DefaultEndDate,
        FinalPremium = DefaultFinalPremium,
        PolicyId = DefaultPolicyId,
        StartDate = DefaultStartDate
    };

    public static readonly PolicyData DefaultPolicyData = new()
    {
        AppliedAdjustments = DefaultAppliedAdjustments,
        BasePremium = DefaultBasePremium,
        BrokerId = DefaultBrokerId,
        BuildingId = DefaultBuildingId,
        ClientId = DefaultClientId,
        CreatedAt = DefaultCreatedAt,
        CurrencyExchangeRateToBase = DefaultCurrencyExchangeRateToBase,
        CurrencyId = DefaultCurrencyId,
        EndDate = DefaultEndDate,
        FinalPremium = DefaultFinalPremium,
        PolicyId = DefaultPolicyId,
        PolicyNumber = DefaultPolicyNumber,
        StartDate = DefaultStartDate,
        Status = DefaultStatus,
        UpdatedAt = DefaultUpdatedAt
    };

    public static Policy Create(PolicyData? data = null)
    {
        return new(data: data ?? DefaultPolicyData);
    }
}
