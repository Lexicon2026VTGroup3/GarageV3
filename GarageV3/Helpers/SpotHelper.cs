using GarageV3.Models.Parking;

namespace GarageV3.Helpers
{
    public static class SpotHelper
    {
        // ToDo: Delete after delete VehicleType Enum
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

        // Display parking spot range based on DB VehicleType.RequiredSpots
        public static string GetSpotDisplay(int? assignedSpotNumber, int requiredSpots)
        {
            if (!assignedSpotNumber.HasValue)
                return "Not assigned";

            int start = assignedSpotNumber.Value;
            int end = start + requiredSpots - 1;

            return requiredSpots > 1
                ? $"#{start} - #{end}"
                : $"#{start}";
        }
    }
}