using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GarageStatisticsViewModel> GetGarageStatisticsAsync()
        {
            // Query 1: all parking spots.
            List<ParkingSpot> allSpots = await _context.ParkingSpots
                .AsNoTracking()
                .ToListAsync();

            // Query 2: all currently active sessions, with vehicle + vehicle type
            // loaded up front so no further DB calls are needed per spot/vehicle.
            List<ParkingSession> activeSessions = await _context.ParkingSessions
                .AsNoTracking()
                .Where(s => s.CheckOutTime == null)
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v!.VehicleTypeRef)
                .ToListAsync();

            // Join in memory via ParkingSpotId - no extra queries per spot.
            HashSet<int> occupiedSpotIds = activeSessions
                .Select(s => s.ParkingSpotId)
                .ToHashSet();

            int outOfService = allSpots.Count(s => s.IsOutOfService);
            int occupied = allSpots.Count(s => !s.IsOutOfService && occupiedSpotIds.Contains(s.Id));
            int free = allSpots.Count(s => !s.IsOutOfService && !occupiedSpotIds.Contains(s.Id));

            List<VehicleTypeCount> activeByType = activeSessions
                .Where(s => s.Vehicle?.VehicleTypeRef != null)
                .GroupBy(s => s.Vehicle!.VehicleTypeRef!.Name)
                .Select(g => new VehicleTypeCount
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(v => v.Count)
                .ToList();

            return new GarageStatisticsViewModel
            {
                FreeSpots = free,
                OccupiedSpots = occupied,
                OutOfServiceSpots = outOfService,
                ActiveVehiclesByType = activeByType,

                TopUsers = await GetTopLucrativeUsersAsync()
            };
        }

        public async Task<List<TopUserViewModel>> GetTopLucrativeUsersAsync()
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v!.Owner)
                .Where(s => s.CheckOutTime != null && s.Vehicle != null && s.Vehicle.Owner != null)
                .GroupBy(s => new { s.Vehicle!.OwnerId, s.Vehicle.Owner!.FirstName, s.Vehicle.Owner.LastName, s.Vehicle.Owner.Email })
                .Select(g => new TopUserViewModel
                {
                    Id = g.Key.OwnerId,
                    FullName = !string.IsNullOrEmpty(g.Key.FirstName) ? $"{g.Key.FirstName} {g.Key.LastName}" : g.Key.Email ?? "Unknown",
                    TotalRevenue = g.Sum(s => (decimal?)(s.TotalPrice)) ?? 0m
                })
                .OrderByDescending(u => u.TotalRevenue)
                .Take(5)
                .ToListAsync();
        }
    }
}