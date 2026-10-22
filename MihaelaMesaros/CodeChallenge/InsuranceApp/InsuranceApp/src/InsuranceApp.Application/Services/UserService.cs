using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.User;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Services;

public sealed class UserService(IUserRepository userRepository, IBrokerRepository brokerRepository, IPasswordService passwordService) : IUserService
{
    public async Task<Result> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            return Result.Failure(UserErrors.UsernameRequired);
        }

        var username = createUserDto.Username.Trim();

        if (username.Length < UserConstraints.UsernameMinLength || username.Length > UserConstraints.UsernameMaxLength)
        {
            return Result.Failure(UserErrors.InvalidUsernameLength);
        }

        if (string.IsNullOrWhiteSpace(createUserDto.Password))
        {
            return Result.Failure(UserErrors.PasswordRequired);
        }

        if (createUserDto.Password.Length < UserConstraints.PasswordMinLength || createUserDto.Password.Length > UserConstraints.PasswordMaxLength)
        {
            return Result.Failure(UserErrors.InvalidPasswordLength);
        }

        var existingUser = await userRepository.GetUserByUsernameAsync(username, cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure(UserErrors.DuplicateUsername);
        }


        if (!Enum.IsDefined(createUserDto.Role))
        {
            return Result.Failure(UserErrors.InvalidRole);
        }

        if (createUserDto.Role == Role.Admin && createUserDto.BrokerId.HasValue)
        {
            return Result.Failure(UserErrors.BrokerIdNotAllowed);
        }

        if (createUserDto.Role == Role.Broker)
        {
            if (!createUserDto.BrokerId.HasValue || createUserDto.BrokerId == Guid.Empty)
            {
                return Result.Failure(UserErrors.BrokerIdRequired);
            }

            var broker = await brokerRepository.GetBrokerByIdAsync(createUserDto.BrokerId.Value, cancellationToken);

            if (broker is null)
            {
                return Result.Failure(UserErrors.BrokerNotFound);
            }

            if (!broker.IsActive)
            {
                return Result.Failure(UserErrors.BrokerInactive);
            }
        }


        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = createUserDto.Username.Trim(),
            PasswordHash = string.Empty,
            RoleId = createUserDto.Role,
            BrokerId = createUserDto.BrokerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordService.HashPassword(user, createUserDto.Password);

        await userRepository.AddUserAsync(user, cancellationToken);

        return Result.Success();
    }
}