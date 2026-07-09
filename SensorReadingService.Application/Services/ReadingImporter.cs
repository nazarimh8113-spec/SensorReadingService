using Microsoft.Extensions.Logging;
using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Domain.Entities;
using SensorReadingService.Domain.ValueObjects;
using System.Text.Json;
using System.Threading;


namespace SensorReadingService.Application.Services;

public class ReadingImporter : IReadingImporter
{
    private readonly IFileReader _fileReader;
    private readonly IReadingRepository _repository;
    private readonly ILogger<ReadingImporter> _logger;

    public ReadingImporter(
        IFileReader fileReader,
        IReadingRepository repository, ILogger<ReadingImporter> logger)
    {
        _fileReader = fileReader;
        _repository = repository;
        _logger = logger;
    }

    public async Task<ImportReport> ImportAsync(
     string filePath,
     CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Import started. FilePath: {FilePath}", filePath);

        var report = new ImportReport();

        var existingIdentities =
            await _repository.GetExistingIdentitiesAsync(cancellationToken);

        var readingsToInsert = new List<Reading>();

        await foreach (var line in _fileReader.ReadLinesAsync(filePath)
            .WithCancellation(cancellationToken))
        {
            report.TotalLines++;

            ReadingDto? dto;

            try
            {
                dto = JsonSerializer.Deserialize<ReadingDto>(
                    line,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid JSON at line {LineNumber}",
                    report.TotalLines);

                report.InvalidReadings++;
                continue;
            }

            if (dto is null)
            {
                _logger.LogWarning(
                    "Deserialization returned null at line {LineNumber}",
                    report.TotalLines);

                report.InvalidReadings++;
                continue;
            }

            if (!IsValid(dto))
            {
                _logger.LogWarning(
                    "Invalid reading at line {LineNumber}. DeviceId:{DeviceId}, Metric:{Metric}, Seq:{Seq}",
                    report.TotalLines,
                    dto.DeviceId,
                    dto.Metric,
                    dto.Seq);

                report.InvalidReadings++;
                continue;
            }

            var identity = new ReadingIdentity(
                dto.DeviceId,
                dto.Metric,
                dto.Ts,
                dto.Seq);

            if (existingIdentities.Contains(identity))
            {
                _logger.LogInformation(
                    "Duplicate reading skipped. Device:{DeviceId}, Metric:{Metric}, Seq:{Seq}",
                    dto.DeviceId,
                    dto.Metric,
                    dto.Seq);

                report.DuplicateReadings++;
                continue;
            }

            existingIdentities.Add(identity);

            readingsToInsert.Add(new Reading(
                dto.DeviceId,
                dto.Metric,
                dto.Ts,
                dto.Value,
                dto.Seq));

            report.StoredReadings++;
        }

        if (readingsToInsert.Count > 0)
        {
            await _repository.AddRangeAsync(readingsToInsert, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Import completed. Total:{Total}, Stored:{Stored}, Invalid:{Invalid}, Duplicate:{Duplicate}",
            report.TotalLines,
            report.StoredReadings,
            report.InvalidReadings,
            report.DuplicateReadings);

        return report;
    }

    private static bool IsValid(ReadingDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.DeviceId)
            && !string.IsNullOrWhiteSpace(dto.Metric)
            && dto.Seq >= 0;
    }
}