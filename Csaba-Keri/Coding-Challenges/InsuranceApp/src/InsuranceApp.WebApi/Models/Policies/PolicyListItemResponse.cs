namespace InsuranceApp.WebApi.Models.Policies;

public record PolicyListItemResponse(
    Guid Id,
    string PolicyNumber,
    PolicyStatusDto Status,
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePremium,
    decimal FinalPremium,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
