using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Models.Parking;
using GarageV3.Data;
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
            // Start with every spot free
            var spots = Enumerable.Range(1, TotalSpots)
                .Select(n => new ParkingSpotInfo { SpotNumber = n, IsFree = true })
                .ToList();

            var parkedVehicles = _context.ParkedVehicle
                .Where(v => v.AssignedSpotNumber != null)
                .ToList();

            foreach (var vehicle in parkedVehicles)
            {
                int start = vehicle.AssignedSpotNumber!.Value;

                if (VehicleSpotRequirement.IsMotorcycleType(vehicle.VehicleType))
                {
                    var spot = spots.FirstOrDefault(s => s.SpotNumber == start);
                    if (spot == null) continue;

                    spot.MotorcycleSlotsUsed++;
                    spot.OccupyingVehicleType = VehicleType.Motorcycle;
                    if (spot.OccupyingVehicleRegNums == null)
                    {
                        spot.OccupyingVehicleRegNums = new string[VehicleSpotRequirement.MotorcycleSlotsPerSpot];
                    }
                    spot.OccupyingVehicleRegNums[spot.MotorcycleSlotsUsed - 1] = vehicle.RegistrationNumber;

                    if (spot.MotorcycleSlotsUsed >= VehicleSpotRequirement.MotorcycleSlotsPerSpot)
                    {
                        spot.IsFree = false;
                    }
                } else if (VehicleSpotRequirement.IsBicycleType(vehicle.VehicleType))
                {
                    var spot = spots.FirstOrDefault(s => s.SpotNumber == start);
                    if (spot == null) continue;

                    spot.BicycleSlotsUsed++;
                    spot.OccupyingVehicleType = VehicleType.Bicycle;
                    if (spot.OccupyingVehicleRegNums == null)
                    {
                        spot.OccupyingVehicleRegNums = new string[VehicleSpotRequirement.BicycleSlotsPerSpot];
                    }
                    spot.OccupyingVehicleRegNums[spot.BicycleSlotsUsed - 1] = vehicle.RegistrationNumber;


                    if (spot.BicycleSlotsUsed >= VehicleSpotRequirement.BicycleSlotsPerSpot)
                    {
                        spot.IsFree = false;
                    }
                }
                else
                {
                    int required = VehicleSpotRequirement.GetRequiredWholeSpots(vehicle.VehicleType);

                    for (int i = 0; i < required; i++)
                    {
                        var spot = spots.FirstOrDefault(s => s.SpotNumber == start + i);
                        if (spot == null) continue;

                        spot.IsFree = false;
                        spot.OccupyingVehicleType = vehicle.VehicleType;
                        spot.OccupyingVehicleId = vehicle.Id;
                        if (spot.OccupyingVehicleRegNums == null)
                        {
                            spot.OccupyingVehicleRegNums = new string[1];
                        }
                        spot.OccupyingVehicleRegNums[0] = vehicle.RegistrationNumber;

                        if (required > 1) {
                            spot.IsLeftSpot = (i == 0);
                            spot.IsMiddleSpot = (i > 0 && i < required - 1);
                            spot.IsRightSpot = (i == required - 1);
                        }
                    }
                }
            }

            return spots;
        }

        public bool CanParkVehicleType(VehicleType type)
        {
            var overview = GetSpotOverview();

            if (VehicleSpotRequirement.IsMotorcycleType(type))
            {
                return HasFreeMotorcycleSlot(overview);
            } else if (VehicleSpotRequirement.IsBicycleType(type))
            {
                return HasFreeBicycleSlot(overview);
            }

            int required = VehicleSpotRequirement.GetRequiredWholeSpots(type);
            return FindContiguousFreeStart(overview, required) != null;
        }

        public IReadOnlyDictionary<VehicleType, bool> GetVehicleTypeAvailability()
        {
            var overview = GetSpotOverview();
            var result = new Dictionary<VehicleType, bool>();

            foreach (VehicleType type in Enum.GetValues(typeof(VehicleType)))
            {
                if (VehicleSpotRequirement.IsMotorcycleType(type))
                {
                    result[type] = HasFreeMotorcycleSlot(overview);
                } else if (VehicleSpotRequirement.IsBicycleType(type))
                {
                    result[type] = HasFreeBicycleSlot(overview);
                }
                else
                {
                    int required = VehicleSpotRequirement.GetRequiredWholeSpots(type);
                    result[type] = FindContiguousFreeStart(overview, required) != null;
                }
            }

            return result;
        }

        public ParkingAssignmentResult AssignSpot(VehicleType type, int vehicleId)
        {
            var vehicle = _context.ParkedVehicle.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                return ParkingAssignmentResult.Fail("Vehicle not found.");
            }

            var overview = GetSpotOverview();

            if (VehicleSpotRequirement.IsMotorcycleType(type))
            {
                var spot = overview.FirstOrDefault(s =>
                    s.MotorcycleSlotsUsed < VehicleSpotRequirement.MotorcycleSlotsPerSpot &&
                    (s.OccupyingVehicleType == null || s.OccupyingVehicleType == VehicleType.Motorcycle));

                if (spot == null)
                {
                    return ParkingAssignmentResult.Fail("No motorcycle slot available.");
                }

                vehicle.AssignedSpotNumber = spot.SpotNumber;
                _context.SaveChanges();
                return ParkingAssignmentResult.Ok(new List<int> { spot.SpotNumber });
            } else if (VehicleSpotRequirement.IsBicycleType(type))
            {
                var spot = overview.FirstOrDefault(s =>
                    s.BicycleSlotsUsed < VehicleSpotRequirement.BicycleSlotsPerSpot &&
                    (s.OccupyingVehicleType == null || s.OccupyingVehicleType == VehicleType.Bicycle));

                if (spot == null)
                {
                    return ParkingAssignmentResult.Fail("No bicycle slot available.");
                }

                vehicle.AssignedSpotNumber = spot.SpotNumber;
                _context.SaveChanges();
                return ParkingAssignmentResult.Ok(new List<int> { spot.SpotNumber });
            }

            int required = VehicleSpotRequirement.GetRequiredWholeSpots(type);
            int? start = FindContiguousFreeStart(overview, required);

            if (start == null)
            {
                return ParkingAssignmentResult.Fail($"Not enough contiguous free spots for {type}.");
            }

            vehicle.AssignedSpotNumber = start;
            _context.SaveChanges();

            var assignedSpots = Enumerable.Range(start.Value, required).ToList();
            return ParkingAssignmentResult.Ok(assignedSpots);
        }

        public void ReleaseSpot(int vehicleId)
        {
            // Normally a no-op in practice: CheckOut removes the ParkedVehicle row entirely,
            // which already frees the spot(s) since GetSpotOverview reads live from the DB.
            // Kept for cases where a vehicle needs to be un-assigned without being removed.
            var vehicle = _context.ParkedVehicle.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.AssignedSpotNumber = null;
            _context.SaveChanges();
        }

        private static bool HasFreeMotorcycleSlot(IReadOnlyList<ParkingSpotInfo> overview)
        {
            return overview.Any(s =>
                s.MotorcycleSlotsUsed < VehicleSpotRequirement.MotorcycleSlotsPerSpot &&
                (s.OccupyingVehicleType == null || s.OccupyingVehicleType == VehicleType.Motorcycle));
        }

        private static bool HasFreeBicycleSlot(IReadOnlyList<ParkingSpotInfo> overview)
        {
            return overview.Any(s =>
                s.BicycleSlotsUsed < VehicleSpotRequirement.BicycleSlotsPerSpot &&
                (s.OccupyingVehicleType == null || s.OccupyingVehicleType == VehicleType.Bicycle));
        }

        private static int? FindContiguousFreeStart(IReadOnlyList<ParkingSpotInfo> overview, int required)
        {
            for (int start = 1; start <= overview.Count - required + 1; start++)
            {
                bool allFree = true;

                for (int i = 0; i < required; i++)
                {
                    var spot = overview[start - 1 + i];
                    if (!spot.IsFree || spot.MotorcycleSlotsUsed > 0 || spot.BicycleSlotsUsed > 0)
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