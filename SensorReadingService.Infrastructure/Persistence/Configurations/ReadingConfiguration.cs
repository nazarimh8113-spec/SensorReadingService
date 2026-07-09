using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorReadingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Infrastructure.Persistence.Configurations
{
    public class ReadingConfiguration
    {
        public void Configure(EntityTypeBuilder<Reading> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeviceId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Metric)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Timestamp)
                   .IsRequired();

            builder.Property(x => x.Value)
                   .IsRequired();

            builder.Property(x => x.Sequence)
                   .IsRequired();

            builder.HasIndex(x => new
            {
                x.DeviceId,
                x.Metric,
                x.Timestamp,
                x.Sequence
            }).IsUnique();
        }
    }
}
