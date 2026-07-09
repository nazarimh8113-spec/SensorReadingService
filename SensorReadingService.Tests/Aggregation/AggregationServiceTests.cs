using FluentAssertions;
using Moq;
using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Application.Services;
using SensorReadingService.Domain.Entities;
using Xunit;

namespace SensorReadingService.Tests.Aggregation;

public class AggregationServiceTests
{
    [Fact]
    public async Task AggregateAsync_Should_Return_Correct_Aggregation()
    {
        // Arrange

        var readings = new List<Reading>
        {
            new Reading("PUMP-01","temperature",
                new DateTime(2025,6,1,8,0,0),
                10,
                1),

            new Reading("PUMP-01","temperature",
                new DateTime(2025,6,1,8,5,0),
                20,
                2),

            new Reading("PUMP-01","temperature",
                new DateTime(2025,6,1,8,10,0),
                30,
                3)
        };

        var repository = new Mock<IReadingRepository>();

        repository
     .Setup(x => x.GetReadingsAsync(
         It.IsAny<string>(),
         It.IsAny<string>(),
         It.IsAny<DateTime>(),
         It.IsAny<DateTime>(),
         It.IsAny<CancellationToken>()))
     .ReturnsAsync(readings);

        var service = new AggregationService(repository.Object);

        var request = new AggregateRequestDto
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            From = new DateTime(2025, 6, 1, 8, 0, 0),
            To = new DateTime(2025, 6, 1, 8, 15, 0),
            BucketSizeInMinutes = 15
        };

        // Act

        var result = (await service.AggregateAsync(request)).First();

        // Assert

        result.Count.Should().Be(3);
        result.Average.Should().Be(20);
        result.Min.Should().Be(10);
        result.Max.Should().Be(30);
    }


    [Fact]
    public async Task AggregateAsync_Should_Return_Empty_Bucket_When_No_Readings_Exist()
    {
        // Arrange

        var repository = new Mock<IReadingRepository>();

        repository
            .Setup(x => x.GetReadingsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reading>());

        var service = new AggregationService(repository.Object);

        var request = new AggregateRequestDto
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            From = new DateTime(2025, 6, 1, 8, 0, 0),
            To = new DateTime(2025, 6, 1, 8, 15, 0),
            BucketSizeInMinutes = 15
        };

        // Act

        var result = await service.AggregateAsync(request);

        // Assert

        result.Should().HaveCount(1);

        var bucket = result.First();

        bucket.BucketStart.Should().Be(request.From);
        bucket.BucketEnd.Should().Be(request.From.AddMinutes(15));

        bucket.Count.Should().Be(0);
        bucket.Average.Should().BeNull();
        bucket.Min.Should().BeNull();
        bucket.Max.Should().BeNull();

        repository.Verify(x =>
            x.GetReadingsAsync(
                request.DeviceId,
                request.Metric,
                request.From,
                request.To,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

}