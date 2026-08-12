using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Services;
using GarageV3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels;


public class ParkedVehiclesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleHandler _vehicleHandler;
    private readonly GarageFeeService _garageFeeService;
    private readonly IParkingSpotService _parkingSpotService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ParkedVehiclesController(
        ApplicationDbContext context,
        IVehicleHandler vehicleHandler,
        GarageFeeService garageFeeService,
        IParkingSpotService parkingSpotService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _vehicleHandler = vehicleHandler;
        _garageFeeService = garageFeeService;
        _parkingSpotService = parkingSpotService;
        _userManager = userManager;
    }

    // GET: PARKEDVEHICLES
    public async Task<IActionResult> Index(string searchString, string sortOrder, string searchTime)
    {
        var vehicleQuery = _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var searchStr = searchString.Trim().ToLower();

            vehicleQuery = vehicleQuery.Where(v =>
                (v.RegistrationNumber != null && v.RegistrationNumber.ToLower().Contains(searchStr)) ||
                v.VehicleTypeRef!.EnumValue.ToString().ToLower().Contains(searchStr) ||
                (v.AssignedSpotNumber != null && v.AssignedSpotNumber.ToString() == searchStr)
            );
        }

        switch (sortOrder)
        {
            case "RegAsc":
                vehicleQuery = vehicleQuery.OrderBy(v => v.RegistrationNumber);
                break;

            case "RegDesc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.RegistrationNumber);
                break;

            case "TypeAsc":
                vehicleQuery = vehicleQuery.OrderBy(v => v.VehicleTypeRef!.EnumValue.ToString());
                break;

            case "TypeDesc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.VehicleTypeRef!.EnumValue.ToString());
                break;

            case "SpotAsc":
                vehicleQuery = vehicleQuery.OrderBy(v => v.AssignedSpotNumber);
                break;

            case "SpotDesc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.AssignedSpotNumber);
                break;

            case "DateAsc":
                vehicleQuery = vehicleQuery.OrderBy(v => v.ArrivalTime);
                break;

            case "DateDesc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.ArrivalTime);
                break;

            case "DurationAsc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.ArrivalTime);
                break;

            case "DurationDesc":
                vehicleQuery = vehicleQuery.OrderBy(v => v.ArrivalTime);
                break;

            default:
                vehicleQuery = vehicleQuery.OrderBy(v => v.RegistrationNumber);
                break;

        }

        var vehicles = await vehicleQuery
            .Select(v => new ParkedVehicleOverviewViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleTypeRef!.EnumValue,
                RegistrationNumber = v.RegistrationNumber,
                ArrivalTime = v.ArrivalTime,
                AssignedSpotNumber = v.AssignedSpotNumber
            })
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(searchTime))
        {
            var searchTimeStr = searchTime.Trim().ToLower();

            bool isDate = DateTime.TryParse(searchTimeStr, out DateTime parsedDate);

            bool hasColon = searchTimeStr.Contains(":");

            bool isNumber = int.TryParse(searchTimeStr, out int number);

            var months = new[]
            {
                "january","february","march","april","may","june",
                "july","august","september","october","november","december"
            };
            bool isMonthName = months.Any(m => m.Contains(searchTimeStr));

            var weeks = new[]
            {
                "sunday", "monday","tuesday","wednesday","thursday","friday","saturday"
            };
            bool isWeekName = weeks.Any(w => w.Contains(searchTimeStr));

            vehicles = vehicles.Where(v =>
                FormatDuration(v.ArrivalTime).Contains(searchTimeStr) ||
                v.ArrivalTime.Hour.ToString().Contains(searchTimeStr) ||
                v.ArrivalTime.Minute.ToString().Contains(searchTimeStr) ||
                (isDate && hasColon && v.ArrivalTime.Hour == parsedDate.Hour && v.ArrivalTime.Minute == parsedDate.Minute) ||
                (isDate && !hasColon && v.ArrivalTime.Date == parsedDate.Date) ||
                (isNumber && (v.ArrivalTime.Year == number || v.ArrivalTime.Day == number || v.ArrivalTime.Month == number)) ||
                (isMonthName && months[v.ArrivalTime.Month - 1].Contains(searchTimeStr)) ||
                (isWeekName && weeks[(int)v.ArrivalTime.DayOfWeek].Contains(searchTimeStr))
            ).ToList();
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentTimeFilter"] = searchTime;

        ViewData["RegSortParm"] = (string.IsNullOrWhiteSpace(sortOrder) || sortOrder == "RegAsc") ? "RegDesc" : "RegAsc";
        ViewData["TypeSortParm"] = sortOrder == "TypeAsc" ? "TypeDesc" : "TypeAsc";
        ViewData["SpotSortParm"] = sortOrder == "SpotAsc" ? "SpotDesc" : "SpotAsc";

        ViewData["DateSortParm"] = sortOrder == "DateAsc" ? "DateDesc" : "DateAsc";
        ViewData["DurationSortParm"] = sortOrder == "DurationAsc" ? "DurationDesc" : "DurationAsc";
        ViewData["CurrentSort"] = sortOrder;

        ViewData["FreeSpotCount"] = _parkingSpotService.GetFreeSpotCount();
        ViewData["TotalSpots"] = _parkingSpotService.TotalSpots;

        return View(vehicles);
    }

    // GET: PARKEDVEHICLES/Overview
    public IActionResult Overview()
    {
        var viewModel = new ParkingOverviewViewModel
        {
            TotalSpots = _parkingSpotService.TotalSpots,
            FreeSpotCount = _parkingSpotService.GetFreeSpotCount(),
            Spots = _parkingSpotService.GetSpotOverview()
        };

        return View(viewModel);
    }

    // GET: PARKEDVEHICLES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parkedvehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (parkedvehicle == null)
        {
            return NotFound();
        }

        return View(parkedvehicle);
    }

    // GET: PARKEDVEHICLES/Create
    public IActionResult Create()
    {
        var viewModel = new ParkedVehicleFormViewModel();
        viewModel.VehicleTypes = BuildVehicleTypeSelectList();

        ViewBag.SpotMap = new ParkingOverviewViewModel
        {
            TotalSpots = _parkingSpotService.TotalSpots,
            FreeSpotCount = _parkingSpotService.GetFreeSpotCount(),
            Spots = _parkingSpotService.GetSpotOverview()
        };

        return View(viewModel);
    }

    // POST: PARKEDVEHICLES/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParkedVehicleFormViewModel viewModel)

    {
        // Normalize registration number (Trim + ToUpper)
        viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();

        bool regExists = await _vehicleHandler.IsExistingAsync(viewModel.RegistrationNumber);

        if (regExists)
        {
            ModelState.AddModelError("RegistrationNumber", "The registration number already exists. Please enter a different one.");
        }

        if (!_parkingSpotService.CanParkVehicleType(viewModel.VehicleType))
        {
            ModelState.AddModelError("VehicleType", "There is no available parking spot for this vehicle type right now.");
        }

        var vehicleTypeEntity = await GetVehicleTypeEntityAsync(viewModel.VehicleType);
        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleType", "Selected vehicle type is not recognized.");
        }

        var ownerId = _userManager.GetUserId(User);
        if (ownerId == null)
        {
            ModelState.AddModelError(string.Empty, "You must be logged in to check in a vehicle.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var parkedvehicle = new Vehicle
                {
                    VehicleTypeRefId = vehicleTypeEntity!.Id,
                    OwnerId = ownerId!,
                    RegistrationNumber = viewModel.RegistrationNumber,
                    Color = viewModel.Color ?? string.Empty,
                    Brand = viewModel.Brand ?? string.Empty,
                    Model = viewModel.Model ?? string.Empty,
                    NumberOfWheels = viewModel.NumberOfWheels.GetValueOrDefault(),
                    ArrivalTime = DateTime.Now
                };

                _context.Add(parkedvehicle);
                await _context.SaveChangesAsync();

                var assignmentResult = _parkingSpotService.AssignSpot(viewModel.VehicleType, parkedvehicle.Id);

                if (!assignmentResult.Success)
                {
                    _context.Remove(parkedvehicle);
                    await _context.SaveChangesAsync();

                    ModelState.AddModelError("VehicleType", assignmentResult.ErrorMessage ?? "Could not assign a parking spot.");
                }
                else
                {
                    TempData["SuccessMessage"] = $"Successfully checked in {viewModel.RegistrationNumber}.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not check in vehicle. Please check all fields.");
                Console.WriteLine("DB ERROR: " + ex.Message);
            }
        }

        viewModel.VehicleTypes = BuildVehicleTypeSelectList();

        ViewBag.SpotMap = new ParkingOverviewViewModel
        {
            TotalSpots = _parkingSpotService.TotalSpots,
            FreeSpotCount = _parkingSpotService.GetFreeSpotCount(),
            Spots = _parkingSpotService.GetSpotOverview()
        };

        return View(viewModel);
    }

    [HttpGet]
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> CheckDuplicate(string registrationNumber, int? id)
    {
        bool isDuplicate = await _vehicleHandler.IsExistingAsync(registrationNumber, id);
        return Json(!isDuplicate);
    }

    // GET: PARKEDVEHICLES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parkedvehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (parkedvehicle == null)
        {
            return NotFound();
        }

        var vm = new ParkedVehicleFormViewModel
        {
            Id = parkedvehicle.Id,
            RegistrationNumber = parkedvehicle.RegistrationNumber,
            VehicleType = parkedvehicle.VehicleTypeRef!.EnumValue,
            Color = parkedvehicle.Color,
            Brand = parkedvehicle.Brand,
            Model = parkedvehicle.Model,
            NumberOfWheels = parkedvehicle.NumberOfWheels,
            ArrivalTime = parkedvehicle.ArrivalTime,
            AssignedSpotNumber = parkedvehicle.AssignedSpotNumber,

            VehicleTypes = Enum.GetValues(typeof(VehicleType))
                .Cast<VehicleType>()
                .Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(),
                    Value = v.ToString()
                })
        };

        return View(vm);
    }

    // POST: PARKEDVEHICLES/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ParkedVehicleFormViewModel vm)
    {
        vm.RegistrationNumber = vm.RegistrationNumber.Trim().ToUpper();
        if (id != vm.Id)
        {
            return NotFound();
        }

        var original = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (original == null) { return NotFound(); }

        if (original.RegistrationNumber != vm.RegistrationNumber)
        {
            bool regExists = await _vehicleHandler.IsExistingAsync(vm.RegistrationNumber, id);

            if (regExists)
            {
                ModelState.AddModelError("RegistrationNumber", "The registration number already exists. Please enter a different one.");
            }
        }

        var vehicleTypeEntity = await GetVehicleTypeEntityAsync(vm.VehicleType);
        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleType", "Selected vehicle type is not recognized.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                original.VehicleTypeRefId = vehicleTypeEntity!.Id;
                original.RegistrationNumber = vm.RegistrationNumber;
                original.Color = vm.Color ?? string.Empty;
                original.Brand = vm.Brand ?? string.Empty;
                original.Model = vm.Model ?? string.Empty;
                original.NumberOfWheels = vm.NumberOfWheels.GetValueOrDefault();

                _context.Update(original);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully saved changes to {vm.RegistrationNumber}.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not save changes. Please check all fields.");
                Console.WriteLine("DB ERROR: " + ex.Message);
            }
        }

        vm.VehicleTypes = Enum.GetValues(typeof(VehicleType))
            .Cast<VehicleType>()
            .Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = v.ToString()
            });

        return View(vm);
    }

    // GET: PARKEDVEHICLES/CheckOut/5
    public async Task<IActionResult> CheckOut(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parkedvehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (parkedvehicle == null)
        {
            return NotFound();
        }

        return View(parkedvehicle);
    }

    // POST: PARKEDVEHICLES/CheckOut/5
    [HttpPost, ActionName("CheckOut")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOutConfirmed(int? id)
    {
        var parkedvehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (parkedvehicle == null)
        {
            return NotFound();
        }

        DateTime checkOutTime = DateTime.Now;

        var receiptViewModel = new ReceiptViewModel
        {
            VehicleType = parkedvehicle.VehicleTypeRef!.EnumValue,
            RegistrationNumber = parkedvehicle.RegistrationNumber,
            Brand = parkedvehicle.Brand,
            Model = parkedvehicle.Model,
            Color = parkedvehicle.Color,
            NumberOfWheels = parkedvehicle.NumberOfWheels,
            AssignedSpotNumber = parkedvehicle.AssignedSpotNumber,
            ArrivalTime = parkedvehicle.ArrivalTime,
            CheckOutTime = checkOutTime,

            TotalPrice = _garageFeeService.CalculateFee(
                parkedvehicle.ArrivalTime,
                checkOutTime)
        };

        _context.Vehicles.Remove(parkedvehicle);

        await _context.SaveChangesAsync();

        TempData["Receipt"] = JsonSerializer.Serialize(receiptViewModel);

        TempData["SuccessMessage"] = $"Successfully checked out {receiptViewModel.RegistrationNumber}.";

        return RedirectToAction(nameof(Receipt));
    }

    // GET: PARKEDVEHICLES/Receipt
    public IActionResult Receipt()
    {
        if (TempData["Receipt"] is not string json)
        {
            return RedirectToAction(nameof(Index));
        }

        var receipt = JsonSerializer.Deserialize<ReceiptViewModel>(json);

        return View(receipt);
    }

    private string FormatDuration(DateTime arrival)
    {
        var span = DateTime.Now - arrival;
        int days = (int)span.TotalDays;
        int hours = span.Hours;
        int minutes = span.Minutes;

        return $"{days}d {hours}h {minutes}m , {days} d {hours} h {minutes} m";
    }

    private IEnumerable<SelectListItem> BuildVehicleTypeSelectList()
    {
        var availability = _parkingSpotService.GetVehicleTypeAvailability();

        return Enum.GetValues(typeof(VehicleType))
            .Cast<VehicleType>()
            .Select(v => new SelectListItem
            {
                Text = availability.TryGetValue(v, out var canPark) && canPark
                    ? v.GetDisplayName()
                    : $"{v.GetDisplayName()} (Full)",
                Value = ((int)v).ToString(),
                Disabled = !(availability.TryGetValue(v, out var available) && available)
            });
    }

    private async Task<VehicleTypeEntity?> GetVehicleTypeEntityAsync(VehicleType type)
    {
        return await _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.EnumValue == type);
    }
}