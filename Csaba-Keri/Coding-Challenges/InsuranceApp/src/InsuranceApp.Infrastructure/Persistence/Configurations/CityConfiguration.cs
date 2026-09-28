using InsuranceApp.Domain.Geography;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<CityEntity>
{
    public void Configure(EntityTypeBuilder<CityEntity> builder)
    {
        builder.ToTable("cities");
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(City.MaxNameLength)
            .IsRequired();
        
        builder.Property(entity => entity.CountyId)
            .HasColumnName("county_id");
        
        builder.HasIndex(entity => new { entity.CountyId, entity.Name })
            .IsUnique();
        
        builder.HasOne<CountyEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CountyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
