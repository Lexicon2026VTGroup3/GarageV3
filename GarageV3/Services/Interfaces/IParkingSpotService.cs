using GarageV3.Models.Entities;
using GarageV3.Models.Parking;

namespace GarageV3.Services
{
    /// <summary>
    /// Handles fixed parking spot allocation, availability overview,
    /// and vehicle type validity checks based on dynamic database entities.
    /// </summary>
    public interface IParkingSpotService
    {
        /// <summary>
        /// Total number of physical parking spots in the garage.
        /// </summary>
        int TotalSpots { get; }

        /// <summary>
        /// Number of fully free spots right now (used for the landing page counter).
        /// </summary>
        int GetFreeSpotCount();

        /// <summary>
        /// Full spot-by-spot overview.
        /// </summary>
        IReadOnlyList<ParkingSpotInfo> GetSpotOverview();

        /// <summary>
        /// Checks whether there is currently room to park a vehicle of the given entity type.
        /// </summary>
        bool CanParkVehicleType(VehicleTypeEntity vehicleType);

        /// <summary>
        /// Returns availability for every vehicle type (keyed by VehicleTypeEntity Id), 
        /// used to gray out invalid options in the parking dropdown.
        /// </summary>
        IReadOnlyDictionary<int, bool> GetVehicleTypeAvailability();

        /// <summary>
        /// Attempts to assign spot(s) to a vehicle using its ID. Fails if there isn't room.
        /// </summary>
        ParkingAssignmentResult AssignSpot(int vehicleId);

        /// <summary>
        /// Frees up the spot(s) held by a vehicle (called on check-out).
        /// </summary>
        void ReleaseSpot(int vehicleId);
    }
}