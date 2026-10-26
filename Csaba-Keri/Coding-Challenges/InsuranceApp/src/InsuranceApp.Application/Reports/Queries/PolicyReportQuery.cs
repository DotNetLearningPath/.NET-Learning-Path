using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Reports.Queries;

public record PolicyReportQuery(
    DateOnly From,
    DateOnly To,
    PolicyStatus? Status = null,
    Guid? CurrencyId = null,
    BuildingType? BuildingType = null
);
