using GarageV3.Models.Entities;
using GarageV3.Models.Parking;
using GarageV3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GarageV3.Services
{
    public class ParkingSpotService : IParkingSpotService
    {
        private readonly ApplicationDbContext _context;
        private readonly GarageSettings _settings;

        public ParkingSpotService(ApplicationDbContext context, IOptions<GarageSettings> options)
        {
            _context = context;
            _settings = options.Value;
        }

        public int TotalSpots => _settings.TotalParkingSpots;

        public int GetFreeSpotCount()
        {
            return GetSpotOverview().Count(s => s.IsFree);
        }

        public IReadOnlyList<ParkingSpotInfo> GetSpotOverview()
        {
            var spots = Enumerable.Range(1, TotalSpots)
                .Select(n => new ParkingSpotInfo { SpotNumber = n, IsFree = true })
                .ToList();

            var parkedVehicles = _context.Vehicles
                .Include(v => v.VehicleTypeRef)
                .Where(v => v.AssignedSpotNumber != null)
                .ToList();

            foreach (var vehicle in parkedVehicles)
            {
                if (vehicle.VehicleTypeRef == null) continue;

                int start = vehicle.AssignedSpotNumber!.Value;
                var vType = vehicle.VehicleTypeRef;

                if (vType.MaxVehiclesPerSpot > 1)
                {
                    var spot = spots.FirstOrDefault(s => s.SpotNumber == start);
                    if (spot == null) continue;

                    if (spot.OccupyingVehicleTypeEntityId == null)
                    {
                        spot.OccupyingVehicleTypeEntityId = vType.Id;
                    }

                    spot.MotorcycleSlotsUsed++;
                    
                    if (spot.OccupyingVehicleRegNums == null)
                    {
                        spot.OccupyingVehicleRegNums = new string[vType.MaxVehiclesPerSpot];
                    }
                    
                    int slotIndex = Array.IndexOf(spot.OccupyingVehicleRegNums, null);
                    if (slotIndex >= 0 && slotIndex < vType.MaxVehiclesPerSpot)
                    {
                        spot.OccupyingVehicleRegNums[slotIndex] = vehicle.RegistrationNumber;
                    }

                    int currentCount = spot.OccupyingVehicleRegNums.Count(r => r != null);
                    if (currentCount >= vType.MaxVehiclesPerSpot)
                    {
                        spot.IsFree = false;
                    }
                }

                else
                {
                    int required = vType.RequiredSpots;

                    for (int i = 0; i < required; i++)
                    {
                        var spot = spots.FirstOrDefault(s => s.SpotNumber == start + i);
                        if (spot == null) continue;

                        spot.IsFree = false;
                        spot.OccupyingVehicleTypeEntityId = vType.Id;
                        spot.OccupyingVehicleId = vehicle.Id;
                        
                        if (spot.OccupyingVehicleRegNums == null)
                        {
                            spot.OccupyingVehicleRegNums = new string[1];
                        }
                        spot.OccupyingVehicleRegNums[0] = vehicle.RegistrationNumber;

                        if (required > 1)
                        {
                            spot.IsLeftSpot = (i == 0);
                            spot.IsMiddleSpot = (i > 0 && i < required - 1);
                            spot.IsRightSpot = (i == required - 1);
                        }
                    }
                }
            }

            return spots;
        }

        public bool CanParkVehicleType(VehicleTypeEntity vehicleType)
        {
            var overview = GetSpotOverview();

            if (vehicleType.MaxVehiclesPerSpot > 1)
            {
                return HasFreeSharedSlot(overview, vehicleType.Id);
            }

            return FindContiguousFreeStart(overview, vehicleType.RequiredSpots) != null;
        }

        public IReadOnlyDictionary<int, bool> GetVehicleTypeAvailability()
        {
            var overview = GetSpotOverview();
            var result = new Dictionary<int, bool>();
            var allTypes = _context.VehicleTypes.ToList();

            foreach (var type in allTypes)
            {
                if (type.MaxVehiclesPerSpot > 1)
                {
                    result[type.Id] = HasFreeSharedSlot(overview, type.Id);
                }
                else
                {
                    result[type.Id] = FindContiguousFreeStart(overview, type.RequiredSpots) != null;
                }
            }

            return result;
        }

        public ParkingAssignmentResult AssignSpot(int vehicleId)
        {
            var vehicle = _context.Vehicles
                .Include(v => v.VehicleTypeRef)
                .FirstOrDefault(v => v.Id == vehicleId);

            if (vehicle == null || vehicle.VehicleTypeRef == null)
            {
                return ParkingAssignmentResult.Fail("Vehicle or Vehicle Type not found.");
            }

            var vType = vehicle.VehicleTypeRef;
            var overview = GetSpotOverview();

            if (vType.MaxVehiclesPerSpot > 1)
            {
                var spot = overview.FirstOrDefault(s =>
                    s.IsFree &&
                    (s.OccupyingVehicleTypeEntityId == null || s.OccupyingVehicleTypeEntityId == vType.Id));

                var targetSpot = overview.FirstOrDefault(s =>
                    (s.OccupyingVehicleTypeEntityId == null || s.OccupyingVehicleTypeEntityId == vType.Id) &&
                    (s.OccupyingVehicleRegNums == null || s.OccupyingVehicleRegNums.Count(r => r != null) < vType.MaxVehiclesPerSpot) &&
                    s.IsFree);

                if (targetSpot == null)
                {
                    targetSpot = overview.FirstOrDefault(s => s.IsFree && s.OccupyingVehicleRegNums == null);
                }

                if (targetSpot == null)
                {
                    return ParkingAssignmentResult.Fail($"No available shared slot for {vType.Name}.");
                }

                vehicle.AssignedSpotNumber = targetSpot.SpotNumber;
                _context.SaveChanges();
                return ParkingAssignmentResult.Ok(new List<int> { targetSpot.SpotNumber });
            }
            else
            {
                int required = vType.RequiredSpots;
                int? start = FindContiguousFreeStart(overview, required);

                if (start == null)
                {
                    return ParkingAssignmentResult.Fail($"Not enough contiguous free spots for {vType.Name}.");
                }

                vehicle.AssignedSpotNumber = start;
                _context.SaveChanges();

                var assignedSpots = Enumerable.Range(start.Value, required).ToList();
                return ParkingAssignmentResult.Ok(assignedSpots);
            }
        }

        public void ReleaseSpot(int vehicleId)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.AssignedSpotNumber = null;
            _context.SaveChanges();
        }

        private static bool HasFreeSharedSlot(IReadOnlyList<ParkingSpotInfo> overview, int typeId)
        {
            return overview.Any(s =>
                s.IsFree || 
                (s.OccupyingVehicleTypeEntityId == typeId && s.OccupyingVehicleRegNums?.Count(r => r != null) < 5));
        }

        private static int? FindContiguousFreeStart(IReadOnlyList<ParkingSpotInfo> overview, int required)
        {
            for (int start = 1; start <= overview.Count - required + 1; start++)
            {
                bool allFree = true;

                for (int i = 0; i < required; i++)
                {
                    var spot = overview[start - 1 + i];
                    if (!spot.IsFree || spot.OccupyingVehicleRegNums != null)
                    {
                        allFree = false;
                        break;
                    }
                }

                if (allFree) return start;
            }

            return null;
        }
    }
}