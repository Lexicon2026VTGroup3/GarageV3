using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels.Parking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GarageV3.Models.Parking;
namespace GarageV3.Controllers
{
    [Authorize]
    public class ParkingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IParkingSessionService _parkingSessionService;
        private readonly IParkingAllocationService _parkingAllocationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GarageSettings _settings;

        public ParkingController(
            ApplicationDbContext context,
            IParkingSessionService parkingSessionService,
            IParkingAllocationService parkingAllocationService,
            UserManager<ApplicationUser> userManager,
            IOptions<GarageSettings> settings)
        {
            _context = context;
            _parkingSessionService = parkingSessionService;
            _parkingAllocationService = parkingAllocationService;
            _userManager = userManager;
            _settings = settings.Value;
        }

        // GET: /Parking
        public IActionResult Index()
        {
            return RedirectToAction("Index", "MyVehicles");
        }

        // GET: Parking/SpotMap
        public async Task<IActionResult> SpotMap()
        {
            var spots = await _context.ParkingSpots
                .OrderBy(s => s.Number)
                .AsNoTracking()
                .ToListAsync();

            var activeAllocations = await _context.ParkingAllocations
                .Where(a => a.ParkingSession!.CheckOutTime == null)
                .Include(a => a.ParkingSession)
                    .ThenInclude(s => s!.Vehicle)
                .AsNoTracking()
                .ToListAsync();

            var usedUnitsBySpot = activeAllocations
                .GroupBy(a => a.ParkingSpotId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.UnitsUsed));

