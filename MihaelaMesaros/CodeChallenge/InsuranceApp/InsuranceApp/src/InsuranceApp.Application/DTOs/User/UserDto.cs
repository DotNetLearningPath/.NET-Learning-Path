using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.User;

public sealed record UserDto(
    Guid UserId,
    string Username,
    Role Role,
    Guid? BrokerId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);
