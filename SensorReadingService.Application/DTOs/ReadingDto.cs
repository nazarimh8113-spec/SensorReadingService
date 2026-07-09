using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.DTOs
{
    public class ReadingDto
    {
        public string DeviceId { get; set; } = default!;

        public string Metric { get; set; } = default!;

        public DateTime Ts { get; set; }

        public double Value { get; set; }

        public int Seq { get; set; }
    }
}