            var regNumsBySpot = activeAllocations
                .GroupBy(a => a.ParkingSpotId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => a.ParkingSession!.Vehicle?.RegistrationNumber ?? string.Empty)
                          .Where(r => r != string.Empty)
                          .Distinct()
                          .ToList());

            var spotIdToPosition = new Dictionary<int, string>();
            var multiSpotSessions = activeAllocations
                .GroupBy(a => a.ParkingSessionId)
                .Where(g => g.Count() > 1);

            foreach (var group in multiSpotSessions)
            {
                var orderedSpotIds = group
                    .Select(a => a.ParkingSpotId)
                    .Distinct()
                    .Select(id => spots.First(s => s.Id == id))
                    .OrderBy(s => s.Number)
                    .Select(s => s.Id)
                    .ToList();

                for (int i = 0; i < orderedSpotIds.Count; i++)
                {
                    spotIdToPosition[orderedSpotIds[i]] =
                        i == 0 ? "Left" :
                        i == orderedSpotIds.Count - 1 ? "Right" :
                        "Middle";
                }
            }

            var spotInfos = spots.Select(s => new SpotMapSpotInfo
            {
                SpotNumber = s.Number,
                Location = s.Location,
                IsOutOfService = s.IsOutOfService,
                CapacityUnits = s.CapacityUnits,
                UsedUnits = usedUnitsBySpot.GetValueOrDefault(s.Id, 0),
                OccupyingRegistrationNumbers = regNumsBySpot.GetValueOrDefault(s.Id, new List<string>()),
                Position = spotIdToPosition.GetValueOrDefault(s.Id, "Whole")
            }).ToList();

            var viewModel = new SpotMapViewModel
            {
                TotalSpots = spots.Count,
                FreeSpots = spotInfos.Count(s => !s.IsOutOfService && s.FreeUnits > 0),
                Spots = spotInfos
            };

            return View(viewModel);
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
                    VehicleTypeName = (ps.Vehicle != null && ps.Vehicle.VehicleTypeRef != null) ? ps.Vehicle.VehicleTypeRef.Name : "Unknown",
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
        public async Task<IActionResult> Park(int? id)
        {
            var userId = _userManager.GetUserId(User);

            var viewModel = new ParkVehicleViewModel
            {
                VehicleId = id ?? 0,
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

            var currentUser = await _userManager.FindByIdAsync(userId!);
            if (currentUser == null || !IsAtLeast18(currentUser.PersonalIdentityNumber))
            {
                ModelState.AddModelError(string.Empty, "Vehicle owner must be at least 18 years old to park.");
            }

            if (viewModel.ParkingSpotId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a parking spot.");
            }

            if (ModelState.IsValid)
            {
                var result = await _parkingAllocationService.AllocateAndStartSessionAsync(
                    viewModel.VehicleId, viewModel.ParkingSpotId, _settings.HourlyRate);

                if (result.Success)
                {
                    var spotNumbers = result.Allocations
                        .Select(a => a.ParkingSpotId)
                        .Distinct()
                        .ToList();

                    TempData["SuccessMessage"] = spotNumbers.Count > 1
                        ? "Vehicle parked successfully across multiple spots."
                        : "Vehicle parked successfully.";

                    return RedirectToAction("Index", "MyVehicles");
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not park the vehicle.");
            }

            viewModel.Vehicles = await BuildOwnedUnparkedVehiclesSelectListAsync(userId!);
            viewModel.ParkingSpots = await BuildParkingSpotsSelectListAsync();

            return View(viewModel);
        }

        // GET: Parking/CheckOut/5
        public async Task<IActionResult> CheckOut(int id)
        {
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

            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = session.Vehicle.Owner?.Id == currentUserId;

            if (!isAdmin && !isOwner)
            {
                return Forbid();
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
                VehicleTypeName = session.Vehicle.VehicleTypeRef?.Name ?? "Unknown",
                ParkingSpotId = session.ParkingSpot?.Id ?? -1,
                CheckInTime = session.ArriveTime,
                HourlyRateAtCheckIn = session.HourlyRateAtCheckIn
            };

            return View(viewModel);
        }

        // POST: Parking/CheckOut/5
        [HttpPost, ActionName("CheckOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOutConfirmed(int id)
        {
            var session = await _parkingSessionService.CompleteSessionAsync(id);

            if (session == null || session.Vehicle == null)
            {
                TempData["ErrorMessage"] = "Unable to checkout. The parking session was not found or is already checked out.";
                return RedirectToAction("Index", "MyVehicles");
            }

            await _parkingAllocationService.ReleaseAllocationsAsync(session.Id);

            var receiptViewModel = new ReceiptViewModel
            {
                OwnerEmail = session.Vehicle.Owner?.Email ?? "No Owner",
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
        public IActionResult Receipt(int? id)
        {
            if (TempData["Receipt"] is not string json)
            {
                return RedirectToAction("Index", "MyVehicles");
            }

            var receipt = JsonSerializer.Deserialize<ReceiptViewModel>(json);

            return View(receipt);
        }

        // GET: Parking/PrintReceipt
        [HttpGet]
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var session = await _context.ParkingSessions
                .Where(ps => ps.CheckOutTime != null && ps.Vehicle != null)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v!.Owner)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v!.VehicleTypeRef)
                .Include(ps => ps.ParkingSpot)
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (session == null || session.Vehicle == null)
            {
                TempData["ErrorMessage"] = "Unable to print receipt. The parking session was not found or is not checked out yet.";
                return RedirectToAction(nameof(History));
            }
            if (session.Vehicle.Owner == null || session.Vehicle.Owner.Id != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not allowed to print others' receipt.";
                return RedirectToAction(nameof(History));
            }

            var receiptViewModel = new ReceiptViewModel
            {
                OwnerEmail = session.Vehicle.Owner?.Email ?? "No Owner",
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
                TotalPrice = session.TotalPrice ?? 0,
                AppliedDiscountPercentage = session.AppliedDiscountPercentage
            };

            TempData["Receipt"] = JsonSerializer.Serialize(receiptViewModel);

            return RedirectToAction(nameof(Receipt), new { id });
        }

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

        private async Task<IEnumerable<SelectListItem>> BuildParkingSpotsSelectListAsync()
        {
            var spots = await _context.ParkingSpots
                .Where(s => !s.IsOutOfService)
                .OrderBy(s => s.Number)
                .ToListAsync();

            var usedUnitsBySpot = await _context.ParkingAllocations
                .Where(a => a.ParkingSession!.CheckOutTime == null)
                .GroupBy(a => a.ParkingSpotId)
                .Select(g => new { SpotId = g.Key, Used = g.Sum(a => a.UnitsUsed) })
                .ToDictionaryAsync(x => x.SpotId, x => x.Used);

            return spots.Select(s =>
            {
                int used = usedUnitsBySpot.GetValueOrDefault(s.Id, 0);
                int free = s.CapacityUnits - used;
                var label = s.Location != null ? $"#{s.Number} ({s.Location})" : $"#{s.Number}";

                string statusText = free <= 0
                    ? "Occupied"
                    : used > 0
                        ? $"{free}/{s.CapacityUnits} free"
                        : "Free";

                return new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{label} ({statusText})",
                    Disabled = free <= 0
                };
            });
        }

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