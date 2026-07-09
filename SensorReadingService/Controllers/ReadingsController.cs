using Microsoft.AspNetCore.Mvc;
using SensorReadingService.Application.DTOs;
using SensorReadingService.Application.Interfaces;

namespace SensorReadingService.Controllers;

[ApiController]
[Route("api/readings")]
public class ReadingsController : ControllerBase
{
    private readonly IAggregationService _aggregationService;

    public ReadingsController(
        IAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    [HttpGet("aggregate")]
    public async Task<IActionResult> Aggregate(
        [FromQuery] AggregateRequestDto request)
    {
        var result = await _aggregationService.AggregateAsync(request);

        return Ok(result);
    }
}