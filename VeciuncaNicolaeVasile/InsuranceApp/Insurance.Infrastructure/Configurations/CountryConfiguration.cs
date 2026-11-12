using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insurance.Infrastructure.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country> 
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(country => country.Id);

            builder.Property(country => country.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
