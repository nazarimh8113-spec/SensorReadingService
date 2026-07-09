using SensorReadingService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application.Interfaces
{
    public interface IReadingImporter
    {
        Task<ImportReport> ImportAsync(string filePath);
    }
}
