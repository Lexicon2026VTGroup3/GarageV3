using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels.Parking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GarageV3.Controllers
{
    [Authorize]
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

        // GET: /Parking
        public IActionResult Index()
        {
            return RedirectToAction("Index", "MyVehicles");
        }

        // GET: Parking/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var historySessions = await _context.ParkingSessions
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.VehicleTypeRef)
                .Include(ps => ps.ParkingSpot)
                .Where(ps => ps.CheckOutTime != null && ps.Vehicle != null && ps.Vehicle.Owner != null && ps.Vehicle.Owner.Id == currentUser.Id)
                .OrderByDescending(ps => ps.CheckOutTime)
                .Select(ps => new ParkingHistoryViewModel
                {
                    SessionId = ps.Id,
                    RegistrationNumber = (ps.Vehicle != null && ps.Vehicle.RegistrationNumber != null) ? ps.Vehicle.RegistrationNumber : string.Empty,
                    VehicleTypeName = (ps.Vehicle != null && ps.Vehicle.VehicleTypeRef != null) ? ps.Vehicle.VehicleTypeRef.EnumValue.ToString() : "Unknown",
                    ParkingSpotId = (ps.ParkingSpot != null) ? ps.ParkingSpot.Id : -1,
                    ArrivalTime = ps.ArriveTime,
                    CheckOutTime = (ps.CheckOutTime != null) ? ps.CheckOutTime.Value : DateTime.MinValue,
                    HourlyRateAtCheckIn = ps.HourlyRateAtCheckIn,
                    TotalPrice = ps.TotalPrice ?? 0
                })
                .AsNoTracking()
                .ToListAsync();

            return View(historySessions);
        }


        // GET: Parking/Park
        public async Task<IActionResult> Park()
        {
            var userId = _userManager.GetUserId(User);

            var viewModel = new ParkVehicleViewModel
            {
                Vehicles = await BuildOwnedUnparkedVehiclesSelectListAsync(userId!),
                ParkingSpots = await BuildParkingSpotsSelectListAsync()
            };

            return View(viewModel);
        }

        // POST: Parking/Park
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Park(ParkVehicleViewModel viewModel)
        {
            var userId = _userManager.GetUserId(User);

            // TASK-06.5: server-side ownership + availability re-check
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == viewModel.VehicleId && v.OwnerId == userId);

            if (vehicle == null)
            {
                ModelState.AddModelError(string.Empty, "Selected vehicle was not found or does not belong to you.");
            }
            else
            {
                bool alreadyActive = await _context.ParkingSessions
                    .AnyAsync(s => s.VehicleId == vehicle.Id && s.CheckOutTime == null);

                if (alreadyActive)
                {
                    ModelState.AddModelError(string.Empty, "This vehicle already has an active parking session.");
                }
            }

            // TASK-06.6: server-side 18+ check, based on the owner's PersonalIdentityNumber
            var currentUser = await _userManager.FindByIdAsync(userId!);
            if (currentUser == null || !IsAtLeast18(currentUser.PersonalIdentityNumber))
            {
                ModelState.AddModelError(string.Empty, "Vehicle owner must be at least 18 years old to park.");
            }

            var spot = await _context.ParkingSpots
                .FirstOrDefaultAsync(s => s.Id == viewModel.ParkingSpotId);

            if (spot == null || spot.IsOutOfService)
            {
                ModelState.AddModelError(string.Empty, "Selected parking spot is not available.");
            }
            else
            {
                bool spotOccupied = await _context.ParkingSessions
                    .AnyAsync(s => s.ParkingSpotId == spot.Id && s.CheckOutTime == null);

                if (spotOccupied)
                {
                    ModelState.AddModelError(string.Empty, "Selected parking spot is already occupied.");
                }
            }

            if (ModelState.IsValid)
            {
                await _parkingSessionService.StartSessionAsync(viewModel.ParkingSpotId, viewModel.VehicleId);

                TempData["SuccessMessage"] = "Vehicle parked successfully.";
                return RedirectToAction("Index", "MyVehicles");
            }

            viewModel.Vehicles = await BuildOwnedUnparkedVehiclesSelectListAsync(userId!);
            viewModel.ParkingSpots = await BuildParkingSpotsSelectListAsync();

            return View(viewModel);
        }

        // GET: Parking/CheckOut/5
        public async Task<IActionResult> CheckOut(int id)
        {
            if (id == null) return NotFound();

                return NotFound();
            }

                return NotFound();
            }

                return NotFound();
            }

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

            if (session == null || session.Vehicle == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = session.Vehicle.Owner?.Id == currentUserId;

            if (!isAdmin && !isOwner)
            {
                return Forbid(); // Returns HTTP 403 Forbidden / Access Denied
            }

            var viewModel = new CheckOutViewModel
            {
                ParkingSessionId = session.Id,
                OwnerEmail = session.Vehicle.Owner?.Email ?? "No Owner",
                RegistrationNumber = session.Vehicle.RegistrationNumber,
                Brand = session.Vehicle.Brand,
                Model = session.Vehicle.Model,
                Color = session.Vehicle.Color,
                NumberOfWheels = session.Vehicle.NumberOfWheels,
                VehicleTypeName = session.Vehicle.VehicleTypeRef?.Name ?? "Unknown",
                ParkingSpotId = session.ParkingSpot?.Id ?? -1,
                CheckInTime = session.ArriveTime,
                HourlyRateAtCheckIn = session.HourlyRateAtCheckIn
            };

            return View(viewModel);
        }

        // POST: Parking/CheckOut/5
        [HttpPost, ActionName("CheckOut")]
            if (parkingSessionId == null)
            {
                TempData["ErrorMessage"] = "Invalid session ID.";
                return RedirectToAction("Index");
            }

            var session = await _parkingSessionService.CompleteSessionAsync(parkingSessionId.Value);
            var session = await _parkingSessionService.CompleteSessionAsync(id);
            }

            var session = await _parkingSessionService.CompleteSessionAsync(parkingSessionId.Value);
                TempData["ErrorMessage"] = "Unable to checkout. The parking session was not found.";
                return RedirectToAction("CheckOut", new { id = parkingSessionId });
                TempData["ErrorMessage"] = "Unable to checkout. The parking session was not found or is already checked out.";
                return RedirectToAction("Index", "MyVehicles");
            {
                TempData["ErrorMessage"] = "Unable to checkout. The parking session was not found.";
                return RedirectToAction("CheckOut", new { id = parkingSessionId });
            }
                VehicleTypeName = session.Vehicle.VehicleTypeRef?.Name ?? "Unknown",
                OwnerEmail = session.Vehicle.Owner?.Email ?? "No Owner",
                VehicleType = session.Vehicle.VehicleTypeRef?.EnumValue ?? default,
            var receiptViewModel = new ReceiptViewModel
            {
                VehicleTypeName = session.Vehicle.VehicleTypeRef?.Name ?? "Unknown",
                RegistrationNumber = session.Vehicle.RegistrationNumber,
                Brand = session.Vehicle.Brand,
                Model = session.Vehicle.Model,
                Color = session.Vehicle.Color,
                NumberOfWheels = session.Vehicle.NumberOfWheels,
                ParkingSpotId = session.ParkingSpot?.Id ?? -1,
                ArrivalTime = session.ArriveTime,
                CheckOutTime = session.CheckOutTime ?? DateTime.UtcNow,
                HourlyRateAtCheckIn = session.HourlyRateAtCheckIn,
                TotalPrice = session.TotalPrice ?? 0
            };

            TempData["Receipt"] = JsonSerializer.Serialize(receiptViewModel);
            TempData["SuccessMessage"] = $"Successfully checked out {receiptViewModel.RegistrationNumber}.";

            return RedirectToAction(nameof(Receipt), new { id = session.Id });
        }

        // GET: Parking/Receipt
        public IActionResult Receipt()
        {
            if (TempData["Receipt"] is not string json)
            {
                return RedirectToAction("Index", "MyVehicles");
            }

            var receipt = JsonSerializer.Deserialize<ReceiptViewModel>(json);

            return View(receipt);
        }

        // TASK-06.2: only the logged-in member's own, currently unparked vehicles
        private async Task<IEnumerable<SelectListItem>> BuildOwnedUnparkedVehiclesSelectListAsync(string userId)
        {
            var activeVehicleIds = _context.ParkingSessions
                .Where(s => s.CheckOutTime == null)
                .Select(s => s.VehicleId);

            return await _context.Vehicles
                .Where(v => v.OwnerId == userId && !activeVehicleIds.Contains(v.Id))
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.RegistrationNumber
                })
                .ToListAsync();
        }

        // TASK-06.3: shows every spot not out of service, greying out (disabling) occupied ones
        // instead of hiding them, matching the pattern already used for VehicleType availability.
        private async Task<IEnumerable<SelectListItem>> BuildParkingSpotsSelectListAsync()
        {
            var occupiedSpotIds = await _context.ParkingSessions
                .Where(s => s.CheckOutTime == null)
                .Select(s => s.ParkingSpotId)
                .ToListAsync();

            var spots = await _context.ParkingSpots
                .Where(s => !s.IsOutOfService)
                .OrderBy(s => s.Number)
                .ToListAsync();

            return spots.Select(s =>
            {
                bool isOccupied = occupiedSpotIds.Contains(s.Id);
                var label = s.Location != null ? $"#{s.Number} ({s.Location})" : $"#{s.Number}";

                return new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = isOccupied ? $"{label} (Occupied)" : label,
                    Disabled = isOccupied
                };
            });
        }

        // TASK-06.6: PersonalIdentityNumber format is YYYYMMDD-XXXX
        private static bool IsAtLeast18(string? personalIdentityNumber)
        {
            if (string.IsNullOrWhiteSpace(personalIdentityNumber) || personalIdentityNumber.Length < 8)
                return false;

            var datePart = personalIdentityNumber[..8];

            if (!DateTime.TryParseExact(
                    datePart,
                    "yyyyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var birthDate))
            {
                return false;
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age >= 18;
        }
    }
}