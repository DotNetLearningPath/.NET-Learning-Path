using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        private const string UniqueIdentityIndex = "UX_Users_IdentityIssuer_IdentitySubject";

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Department)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(u => u.IdentityIssuer)
                .IsRequired();

            builder.Property(u => u.IdentitySubject)
                .IsRequired();

            builder.HasIndex(u => new
            {
                u.IdentityIssuer,
                u.IdentitySubject
            })
            .IsUnique()
            .HasDatabaseName(UniqueIdentityIndex);
        }
    }
}