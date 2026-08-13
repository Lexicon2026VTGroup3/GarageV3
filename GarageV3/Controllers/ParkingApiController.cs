using GarageV3.Data;
using GarageV3.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GarageV3.Controllers;

[ApiController]
[Route("api/parking")]
public class ParkingApiController : ControllerBase
{
    private readonly GarageFeeService _garageFeeService;

    public ParkingApiController(GarageFeeService garageFeeService)
    {
        _garageFeeService = garageFeeService;
    }

    [HttpGet("calculate-fee")]
    public async Task<IActionResult> CalculateFee([FromQuery] DateTime arrivalTime, [FromQuery] decimal hourlyRate, [FromQuery] bool isPro)
    {
        // Ensure arrivalTime is treated as UTC if passed as unspecified
        var utcArrival = arrivalTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(arrivalTime, DateTimeKind.Utc)
            : arrivalTime.ToUniversalTime();

        hourlyRate = Math.Round(hourlyRate, 2);
        var fee = _garageFeeService.CalculateFee(utcArrival, DateTime.UtcNow, hourlyRate, isPro);

        return Ok(new { totalPrice = fee });
    }
}