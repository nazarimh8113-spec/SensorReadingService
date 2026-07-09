using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;

namespace SensorReadingService.Application.Services;

public class AggregationService : IAggregationService
{
    private readonly IReadingRepository _repository;

    public AggregationService(IReadingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AggregateResultDto>> AggregateAsync(
        AggregateRequestDto request)
    {
        Validate(request);

        var readings = await _repository.GetReadingsAsync(
            request.DeviceId,
            request.Metric,
            request.From,
            request.To);

        var bucketSize = TimeSpan.FromMinutes(request.BucketSizeInMinutes);

        var result = new List<AggregateResultDto>();

        var bucketStart = request.From;

        while (bucketStart < request.To)
        {
            var bucketEnd = bucketStart.Add(bucketSize);

            var bucketReadings = readings
                .Where(x =>
                    x.Timestamp >= bucketStart &&
                    x.Timestamp < bucketEnd)
                .ToList();

            if (bucketReadings.Any())
            {
                result.Add(new AggregateResultDto
                {
                    BucketStart = bucketStart,
                    BucketEnd = bucketEnd,
                    Count = bucketReadings.Count,
                    Average = bucketReadings.Average(x => x.Value),
                    Min = bucketReadings.Min(x => x.Value),
                    Max = bucketReadings.Max(x => x.Value)
                });
            }
            else
            {
                result.Add(new AggregateResultDto
                {
                    BucketStart = bucketStart,
                    BucketEnd = bucketEnd,
                    Count = 0,
                    Average = null,
                    Min = null,
                    Max = null
                });
            }

            bucketStart = bucketEnd;
        }

        return result;
    }

    private static void Validate(AggregateRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
            throw new ArgumentException("DeviceId is required.");

        if (string.IsNullOrWhiteSpace(request.Metric))
            throw new ArgumentException("Metric is required.");

        if (request.From >= request.To)
            throw new ArgumentException("From must be earlier than To.");

        if (request.BucketSizeInMinutes <= 0)
            throw new ArgumentException("BucketSizeInMinutes must be greater than zero.");
    }
}