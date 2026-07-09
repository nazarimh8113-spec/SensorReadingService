using SensorReadingService.Domain.Entities;
using SensorReadingService.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.Interfaces
{
    public interface IReadingRepository
    {
        Task<HashSet<ReadingIdentity>> GetExistingIdentitiesAsync();

        Task AddRangeAsync(IEnumerable<Reading> readings);
        Task<List<Reading>> GetReadingsAsync(
    string deviceId,
    string metric,
    DateTime from,
    DateTime to);

        Task SaveChangesAsync();
    }
}
