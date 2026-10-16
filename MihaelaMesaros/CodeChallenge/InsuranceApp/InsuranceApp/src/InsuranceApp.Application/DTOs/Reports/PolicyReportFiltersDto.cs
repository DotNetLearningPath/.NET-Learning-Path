using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Reports;

public sealed record PolicyReportFiltersDto(
    DateTime? From,
    DateTime? To,
    string? Status,
    string? Currency,
    string? BuildingType
);
