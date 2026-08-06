using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Models.ViewModels;
using GarageV3.Services;
using GarageV3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


public class ParkedVehiclesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleHandler _vehicleHandler;
    private readonly GarageFeeService _garageFeeService;
    private readonly IParkingSpotService _parkingSpotService;

    public ParkedVehiclesController(
        ApplicationDbContext context,
        IVehicleHandler vehicleHandler,
        GarageFeeService garageFeeService,
        IParkingSpotService parkingSpotService)
    {
        _context = context;
        _vehicleHandler = vehicleHandler;
        _garageFeeService = garageFeeService;
        _parkingSpotService = parkingSpotService;
    }

    // GET: PARKEDVEHICLES
    public async Task<IActionResult> Index(string searchString, string sortOrder, string searchTime)
    {
        var vehicleQuery = _context.ParkedVehicle.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString= searchString.Trim().ToLower();

            vehicleQuery = vehicleQuery.Where(
                v => v.RegistrationNumber.ToLower().Contains(searchString) || 
                v.VehicleType.ToString().ToLower().Contains(searchString) ||
                v.AssignedSpotNumber.ToString().Contains(searchString)
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
                vehicleQuery = vehicleQuery.OrderBy(v => v.VehicleType.ToString());
                break;

            case "TypeDesc":
                vehicleQuery = vehicleQuery.OrderByDescending(v => v.VehicleType.ToString());
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
                VehicleType = v.VehicleType,
                RegistrationNumber = v.RegistrationNumber,
                ArrivalTime = v.ArrivalTime,
                AssignedSpotNumber = v.AssignedSpotNumber
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(searchTime)) 
        {
            searchTime = searchTime.Trim().ToLower();

            bool isDate = DateTime.TryParse(searchTime, out DateTime parsedDate);

            bool hasColon = searchTime.Contains(":");

            bool isNumber = int.TryParse(searchTime, out int number);

            var months = new[]
            {
                "january","february","march","april","may","june",
                "july","august","september","october","november","december"
            };
            bool isMonthName = months.Any(m => m.Contains(searchTime));

            var weeks = new[]
            {
                "sunday", "monday","tuesday","wednesday","thursday","friday","saturday"
            };
            bool isWeekName = weeks.Any(w => w.Contains(searchTime));

            bool isDouble = double.TryParse(searchTime, out double doubleNumber);

            vehicles = vehicles.Where(v =>
                FormatDuration(v.ArrivalTime).Contains(searchTime) ||
                v.ArrivalTime.Hour.ToString().Contains(searchTime) ||
                v.ArrivalTime.Minute.ToString().Contains(searchTime) ||
                (isDate && hasColon && v.ArrivalTime.Hour == parsedDate.Hour && v.ArrivalTime.Minute == parsedDate.Minute) ||
                (isDate && !hasColon && v.ArrivalTime.Date == parsedDate.Date) ||
                (isNumber && (v.ArrivalTime.Year == number || v.ArrivalTime.Day == number || v.ArrivalTime.Month == number)) ||
                (isMonthName && months[v.ArrivalTime.Month - 1].Contains(searchTime)) ||
                (isWeekName && weeks[(int)v.ArrivalTime.DayOfWeek].Contains(searchTime))
            ).ToList();
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentTimeFilter"] = searchTime;

        ViewData["RegSortParm"] = (string.IsNullOrEmpty(sortOrder) || sortOrder == "RegAsc") ? "RegDesc" : "RegAsc";
        ViewData["TypeSortParm"] = sortOrder == "TypeAsc" ? "TypeDesc" : "TypeAsc";
        ViewData["SpotSortParm"] = sortOrder == "SpotAsc" ? "SpotDesc" : "SpotAsc";

        ViewData["DateSortParm"] = sortOrder == "DateAsc" ? "DateDesc" : "DateAsc";
        ViewData["DurationSortParm"] = sortOrder == "DurationAsc" ? "DurationDesc" : "DurationAsc";
        ViewData["CurrentSort"] = sortOrder;

        // Del 2: free spot count for the landing page
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
    // GET: PARKEDVEHICLES/Statistics
    public async Task<IActionResult> Statistics()
    {
        var vehicles = await _context.ParkedVehicle.AsNoTracking().ToListAsync();

        var now = DateTime.Now;

        var viewModel = new GarageStatisticsViewModel
        {
            TotalVehicles = vehicles.Count,
            TotalWheels = vehicles.Sum(v => v.NumberOfWheels),
            EstimatedCurrentRevenue = vehicles.Sum(v => _garageFeeService.CalculateFee(v.ArrivalTime, now)),
            VehicleCountsByType = vehicles
                .GroupBy(v => v.VehicleType)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageParkedDuration = vehicles.Any()
                ? TimeSpan.FromMinutes(vehicles.Average(v => (now - v.ArrivalTime).TotalMinutes))
                : TimeSpan.Zero
        };

        viewModel.MostCommonType = viewModel.VehicleCountsByType.Any()
            ? viewModel.VehicleCountsByType.OrderByDescending(kvp => kvp.Value).First().Key
            : null;

        var longestParked = vehicles.OrderBy(v => v.ArrivalTime).FirstOrDefault();
        if (longestParked != null)
        {
            viewModel.LongestParkedArrivalTime = longestParked.ArrivalTime;
            viewModel.LongestParkedRegistrationNumber = longestParked.RegistrationNumber;
        }

        return View(viewModel);
    }
    // GET: PARKEDVEHICLES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parkedvehicle = await _context.ParkedVehicle.AsNoTracking()
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

        bool regExists = await _context.ParkedVehicle.AnyAsync(v => v.RegistrationNumber == viewModel.RegistrationNumber);

        if (regExists)
        {
            ModelState.AddModelError("RegistrationNumber", "The registration number already exists. Please enter a different one.");
        }

        // Del 2: reject if there isn't actually room for this vehicle type
        if (!_parkingSpotService.CanParkVehicleType(viewModel.VehicleType))
        {
            ModelState.AddModelError("VehicleType", "There is no available parking spot for this vehicle type right now.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var parkedvehicle = new ParkedVehicle
                {
                    VehicleType = viewModel.VehicleType,
                    RegistrationNumber = viewModel.RegistrationNumber,
                    Color = viewModel.Color ?? string.Empty,
                    Brand = viewModel.Brand ?? string.Empty,
                    Model = viewModel.Model ?? string.Empty,
                    NumberOfWheels = viewModel.NumberOfWheels.Value,
                    ArrivalTime = DateTime.Now
                };

                _context.Add(parkedvehicle);
                await _context.SaveChangesAsync();

                // Del 2: assign a fixed parking spot now that the vehicle has an Id
                var assignmentResult = _parkingSpotService.AssignSpot(viewModel.VehicleType, parkedvehicle.Id);

                if (!assignmentResult.Success)
                {
                    // Extremely unlikely race condition: spot became unavailable between
                    // the check above and this point. Roll back the check-in.
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
    public IActionResult CheckDuplicate(string registrationNumber, int? id)
    {
        // If editing, we don't want to check for duplicates against the same record
        bool isDuplicate = id.HasValue
            ? false
            : _context.ParkedVehicles.Any(v => v.RegistrationNumber == registrationNumber);
        return Json(!isDuplicate);
    }

    // GET: PARKEDVEHICLES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parkedvehicle = await _context.ParkedVehicle.FindAsync(id);
        if (parkedvehicle == null)
        {
            return NotFound();
        }

        var vm = new ParkedVehicleFormViewModel
        {
            Id = parkedvehicle.Id,
            RegistrationNumber = parkedvehicle.RegistrationNumber,
            VehicleType = parkedvehicle.VehicleType,
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
        // Normalize registration number (Trim + ToUpper)
        vm.RegistrationNumber = vm.RegistrationNumber.Trim().ToUpper();
        if (id != vm.Id)
        {
            return NotFound();
        }

        var original = await _context.ParkedVehicle.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (original == null) { return NotFound(); }

        if (original.RegistrationNumber != vm.RegistrationNumber)
        {
            bool regExists = await _context.ParkedVehicle.AnyAsync(v => v.RegistrationNumber == vm.RegistrationNumber);

            if (regExists)
            {
                ModelState.AddModelError("RegistrationNumber", "The registration number already exists. Please enter a different one.");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                original.VehicleType = vm.VehicleType;
                original.RegistrationNumber = vm.RegistrationNumber;
                original.Color = vm.Color ?? string.Empty;
                original.Brand = vm.Brand ?? string.Empty;
                original.Model = vm.Model ?? string.Empty;
                original.NumberOfWheels = vm.NumberOfWheels.Value;

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

        var parkedvehicle = await _context.ParkedVehicle.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (parkedvehicle == null)
        {
            return NotFound();
        }

        /* var viewModel = new CheckOutViewModel
        {
            Id = parkedvehicle.Id,
            RegistrationNumber = parkedvehicle.RegistrationNumber,
            VehicleType = parkedvehicle.VehicleType.ToString(),
            ArrivalTime = parkedvehicle.ArrivalTime
        }; */

        return View(parkedvehicle);
    }

    // POST: PARKEDVEHICLES/CheckOut/5
    [HttpPost, ActionName("CheckOut")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOutConfirmed(int? id)
    {
        var parkedvehicle = await _context.ParkedVehicle.FindAsync(id);

        if (parkedvehicle == null)
        {
            return NotFound();
        }

        DateTime checkOutTime = DateTime.Now;

        // Create the receipt data that will be displayed after check out
        var receiptViewModel = new ReceiptViewModel
        {
            VehicleType = parkedvehicle.VehicleType,
            RegistrationNumber = parkedvehicle.RegistrationNumber,
            Brand = parkedvehicle.Brand,
            Model = parkedvehicle.Model,
            Color = parkedvehicle.Color,
            NumberOfWheels = parkedvehicle.NumberOfWheels,
            //Ny kod for Spot in thr recept
            AssignedSpotNumber = parkedvehicle.AssignedSpotNumber,
            ArrivalTime = parkedvehicle.ArrivalTime,
            CheckOutTime = checkOutTime,

            TotalPrice = _garageFeeService.CalculateFee(
                parkedvehicle.ArrivalTime,
                checkOutTime)
        };

        _context.ParkedVehicle.Remove(parkedvehicle);

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

    // Del 2: builds the VehicleType dropdown with unavailable types grayed out
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
}