using InsuranceApp.Domain.Geography;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<CountryEntity>
{
    public void Configure(EntityTypeBuilder<CountryEntity> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(Country.MaxNameLength)
            .IsRequired();
        
        builder.HasIndex(entity => entity.Name)
            .IsUnique();
    }
}
