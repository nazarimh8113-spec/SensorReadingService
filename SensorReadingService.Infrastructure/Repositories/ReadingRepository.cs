using Microsoft.EntityFrameworkCore;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Domain.Entities;
using SensorReadingService.Domain.ValueObjects;
using SensorReadingService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Infrastructure.Repositories
{
    public class ReadingRepository : IReadingRepository
    {
        private readonly SensorDbContext _context;

        public ReadingRepository(SensorDbContext context)
        {
            _context = context;
        }

        public async Task<HashSet<ReadingIdentity>> GetExistingIdentitiesAsync(
     CancellationToken cancellationToken = default)
        {
            var identities = await _context.Readings
                .AsNoTracking()
                .Select(x => new ReadingIdentity(
                    x.DeviceId,
                    x.Metric,
                    x.Timestamp,
                    x.Sequence))
                .ToListAsync(cancellationToken);

            return identities.ToHashSet();
        }

        public async Task AddRangeAsync(
      IEnumerable<Reading> readings,
      CancellationToken cancellationToken = default)
        {
            await _context.Readings.AddRangeAsync(readings, cancellationToken);
        }

        public async Task<List<Reading>> GetReadingsAsync(
    string deviceId,
    string metric,
    DateTime from,
    DateTime to,
    CancellationToken cancellationToken = default)
        {
            return await _context.Readings
                .AsNoTracking()
                .Where(x =>
                    x.DeviceId == deviceId &&
                    x.Metric == metric &&
                    x.Timestamp >= from &&
                    x.Timestamp <= to)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(
      CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
