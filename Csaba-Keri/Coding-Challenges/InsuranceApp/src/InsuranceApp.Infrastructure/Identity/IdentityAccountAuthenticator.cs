using InsuranceApp.Application.Authentication;
using InsuranceApp.Application.Authentication.Exceptions;
using InsuranceApp.Application.Authentication.Results;
using InsuranceApp.Application.Common.Security;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Identity;

public class IdentityAccountAuthenticator(
    UserManager<ApplicationUser> userManager,
    InsuranceDbContext context
) : IAccountAuthenticator
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly InsuranceDbContext _context = context;

    public async Task<AuthenticatedAccount> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new InvalidCredentialsException();

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new AccountAccessDeniedException();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles.Count < 1)
        {
            throw new AccountAccessDeniedException();
        }

        if (userRoles.Count > 1)
        {
            throw new InvalidOperationException("Multiple roles are assigned, but only one role per account is supported.");
        }

        var role = userRoles[0];

        switch (role)
        {
            case ApplicationRoles.Admin:
                if (user.BrokerId is not null)
                {
                    throw new InvalidOperationException("An admin account must not be linked to a broker.");
                }

                return new(user.Id, role, null);
            
            case ApplicationRoles.Broker:
                if (user.BrokerId is null)
                {
                    throw new AccountAccessDeniedException();
                }

                var isActiveBroker = await _context.Brokers
                    .AsNoTracking()
                    .AnyAsync(
                        broker => broker.Id == user.BrokerId && broker.Status == BrokerStatus.Active,
                        cancellationToken
                    );

                if (!isActiveBroker)
                {
                    throw new AccountAccessDeniedException();
                }

                return new(user.Id, role, user.BrokerId);
            
            default:
                throw new AccountAccessDeniedException();
        }
    }
}
