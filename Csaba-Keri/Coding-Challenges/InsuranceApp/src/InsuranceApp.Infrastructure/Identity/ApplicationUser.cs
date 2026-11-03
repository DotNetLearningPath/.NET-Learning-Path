using Microsoft.AspNetCore.Identity;

namespace InsuranceApp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; private set; }
    public Guid? BrokerId { get; private set; }

    private ApplicationUser()
    {
        // Used by EF Core.
    }

    public ApplicationUser(string email, Guid? brokerId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (brokerId == Guid.Empty)
        {
            throw new ArgumentException("Broker identifier must not be empty.", nameof(brokerId));
        }

        var trimmedEmail = email.Trim();

        Id = Guid.NewGuid();
        UserName = trimmedEmail;
        Email = trimmedEmail;
        IsActive = true;
        BrokerId = brokerId;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
