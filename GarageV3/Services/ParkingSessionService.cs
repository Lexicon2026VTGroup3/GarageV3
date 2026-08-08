using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Parking;
using GarageV3.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GarageV3.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly GarageSettings _settings;

        public ParkingSessionService(ApplicationDbContext context, IOptions<GarageSettings> options)
        {
            _context = context;
            _settings = options.Value;
        }

        public async Task<ParkingSession> StartSessionAsync(int parkingSpotId, int vehicleId)
        {
            var session = new ParkingSession
            {
                ParkingSpotId = parkingSpotId,
                VehicleId = vehicleId,
                ArriveTime = DateTime.UtcNow,
                HourlyRateAtCheckIn = _settings.HourlyRate
            };

            _context.ParkingSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<ParkingSession?> CompleteSessionAsync(int sessionId)
        {
            var session = await _context.ParkingSessions.FindAsync(sessionId);
            if (session is null || session.CheckOutTime != null)
                return null;

            var checkOutTime = DateTime.UtcNow;
            var hours = (decimal)(checkOutTime - session.ArriveTime).TotalHours;

            session.CheckOutTime = checkOutTime;
            session.TotalPrice = Math.Round(hours * session.HourlyRateAtCheckIn, 2);

            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<ParkingSession?> GetActiveSessionForSpotAsync(int parkingSpotId)
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .Where(s => s.ParkingSpotId == parkingSpotId && s.CheckOutTime == null)
                .FirstOrDefaultAsync();
        }
    }
}