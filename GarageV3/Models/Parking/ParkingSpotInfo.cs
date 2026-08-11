using GarageV3.Models.Enums;

namespace GarageV3.Models.Parking
{
    /// <summary>
    /// Represents the current state of a single physical parking spot,
    /// used for the overview view (free/occupied per spot).
    /// </summary>
    public class ParkingSpotInfo
    {
        public int SpotNumber { get; set; }

        public bool IsFree { get; set; }

        public int? OccupyingVehicleTypeEntityId { get; set; }

        /// <summary>
        /// Set when a non-motorcycle vehicle occupies this spot
        /// (possibly spanning into following spot numbers).
        /// </summary>
        public VehicleType? OccupyingVehicleType { get; set; }

        public int? OccupyingVehicleId { get; set; }
        public string[]? OccupyingVehicleRegNums { get; set; }

        /// <summary>
        /// How many of the motorcycle slots (0-3) on this spot are currently used.
        /// Only relevant when the spot is being used for motorcycles.
        /// </summary>
        public int MotorcycleSlotsUsed { get; set; }
        public int BicycleSlotsUsed { get; set; }

        public bool IsLeftSpot { get; set; }
        public bool IsMiddleSpot { get; set; }
        public bool IsRightSpot { get; set; }
    }
}