using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasOne(c => c.County)
                .WithMany()
                .HasForeignKey(c => c.CountyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
