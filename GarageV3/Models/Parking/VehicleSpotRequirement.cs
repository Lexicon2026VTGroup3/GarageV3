using GarageV3.Models.Enums;

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
        /// Returns how many whole, contiguous spots a vehicle type requires.
        /// Returns 0 for motorcycles, since they use fractional spots instead.
        /// </summary>
        public static int GetRequiredWholeSpots(VehicleType type)
        {
            return type switch
            {
                VehicleType.Truck => 2,
                VehicleType.Bus => 2,
                VehicleType.Airplane => 3,
                VehicleType.Boat => 3,
                VehicleType.Motorcycle => 0,
                VehicleType.Bicycle => 0,
                _ => 1 // Car
            };
        }

        public static bool IsMotorcycleType(VehicleType type)
        {
            return type == VehicleType.Motorcycle;
        }

        public static bool IsBicycleType(VehicleType type)
        {
            return type == VehicleType.Bicycle;
        }
    }
}