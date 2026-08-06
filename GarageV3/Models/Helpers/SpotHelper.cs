using GarageV3.Models.Parking;
using GarageV3.Models.Enums;

namespace GarageV3.Helpers
{
    public static class SpotHelper
    {
        public static string GetSpotDisplay(int? assignedSpotNumber, VehicleType vehicleType)
        {
            if (!assignedSpotNumber.HasValue)
                return "Not assigned";

            int spotSpan = VehicleSpotRequirement.GetRequiredWholeSpots(vehicleType);
            int start = assignedSpotNumber.Value;
            int end = start + spotSpan - 1;

            return spotSpan > 1
                ? $"#{start} - #{end}"
                : $"#{start}";
        }
    }
}
