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
        private readonly GarageFeeService _garageFeeService;

        public ParkingSessionService(ApplicationDbContext context, IOptions<GarageSettings> options, GarageFeeService garageFeeService)
        {
            _context = context;
            _settings = options.Value;
            _garageFeeService = garageFeeService;
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
            var session = await _context.ParkingSessions
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.Owner)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.VehicleTypeRef)
                .Include(ps => ps.ParkingSpot)
                .FirstOrDefaultAsync(ps => ps.Id == sessionId);

            if (session is null || session.CheckOutTime != null || session.Vehicle is null || session.Vehicle.Owner is null)
            {
                return null;
            }

            var checkOutTime = DateTime.UtcNow;
            // Ensure arrivalTime is treated as UTC if passed as unspecified
            var utcArrival = session.ArriveTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(session.ArriveTime, DateTimeKind.Utc)
                : session.ArriveTime.ToUniversalTime();

            session.CheckOutTime = checkOutTime;

            var feeResult = _garageFeeService.CalculateDetailedFee(utcArrival, checkOutTime, session.HourlyRateAtCheckIn, session.Vehicle.Owner.IsProMember);
            session.AppliedDiscountPercentage = feeResult.DiscountPercentage;
            session.TotalPrice = feeResult.TotalPrice;

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