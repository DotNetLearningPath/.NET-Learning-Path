using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.User;

public sealed record CreateUserDto(
    string Username,
    string Password,
    Role Role,
    Guid? BrokerId
);
