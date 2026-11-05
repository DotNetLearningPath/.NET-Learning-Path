using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insurance.Infrastructure.Configurations
{
    public class CountyConfiguration : IEntityTypeConfiguration<County>
    {
        public void Configure(EntityTypeBuilder<County> builder)
        {
            builder.HasKey(county => county.Id);

            builder.Property(county => county.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(county => county.Country)
                .WithMany(country => country.Counties)
                .HasForeignKey(county => county.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
