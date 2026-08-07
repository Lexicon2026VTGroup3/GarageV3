using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly ApplicationDbContext _context;

        public ParkingSessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingSession> StartSessionAsync(int parkingSpotNumber, int parkedVehicleId)
        {
            var session = new ParkingSession
            {
                ParkingSpotNumber = parkingSpotNumber,
                ParkedVehicleId = parkedVehicleId,
                Status = ParkingSessionStatus.Active,
                ArriveTime = DateTime.UtcNow
            };

            _context.ParkingSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<ParkingSession?> CompleteSessionAsync(int sessionId)
        {
            var session = await _context.ParkingSessions.FindAsync(sessionId);
            if (session is null || session.Status == ParkingSessionStatus.Completed)
                return null;

            session.Status = ParkingSessionStatus.Completed;
            session.CheckOutTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<ParkingSession?> GetActiveSessionForSpotAsync(int parkingSpotNumber)
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .Where(s => s.ParkingSpotNumber == parkingSpotNumber && s.Status == ParkingSessionStatus.Active)
                .FirstOrDefaultAsync();
        }
    }
}