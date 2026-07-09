using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.DTOs
{
    public class AggregateResultDto
    {
        public DateTime BucketStart { get; set; }

        public DateTime BucketEnd { get; set; }

        public int Count { get; set; }

        public double? Average { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }
    }
}
