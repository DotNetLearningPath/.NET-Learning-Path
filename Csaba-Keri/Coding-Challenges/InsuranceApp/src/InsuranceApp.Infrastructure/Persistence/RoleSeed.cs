using InsuranceApp.Application.Common.Security;
using InsuranceApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

internal static class RoleSeed
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        var admin = new ApplicationRole
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = ApplicationRoles.Admin,
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "11111111-aaaa-4aaa-8aaa-aaaaaaaaaaaa"
        };

        var broker = new ApplicationRole
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            Name = ApplicationRoles.Broker,
            NormalizedName = "BROKER",
            ConcurrencyStamp = "22222222-bbbb-4bbb-8bbb-bbbbbbbbbbbb"
        };

        modelBuilder.Entity<ApplicationRole>().HasData(admin, broker);
    }
}
