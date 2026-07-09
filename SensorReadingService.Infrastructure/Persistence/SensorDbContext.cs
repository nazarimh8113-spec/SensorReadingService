using Microsoft.EntityFrameworkCore;
using SensorReadingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Infrastructure.Persistence
{
    public class SensorDbContext : DbContext
    {
        public SensorDbContext(DbContextOptions<SensorDbContext> options)
       : base(options)
        {
        }

        public DbSet<Reading> Readings => Set<Reading>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SensorDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
