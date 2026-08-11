using GarageV3.Models.Parking;

namespace GarageV3.Helpers
{
    public static class SpotHelper
    {
        public static string GetSpotDisplay(int? assignedSpotNumber, string vehicleTypeName)
        {
            if (!assignedSpotNumber.HasValue)
                return "Not assigned";

            int spotSpan = VehicleSpotRequirement.GetRequiredWholeSpots(vehicleTypeName);
            int start = assignedSpotNumber.Value;
            int end = start + spotSpan - 1;

            return spotSpan > 1
                ? $"#{start} - #{end}"
                : $"#{start}";
        }
    }
}