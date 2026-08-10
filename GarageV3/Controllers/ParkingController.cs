using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Services;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels.Parking;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GarageV3.Controllers
{
    public class ParkingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IParkingSessionService _parkingSessionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParkingController(
          ApplicationDbContext context,
          IParkingSessionService parkingSessionService,
          UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _parkingSessionService = parkingSessionService;
            _userManager = userManager;

        }
        // GET: PARKEDVEHICLES/CheckOut/5
        public async Task<IActionResult> CheckOut(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.ParkingSessions
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.Owner)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.VehicleTypeRef)
                .Include(ps => ps.ParkingSpot)
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (session == null || session.Vehicle == null)
            {
                return NotFound();
            }

            var viewModel = new CheckOutViewModel
            {
                ParkingSessionId = session.Id,
                OwnerEmail = ((session.Vehicle.Owner is null) || (session.Vehicle.Owner.Email is null)) ?
                    "No Owner" :
                    session.Vehicle.Owner.Email,
                RegistrationNumber = session.Vehicle.RegistrationNumber,
                Brand = session.Vehicle.Brand,
                Model = session.Vehicle.Model,
                Color = session.Vehicle.Color,
                NumberOfWheels = session.Vehicle.NumberOfWheels,
                VehicleType = session.Vehicle.VehicleTypeRef,
                VehicleTypeName = session.Vehicle.VehicleTypeRef?.EnumValue.GetDisplayName() ?? "Unknown",
                ParkingSpotId = session.ParkingSpot?.Id ?? -1,
                CheckInTime = session.ArriveTime,
                HourlyRateAtCheckIn = session.HourlyRateAtCheckIn
            };

            return View(viewModel);
        }

        // POST: PARKEDVEHICLES/CheckOut/5
        [HttpPost, ActionName("CheckOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int? parkingSessionId, CheckOutViewModel vm)
        {
            if (parkingSessionId == null)
            {
                TempData["ErrorMessage"] = "Invalid session ID.";
                return RedirectToAction("Index");
            }
            Console.WriteLine($"CheckOutConfirmed called with id = {parkingSessionId}");
            var session = await _parkingSessionService.CompleteSessionAsync(parkingSessionId.Value);

            if (session == null || session.Vehicle == null)
            {
                TempData["ErrorMessage"] = "Unable to checkout. The parking session was not found or is already checked out.";
                return RedirectToAction("CheckOut", new { id = parkingSessionId });
            }

            var receiptViewModel = new ReceiptViewModel
            {
                VehicleType = session.Vehicle.VehicleTypeRef?.EnumValue ?? default,
                RegistrationNumber = session.Vehicle.RegistrationNumber,
                Brand = session.Vehicle.Brand,
                Model = session.Vehicle.Model,
                Color = session.Vehicle.Color,
                NumberOfWheels = session.Vehicle.NumberOfWheels,
                AssignedSpotNumber = session.ParkingSpot?.Number ?? -1,
                ArrivalTime = session.ArriveTime,
                CheckOutTime = session.CheckOutTime ?? DateTime.UtcNow,
                TotalPrice = session.TotalPrice ?? 0
            };

            TempData["Receipt"] = JsonSerializer.Serialize(receiptViewModel);

            TempData["SuccessMessage"] = $"Successfully checked out {receiptViewModel.RegistrationNumber}.";

            return RedirectToAction("Receipt", new { id = session.Id });
        }
    }
}
