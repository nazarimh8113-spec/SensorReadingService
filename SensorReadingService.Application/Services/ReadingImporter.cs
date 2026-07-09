using System.Text.Json;
using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Domain.Entities;
using SensorReadingService.Domain.ValueObjects;

namespace SensorReadingService.Application.Services;

public class ReadingImporter : IReadingImporter
{
    private readonly IFileReader _fileReader;
    private readonly IReadingRepository _repository;

    public ReadingImporter(
        IFileReader fileReader,
        IReadingRepository repository)
    {
        _fileReader = fileReader;
        _repository = repository;
    }

    public async Task<ImportReport> ImportAsync(string filePath)
    {
        var report = new ImportReport();

        var existingIdentities =
            await _repository.GetExistingIdentitiesAsync();

        var readingsToInsert = new List<Reading>();

        await foreach (var line in _fileReader.ReadLinesAsync(filePath))
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
            catch
            {
                report.InvalidReadings++;
                continue;
            }

            if (dto is null)
            {
                report.InvalidReadings++;
                continue;
            }

            if (!IsValid(dto))
            {
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

        await _repository.AddRangeAsync(readingsToInsert);

        await _repository.SaveChangesAsync();

        return report;
    }

    private static bool IsValid(ReadingDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.DeviceId)
            && !string.IsNullOrWhiteSpace(dto.Metric)
            && dto.Seq >= 0;
    }
}