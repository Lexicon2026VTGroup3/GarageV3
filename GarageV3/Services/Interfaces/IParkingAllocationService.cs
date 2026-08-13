using GarageV3.Models.Entities;

namespace GarageV3.Services.Interfaces
{
    public interface IParkingAllocationService
    {
        /// <summary>
        /// Creates a new ParkingSession together with the ParkingAllocation(s)
        /// it needs, in one transaction. If a preferredSpotId is given and it
        /// has enough free capacity for the vehicle's type, that spot is used;
        /// otherwise (or for vehicle types needing multiple spots) a spot or
        /// set of contiguous spots is found automatically. Either fully
        /// succeeds (session + all allocations saved) or fully fails (nothing
        /// saved) — see US12 acceptance criterion "En misslyckad allokering
        /// lämnar inte ofullständig data".
        /// </summary>
        Task<AllocationResult> AllocateAndStartSessionAsync(int vehicleId, int? preferredSpotId, decimal hourlyRate);

        /// <summary>
        /// Frees every allocation belonging to a session (called on checkout).
        /// Does not delete the allocation rows themselves — history is kept,
        /// same as ParkingSession rows are never deleted.
        /// </summary>
        Task ReleaseAllocationsAsync(int parkingSessionId);

        /// <summary>
        /// Checks whether there is currently room for this vehicle type,
        /// without allocating anything. Used to filter/grey out vehicle
        /// types that can't fit right now.
        /// </summary>
        Task<bool> CanAllocateAsync(int vehicleTypeId);
    }

    public class AllocationResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public ParkingSession? Session { get; init; }
        public IReadOnlyList<ParkingAllocation> Allocations { get; init; } = new List<ParkingAllocation>();

        public static AllocationResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };

        public static AllocationResult Ok(ParkingSession session, IReadOnlyList<ParkingAllocation> allocations) =>
            new() { Success = true, Session = session, Allocations = allocations };
    }
}