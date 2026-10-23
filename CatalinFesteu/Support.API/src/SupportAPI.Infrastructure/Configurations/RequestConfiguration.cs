using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportAPI.Infrastructure.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .HasMaxLength(200)
                .IsRequired();
           
            builder.Property(r => r.RequestContent)
                .HasMaxLength(3000)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId);
        }
    }
}
