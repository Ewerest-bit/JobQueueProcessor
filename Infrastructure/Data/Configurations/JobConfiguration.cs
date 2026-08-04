using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
    internal class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.Property(j => j.Payload)
                .IsRequired();
            builder.Property(j => j.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            builder.Property(j => j.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}
