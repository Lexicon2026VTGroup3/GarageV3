using GarageV3.Data;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminParkingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminParkingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? registrationSearch,
            string? vehicleType)
        {
            var query = _context.ParkingSessions
                .Where(ps => ps.CheckOutTime == null);

            if (!string.IsNullOrWhiteSpace(registrationSearch))
            {
                registrationSearch = registrationSearch.Trim();

                query = query.Where(ps =>
                    ps.Vehicle != null &&
                    ps.Vehicle.RegistrationNumber.Contains(registrationSearch));
            }

            if (!string.IsNullOrWhiteSpace(vehicleType))
            {
                vehicleType = vehicleType.Trim();

                query = query.Where(ps =>
                    ps.Vehicle != null &&
                    ps.Vehicle.VehicleTypeRef != null &&
                    ps.Vehicle.VehicleTypeRef.Name == vehicleType);
            }

            var activeParkings = await query
                .Select(ps => new ActiveParkingOverviewViewModel
                {
                    ParkingSessionId = ps.Id,

                    OwnerName = ps.Vehicle != null && ps.Vehicle.Owner != null
                        ? ps.Vehicle.Owner.FullName
                        : "Unknown",

                    VehicleTypeName = ps.Vehicle != null && ps.Vehicle.VehicleTypeRef != null
                        ? ps.Vehicle.VehicleTypeRef.Name
                        : "Unknown",

                    RegistrationNumber = ps.Vehicle != null
                        ? ps.Vehicle.RegistrationNumber
                        : string.Empty,

                    ParkingSpotNumber = ps.ParkingSpot != null
                        ? ps.ParkingSpot.Number
                        : 0,

                    Location = ps.ParkingSpot != null
                        ? ps.ParkingSpot.Location ?? string.Empty
                        : string.Empty,

                    CheckInTime = ps.ArriveTime,

                    ParkingDurationMinutes =
                        (int)(DateTime.UtcNow - ps.ArriveTime).TotalMinutes
                })
                .AsNoTracking()
                .ToListAsync();

            ViewData["VehicleTypes"] =
                Enum.GetNames<GarageV3.Models.Enums.VehicleType>();

            ViewData["CurrentRegistrationSearch"] = registrationSearch;
            ViewData["CurrentVehicleType"] = vehicleType;

            return View(activeParkings);
        }
    }
}