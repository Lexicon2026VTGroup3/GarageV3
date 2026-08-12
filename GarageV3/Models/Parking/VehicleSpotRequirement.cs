namespace GarageV3.Models.Parking
{
    /// <summary>
    /// Defines how many parking spots each vehicle type requires,
    /// and whether the type uses fractional (shared) spots.
    /// </summary>
    public static class VehicleSpotRequirement
    {
        /// <summary>
        /// Number of motorcycles allowed to share a single spot.
        /// </summary>
        public const int MotorcycleSlotsPerSpot = 3;

        public const int BicycleSlotsPerSpot = 5;

        /// <summary>
        /// Returns how many whole, contiguous spots a vehicle type requires based on its name.
        /// Returns 0 for motorcycles and bicycles, since they use fractional spots instead.
        /// </summary>
        public static int GetRequiredWholeSpots(string vehicleTypeName)
        {
            return vehicleTypeName?.ToLower() switch
            {
                "truck" => 2,
                "bus" => 2,
                "airplane" => 3,
                "boat" => 3,
                "motorcycle" => 0,
                "bicycle" => 0,
                _ => 1 // Car / Default
            };
        }

        public static bool IsMotorcycleType(string vehicleTypeName)
        {
            return string.Equals(vehicleTypeName, "Motorcycle", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBicycleType(string vehicleTypeName)
        {
            return string.Equals(vehicleTypeName, "Bicycle", StringComparison.OrdinalIgnoreCase);
        }
    }
}