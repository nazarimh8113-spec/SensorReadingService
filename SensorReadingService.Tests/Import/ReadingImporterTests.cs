using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Application.Services;
using SensorReadingService.Domain.Entities;
using SensorReadingService.Domain.ValueObjects;
using System.Text.Json;
using Xunit;

namespace SensorReadingService.Tests.Import;

public class ReadingImporterTests
{
    [Fact]
    public void Constructor_Should_Create_Service()
    {
        // Arrange
        var fileReader = new Mock<IFileReader>();
        var repository = new Mock<IReadingRepository>();
        var logger = new Mock<ILogger<ReadingImporter>>();

        // Act
        var service = new ReadingImporter(
            fileReader.Object,
            repository.Object,
            logger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportAsync_Should_Store_Valid_Reading()
    {
        // Arrange

        var json =
            """
            {"deviceId":"PUMP-01","metric":"temperature","ts":"2025-06-01T08:21:30Z","value":68.936,"seq":1130}
            """;

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(x => x.ReadLinesAsync(It.IsAny<string>()))
            .Returns(GetLines(json));

        var repository = new Mock<IReadingRepository>();

        repository
            .Setup(x => x.GetExistingIdentitiesAsync(default))
            .ReturnsAsync(new HashSet<ReadingIdentity>());

        var logger = new Mock<ILogger<ReadingImporter>>();

        var importer = new ReadingImporter(
            fileReader.Object,
            repository.Object,
            logger.Object);

        // Act

        var report = await importer.ImportAsync("dummy.jsonl");

        // Assert

        report.TotalLines.Should().Be(1);
        report.StoredReadings.Should().Be(1);
        report.InvalidReadings.Should().Be(0);
        report.DuplicateReadings.Should().Be(0);

        repository.Verify(x =>
            x.AddRangeAsync(
                It.IsAny<IEnumerable<Reading>>(),
                default),
            Times.Once);

        repository.Verify(x =>
            x.SaveChangesAsync(default),
            Times.Once);
    }

    private static async IAsyncEnumerable<string> GetLines(params string[] lines)
    {
        foreach (var line in lines)
        {
            yield return line;
            await Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ImportAsync_Should_Skip_Duplicate_Reading()
    {
        // Arrange

        var json =
            """
            {"deviceId":"PUMP-01","metric":"temperature","ts":"2025-06-01T08:21:30Z","value":68.936,"seq":1130}
            """;

        // دقیقا همان ReadingDto که سرویس Deserialize می‌کند
        var dto = JsonSerializer.Deserialize<ReadingDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        var duplicateIdentity = new ReadingIdentity(
            dto.DeviceId,
            dto.Metric,
            dto.Ts,
            dto.Seq);

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(x => x.ReadLinesAsync(It.IsAny<string>()))
            .Returns(GetLines(json));

        var repository = new Mock<IReadingRepository>();

        repository
            .Setup(x => x.GetExistingIdentitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<ReadingIdentity>
            {
                duplicateIdentity
            });

        var logger = new Mock<ILogger<ReadingImporter>>();

        var importer = new ReadingImporter(
            fileReader.Object,
            repository.Object,
            logger.Object);

        // Act

        var report = await importer.ImportAsync(
            "dummy.jsonl",
            CancellationToken.None);

        // Assert

        report.TotalLines.Should().Be(1);
        report.StoredReadings.Should().Be(0);
        report.InvalidReadings.Should().Be(0);
        report.DuplicateReadings.Should().Be(1);

        repository.Verify(x =>
            x.AddRangeAsync(
                It.IsAny<IEnumerable<Reading>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(x =>
            x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Count_Invalid_Json()
    {
        // Arrange

        var invalidJson =
            """
        { invalid json }
        """;

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(x => x.ReadLinesAsync(It.IsAny<string>()))
            .Returns(GetLines(invalidJson));

        var repository = new Mock<IReadingRepository>();

        repository
            .Setup(x => x.GetExistingIdentitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<ReadingIdentity>());

        var logger = new Mock<ILogger<ReadingImporter>>();

        var importer = new ReadingImporter(
            fileReader.Object,
            repository.Object,
            logger.Object);

        // Act

        var report = await importer.ImportAsync(
            "dummy.jsonl",
            CancellationToken.None);

        // Assert

        report.TotalLines.Should().Be(1);
        report.StoredReadings.Should().Be(0);
        report.InvalidReadings.Should().Be(1);
        report.DuplicateReadings.Should().Be(0);

        repository.Verify(x =>
            x.AddRangeAsync(
                It.IsAny<IEnumerable<Reading>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(x =>
            x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

}