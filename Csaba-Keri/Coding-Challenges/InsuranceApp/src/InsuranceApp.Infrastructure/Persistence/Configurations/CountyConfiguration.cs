using InsuranceApp.Domain.Geography;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class CountyConfiguration : IEntityTypeConfiguration<CountyEntity>
{
    public void Configure(EntityTypeBuilder<CountyEntity> builder)
    {
        builder.ToTable("counties");
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(County.MaxNameLength)
            .IsRequired();
        
        builder.Property(entity => entity.CountryId)
            .HasColumnName("country_id");
        
        builder.HasIndex(entity => new { entity.CountryId, entity.Name })
            .IsUnique();
        
        builder.HasOne<CountryEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
