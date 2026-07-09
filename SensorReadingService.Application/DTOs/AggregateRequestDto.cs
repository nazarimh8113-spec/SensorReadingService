using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.DTOs
{
    public class AggregateRequestDto
    {
        public string DeviceId { get; set; } = string.Empty;

        public string Metric { get; set; } = string.Empty;

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public int BucketSizeInMinutes { get; set; }
    }
}
