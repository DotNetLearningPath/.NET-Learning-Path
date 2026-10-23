using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.User;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class UserService(
    IUserRepository userRepository, 
    IBrokerRepository brokerRepository, 
    IPasswordService passwordService, 
    ILogger<UserService> logger
    ) : IUserService
{
    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Result<UserDto>.Failure(UserErrors.InvalidUserId);
        }

        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.Failure(UserErrors.UserNotFoundById);
        }

        return Result<UserDto>.Success(MapUserToDto(user));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        var validationUserInput = ValidateUserInput(createUserDto);

        if (validationUserInput is not null)
        {
            return Result<UserDto>.Failure(validationUserInput);
        }

        var username = createUserDto.Username.Trim();


        var existingUser = await userRepository.GetUserByUsernameAsync(username, cancellationToken);

        if (existingUser is not null)
        {
            return Result<UserDto>.Failure(UserErrors.DuplicateUsername);
        }

        var validationBroker = await ValidateBrokerAsync(createUserDto, cancellationToken);

        if (validationBroker is not null)
        {
            return Result<UserDto>.Failure(validationBroker);
        }


        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = string.Empty,
            RoleId = createUserDto.Role,
            BrokerId = createUserDto.BrokerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordService.HashPassword(user, createUserDto.Password);

        try
        {
            await userRepository.AddUserAsync(user, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<UserDto>.Failure(UserErrors.DuplicateUsername);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {UserId} created.", user.UserId);
        }

        return Result<UserDto>.Success(MapUserToDto(user));
    }

    private static UserDto MapUserToDto(User user)
    {
        return new UserDto(
            user.UserId,
            user.Username,
            user.RoleId,
            user.BrokerId,
            user.IsActive,
            user.CreatedAt,
            user.ModifiedAt
        );
    }

    private static Error? ValidateUserInput(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            return UserErrors.UsernameRequired;
        }

        var username = dto.Username.Trim();

        if (username.Length < UserConstraints.UsernameMinLength || username.Length > UserConstraints.UsernameMaxLength)
        {
            return UserErrors.InvalidUsernameLength;
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return UserErrors.PasswordRequired;
        }

        if (dto.Password.Length < UserConstraints.PasswordMinLength || dto.Password.Length > UserConstraints.PasswordMaxLength)
        {
            return UserErrors.InvalidPasswordLength;
        }

        if (!Enum.IsDefined(dto.Role))
        {
            return UserErrors.InvalidRole;
        }

        return null;
    }

    private async Task<Error?> ValidateBrokerAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (dto.Role == Role.Admin && dto.BrokerId.HasValue)
        {
            return UserErrors.BrokerIdNotAllowed;
        }

        if (dto.Role != Role.Broker)
        {
            return null;
        }

        if (!dto.BrokerId.HasValue || dto.BrokerId == Guid.Empty)
        {
            return UserErrors.BrokerIdRequired;
        }

        var broker = await brokerRepository.GetBrokerByIdAsync(dto.BrokerId.Value, cancellationToken);

        if (broker is null)
        {
            return UserErrors.BrokerNotFound;
        }

        if (!broker.IsActive)
        {
            return UserErrors.BrokerInactive;
        }

        return null;
    }
}