using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Services
{
    public class ParkingAllocationService : IParkingAllocationService
    {
        private readonly ApplicationDbContext _context;

        public ParkingAllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AllocationResult> AllocateAndStartSessionAsync(int vehicleId, int? preferredSpotId, decimal hourlyRate)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleTypeRef)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle?.VehicleTypeRef == null)
            {
                return AllocationResult.Fail("Vehicle or vehicle type not found.");
            }

            var vehicleType = vehicle.VehicleTypeRef;
            var plan = await BuildAllocationPlanAsync(vehicleType.RequiredSpaceUnits, preferredSpotId);

            if (plan == null)
            {
                return AllocationResult.Fail($"No available spot(s) for {vehicleType.Name} right now.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var session = new ParkingSession
                {
                    VehicleId = vehicleId,
                    ParkingSpotId = plan[0].SpotId, // primary spot, kept for backward compatibility
                    ArriveTime = DateTime.UtcNow,
                    HourlyRateAtCheckIn = hourlyRate
                };

                _context.ParkingSessions.Add(session);
                await _context.SaveChangesAsync();

                var allocations = plan
                    .Select(p => new ParkingAllocation
                    {
                        ParkingSessionId = session.Id,
                        ParkingSpotId = p.SpotId,
                        UnitsUsed = p.UnitsUsed
                    })
                    .ToList();

                _context.ParkingAllocations.AddRange(allocations);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return AllocationResult.Ok(session, allocations);
            }
            catch
            {
                await transaction.RollbackAsync();
                return AllocationResult.Fail("Could not save the parking session. No changes were made.");
            }
        }

        public async Task ReleaseAllocationsAsync(int parkingSessionId)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> CanAllocateAsync(int vehicleTypeId)
        {
            var vehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId);
            if (vehicleType == null) return false;

            var plan = await BuildAllocationPlanAsync(vehicleType.RequiredSpaceUnits, preferredSpotId: null);
            return plan != null;
        }

        private async Task<List<(int SpotId, int UnitsUsed)>?> BuildAllocationPlanAsync(int requiredUnits, int? preferredSpotId)
        {
            var spots = await _context.ParkingSpots
                .Where(s => !s.IsOutOfService)
                .OrderBy(s => s.Number)
                .ToListAsync();

            if (spots.Count == 0) return null;

            var usedUnitsBySpot = await _context.ParkingAllocations
                .Where(a => a.ParkingSession!.CheckOutTime == null)
                .GroupBy(a => a.ParkingSpotId)
                .Select(g => new { SpotId = g.Key, Used = g.Sum(a => a.UnitsUsed) })
                .ToDictionaryAsync(x => x.SpotId, x => x.Used);

            int FreeUnits(ParkingSpot spot) =>
                spot.CapacityUnits - usedUnitsBySpot.GetValueOrDefault(spot.Id, 0);

            if (preferredSpotId != null)
            {
                var preferred = spots.FirstOrDefault(s => s.Id == preferredSpotId);
                if (preferred != null && requiredUnits <= preferred.CapacityUnits && FreeUnits(preferred) >= requiredUnits)
                {
                    return new List<(int, int)> { (preferred.Id, requiredUnits) };
                }
            }

            var singleSpot = spots.FirstOrDefault(s =>
                requiredUnits <= s.CapacityUnits && FreeUnits(s) >= requiredUnits);

            if (singleSpot != null)
            {
                return new List<(int, int)> { (singleSpot.Id, requiredUnits) };
            }

            for (int startIndex = 0; startIndex < spots.Count; startIndex++)
            {
                if (FreeUnits(spots[startIndex]) != spots[startIndex].CapacityUnits)
                {
                    continue;
                }

                var window = new List<ParkingSpot> { spots[startIndex] };
                int coveredUnits = spots[startIndex].CapacityUnits;
                int i = startIndex;

                while (coveredUnits < requiredUnits && i + 1 < spots.Count
                       && spots[i + 1].Number == spots[i].Number + 1
                       && FreeUnits(spots[i + 1]) == spots[i + 1].CapacityUnits)
                {
                    i++;
                    window.Add(spots[i]);
                    coveredUnits += spots[i].CapacityUnits;
                }

                if (coveredUnits >= requiredUnits)
                {
                    return window.Select(s => (s.Id, s.CapacityUnits)).ToList();
                }
            }

            return null;
        }
    }
}