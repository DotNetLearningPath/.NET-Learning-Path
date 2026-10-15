using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Policy;

public sealed record PolicySearchDto(
    Guid? ClientId,
    Guid? BrokerId,
    PolicyStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int PageNumber = 1,
    int PageSize = 50
);
