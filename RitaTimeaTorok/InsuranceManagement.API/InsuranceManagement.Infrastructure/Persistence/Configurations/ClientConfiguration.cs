using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasIndex(c => c.IdentificationNumber)
            .IsUnique();
    }
}