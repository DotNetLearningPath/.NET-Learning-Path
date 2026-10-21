using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Results;

public record PolicyResult(
    Guid Id,
    string PolicyNumber,
    PolicyStatus Status,
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
    IReadOnlyList<AppliedAdjustment> AppliedAdjustments,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CancelledAt,
    DateOnly? CancellationEffectiveDate,
    string? CancellationReason,
    DateTimeOffset? ExpiredAt
);
