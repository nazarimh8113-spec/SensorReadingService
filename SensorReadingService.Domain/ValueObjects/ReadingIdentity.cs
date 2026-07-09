using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Domain.ValueObjects
{
    public record ReadingIdentity(
       string DeviceId,
       string Metric,
       DateTime Timestamp,
       int Sequence);
}
