using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion<string>();
        builder.Property(x => x.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.PolicyNumber).IsUnique();
        builder.Property(x => x.ClientId).HasConversion<string>().IsRequired();
        builder.Property(x => x.BuildingId).HasConversion<string>().IsRequired();
        builder.Property(x => x.BrokerId).HasConversion<string>().IsRequired();
        builder.Property(x => x.CurrencyId).HasConversion<string>().IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.BasePremium).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.FinalPremium).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);

        builder.HasOne<Client>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Building>().WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Broker>().WithMany().HasForeignKey(x => x.BrokerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.BuildingId);
        builder.HasIndex(x => x.BrokerId);
        builder.HasIndex(x => x.CurrencyId);
    }
}
