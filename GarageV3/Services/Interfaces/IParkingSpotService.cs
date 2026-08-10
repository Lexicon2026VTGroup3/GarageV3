using GarageV3.Models.Enums;
using GarageV3.Models.Parking;
using VehicleType = GarageV3.Models.Enums.VehicleType;

namespace GarageV3.Services
{
    /// <summary>
    /// Handles fixed parking spot allocation, availability overview,
    /// and vehicle type validity checks for del 2 (utökad funktionalitet).
    /// </summary>
    public interface IParkingSpotService
    {
        /// <summary>
        /// Total number of physical parking spots in the garage.
        /// </summary>
        int TotalSpots { get; }

        /// <summary>
        /// Number of fully free spots right now (used for the landing page counter).
        /// A spot partially used by motorcycles still counts as occupied here.
        /// </summary>
        int GetFreeSpotCount();

        /// <summary>
        /// Full spot-by-spot overview, for the extended overview view.
        /// </summary>
        IReadOnlyList<ParkingSpotInfo> GetSpotOverview();

        /// <summary>
        /// Checks whether there is currently room to park a vehicle of the given type
        /// (e.g. enough contiguous free spots for a boat, or a free motorcycle slot).
        /// </summary>
        bool CanParkVehicleType(VehicleType type);

        /// <summary>
        /// Returns availability for every vehicle type, used to gray out
        /// invalid options in the parking dropdown.
        /// </summary>
        IReadOnlyDictionary<VehicleType, bool> GetVehicleTypeAvailability();

        /// <summary>
        /// Attempts to assign spot(s) to a vehicle. Fails if there isn't room.
        /// </summary>
        ParkingAssignmentResult AssignSpot(VehicleType type, int vehicleId);

        /// <summary>
        /// Frees up the spot(s) held by a vehicle (called on check-out).
        /// </summary>
        void ReleaseSpot(int vehicleId);
    }
}