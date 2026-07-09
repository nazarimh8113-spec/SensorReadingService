using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.DTOs
{
    public class ImportReport
    {
        public int TotalLines { get; set; }

        public int StoredReadings { get; set; }

        public int DuplicateReadings { get; set; }

        public int InvalidReadings { get; set; }
    }
}
