using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insurance.Infrastructure.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(city => city.Id);

            builder.Property(city => city.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(city => city.County)
                .WithMany(county => county.Cities)
                .HasForeignKey(city => city.CountyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
