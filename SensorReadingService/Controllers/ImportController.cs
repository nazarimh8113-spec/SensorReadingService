using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Configuration;

namespace SensorReadingService.Controllers
{
    [ApiController]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly IReadingImporter _importer;
        private readonly IWebHostEnvironment _environment;
        private readonly ImportSettings _settings;

        public ImportController(
            IReadingImporter importer,
            IOptions<ImportSettings> options,
            IWebHostEnvironment environment)
        {
            _importer = importer;
            _settings = options.Value;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            var filePath = Path.Combine(
            _environment.ContentRootPath,
            _environment.ContentRootPath,
             _settings.FilePath);

            var report = await _importer.ImportAsync(filePath);

            return Ok(report);

        }
    }
}
