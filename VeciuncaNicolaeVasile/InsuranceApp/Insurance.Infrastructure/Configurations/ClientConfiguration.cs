using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insurance.Infrastructure.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(client => client.Id);

            builder.Property(client => client.Name)
                .IsRequired();

            builder.Property(client => client.IdentificationNumber)
                .IsRequired();

            builder.HasIndex(client => client.IdentificationNumber)
                .IsUnique();

            builder.HasMany(client => client.Buildings)
                .WithOne(building => building.Client)
                .HasForeignKey(building => building.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}