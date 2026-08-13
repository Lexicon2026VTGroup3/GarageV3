using GarageV3.Data;
using GarageV3.Models.Parking;

namespace GarageV3.Services;

public class GarageFeeService
{
    private const decimal ProDiscountPercentage = 0.20m; // 20% discount for Pro-members

    public decimal CalculateRawFee(DateTime arrivalTime, DateTime departureTime, decimal hourlyRate)
    {
        // Ensure both dates are normalized to UTC for comparison
        var start = arrivalTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(arrivalTime, DateTimeKind.Utc)
            : arrivalTime.ToUniversalTime();

        var end = departureTime.ToUniversalTime();

        if (end <= start) return 0m;

        decimal totalFee = 0m;
        var currentTime = start;

        while (currentTime < end)
        {
            if (currentTime.DayOfWeek != DayOfWeek.Sunday)
            {
                if (currentTime.Hour >= 6 && currentTime.Hour < 20)
                {
                    totalFee += hourlyRate / 60m;
                }
                else
                {
                    totalFee += 2m / 60m;
                }
            }

            currentTime = currentTime.AddMinutes(1);
        }

        return Math.Round(totalFee, 2);
    }

    public decimal CalculateFee(DateTime arrivalTime, DateTime departureTime, decimal hourlyRate, bool isProMember = false)
    {
        decimal totalFee = CalculateRawFee(arrivalTime, departureTime, hourlyRate);

        if (isProMember)
        {
            totalFee *= (1m - ProDiscountPercentage);
        }

        return Math.Round(totalFee, 2);
    }

    public FeeCalculationResult CalculateDetailedFee(DateTime arrivalTime, DateTime departureTime, decimal hourlyRate, bool isProMember)
    {
        decimal grossFee = CalculateRawFee(arrivalTime, departureTime, hourlyRate);

        if (!isProMember)
        {
            return new FeeCalculationResult
            {
                GrossFee = grossFee,
                DiscountPercentage = 0,
                TotalPrice = grossFee
            };
        }

        decimal finalFee = Math.Round(grossFee * (1m - ProDiscountPercentage), 2);

        return new FeeCalculationResult
        {
            GrossFee = grossFee,
            DiscountPercentage = ProDiscountPercentage,
            TotalPrice = finalFee
        };
    }
}