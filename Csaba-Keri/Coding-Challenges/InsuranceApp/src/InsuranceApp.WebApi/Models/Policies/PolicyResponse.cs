namespace InsuranceApp.WebApi.Models.Policies;

public record PolicyResponse(
    Guid Id,
    string PolicyNumber,
    PolicyStatusDto Status,
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    string CurrencyCode,
    decimal CurrencyExchangeRateToBase,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePremium,
    decimal FinalPremium,
    IReadOnlyList<AppliedAdjustmentResponse> AppliedAdjustments,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CancelledAt,
    DateOnly? CancellationEffectiveDate,
    string? CancellationReason,
    DateTimeOffset? ExpiredAt
);
