using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminVehiclesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleHandler _vehicleHandler;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminVehiclesController(
        ApplicationDbContext context,
        IVehicleHandler vehicleHandler,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _vehicleHandler = vehicleHandler;
        _userManager = userManager;
    }

    // GET: VEHICLES
    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery)
    {
        var query = _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .Include(v => v.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(v => v.Owner != null && v.Owner.Email != null && v.Owner.Email.Contains(searchQuery));
        }

        var activeSessionSpots = _context.ParkingSessions
            .Where(s => s.CheckOutTime == null)
            .Select(s => new { s.VehicleId, s.ParkingSpotId });

        var vehicleItems = await query
            .Select(v => new AdminVehiclesIndexViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Brand = v.Brand,
                Model = v.Model,
                Color = v.Color,
                ArrivalTime = v.ArrivalTime,
                VehicleTypeName = v.VehicleTypeRef != null ? v.VehicleTypeRef.Name : "Unknown",
                VehicleTypeIcon = v.VehicleTypeRef != null ? v.VehicleTypeRef.Icon : "Unknown",
                ParkingSpotId = activeSessionSpots
                    .Where(s => s.VehicleId == v.Id)
                    .Select(s => (int?)s.ParkingSpotId)
                    .FirstOrDefault(),
                OwnerEmail = v.Owner != null ? v.Owner.Email! : "No Email"
            })
            .ToListAsync();

        ViewData["CurrentFilter"] = searchQuery;

        return View(vehicleItems);
    }

    // GET: AdminVehicles/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null) return NotFound();

        var activeSpotId = await _context.ParkingSessions
            .Where(s => s.VehicleId == vehicle.Id && s.CheckOutTime == null)
            .Select(s => (int?)s.ParkingSpotId)
            .FirstOrDefaultAsync();

        var viewModel = new AdminVehiclesIndexViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Color = vehicle.Color,
            NumberOfWheels = vehicle.NumberOfWheels,
            ArrivalTime = vehicle.ArrivalTime,
            VehicleTypeName = vehicle.VehicleTypeRef?.Name ?? "Unknown",
            VehicleTypeIcon = vehicle.VehicleTypeRef?.Icon ?? "Unknown",
            BadgeColor = vehicle.VehicleTypeRef?.BadgeColor ?? "Unknown",
            BadgeTextColor = vehicle.VehicleTypeRef?.BadgeTextColor ?? "Unknown",
            RequiredSpots = vehicle.VehicleTypeRef?.RequiredSpots ?? 1,
            ParkingSpotId = activeSpotId,
            OwnerEmail = vehicle.Owner?.Email ?? "No Owner"
        };

        return View(viewModel);
    }

    // GET: VEHICLES/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new AdminVehicleCreateViewModel
        {
            VehicleTypes = await _context.VehicleTypes
                .Select(vt => new SelectListItem
                {
                    Value = vt.Id.ToString(),
                    Text = vt.Icon + " " + vt.Name
                })
                .ToListAsync(),

            Users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                })
                .ToListAsync()
        };
        return View(viewModel);
    }

    // POST: VEHICLES/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminVehicleCreateViewModel viewModel)
    {
        viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();

        int vehicleTypeId = 0;
        int.TryParse(viewModel.VehicleTypeId, out vehicleTypeId);

        var vehicleTypeEntity = await _context.VehicleTypes.FindAsync(vehicleTypeId);

        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleTypeId", "Selected vehicle type is not recognized.");
        }

        if (string.IsNullOrWhiteSpace(viewModel.OwnerId))
        {
            ModelState.AddModelError("OwnerId", "You must select a vehicle owner.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var vehicle = new Vehicle
                {
                    VehicleTypeRefId = vehicleTypeEntity!.Id,
                    OwnerId = viewModel.OwnerId,
                    RegistrationNumber = viewModel.RegistrationNumber,
                    Color = viewModel.Color ?? string.Empty,
                    Brand = viewModel.Brand ?? string.Empty,
                    Model = viewModel.Model ?? string.Empty,
                    NumberOfWheels = viewModel.NumberOfWheels.GetValueOrDefault(),
                    ArrivalTime = DateTime.Now
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully registered vehicle {viewModel.RegistrationNumber}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not register vehicle. Please check all fields.");
                Console.WriteLine("DB ERROR: " + ex.Message);
            }
        }

        viewModel.VehicleTypes = await _context.VehicleTypes
            .Select(vt => new SelectListItem
            {
                Value = vt.Id.ToString(),
                Text = vt.Icon + " " + vt.Name
            })
            .ToListAsync();

        viewModel.Users = await _context.Users
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Email
            })
            .ToListAsync();

        return View(viewModel);
    }

    // GET: AdminVehicles/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null) return NotFound();

        bool isParked = await _context.ParkingSessions
        .AnyAsync(ps => ps.VehicleId == id && ps.CheckOutTime == null);

        var vm = new AdminVehicleCreateViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            VehicleTypeId = vehicle.VehicleTypeRefId.ToString(),
            Color = vehicle.Color,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            NumberOfWheels = vehicle.NumberOfWheels,
            ArrivalTime = vehicle.ArrivalTime,
            OwnerId = vehicle.OwnerId,
            IsParked = isParked,

            Users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                })
                .ToListAsync(),

            VehicleTypes = await _context.VehicleTypes
                .Select(vt => new SelectListItem
                {
                    Value = vt.Id.ToString(),
                    Text = vt.Icon + " " + vt.Name
                })
                .ToListAsync()
        };

        return View(vm);
    }

    // POST: AdminVehicles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, AdminVehicleCreateViewModel vm)
    {
        if (id != vm.Id)
        {
            return NotFound();
        }

        vm.RegistrationNumber = vm.RegistrationNumber.Trim().ToUpper();

        var original = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (original == null)
        {
            return NotFound();
        }

        if (original.RegistrationNumber != vm.RegistrationNumber)
        {
            bool regExists = await _vehicleHandler.IsExistingAsync(vm.RegistrationNumber, id);
            if (regExists)
            {
                ModelState.AddModelError("RegistrationNumber", "The registration number already exists. Please enter a different one.");
            }
        }

        int vehicleTypeId = 0;
        int.TryParse(vm.VehicleTypeId, out vehicleTypeId);
        var vehicleTypeEntity = await _context.VehicleTypes.FindAsync(vehicleTypeId);

        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleTypeId", "Selected vehicle type is not recognized.");
        }

        if (string.IsNullOrWhiteSpace(vm.OwnerId))
        {
            ModelState.AddModelError("OwnerId", "You must select a vehicle owner.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                original.VehicleTypeRefId = vehicleTypeEntity!.Id;
                original.OwnerId = vm.OwnerId;
                original.RegistrationNumber = vm.RegistrationNumber;
                original.Color = vm.Color ?? string.Empty;
                original.Brand = vm.Brand ?? string.Empty;
                original.Model = vm.Model ?? string.Empty;
                original.NumberOfWheels = vm.NumberOfWheels.GetValueOrDefault();

                _context.Update(original);
                await _context.SaveChangesAsync();

                bool isParked = await _context.ParkingSessions
                    .AnyAsync(ps => ps.VehicleId == id && ps.CheckOutTime == null);

                if (isParked)
                {
                    TempData["WarningMessage"] = $"Vehicle {vm.RegistrationNumber} was updated, but please note it is currently parked in the garage.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Successfully updated vehicle {vm.RegistrationNumber}.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not save changes. Please check all fields.");
                Console.WriteLine("DB ERROR: " + ex.Message);
            }
        }

        vm.VehicleTypes = await _context.VehicleTypes
            .Select(vt => new SelectListItem
            {
                Value = vt.Id.ToString(),
                Text = vt.Icon + " " + vt.Name
            })
            .ToListAsync();

        vm.Users = await _context.Users
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Email
            })
            .ToListAsync();

        return View(vm);
    }

    // GET: AdminVehicles/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null) return NotFound();

        var activeSpotId = await _context.ParkingSessions
            .Where(s => s.VehicleId == vehicle.Id && s.CheckOutTime == null)
            .Select(s => (int?)s.ParkingSpotId)
            .FirstOrDefaultAsync();

        var viewModel = new AdminVehiclesIndexViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Color = vehicle.Color,
            NumberOfWheels = vehicle.NumberOfWheels,
            ArrivalTime = vehicle.ArrivalTime,
            VehicleTypeName = vehicle.VehicleTypeRef?.Name ?? "Unknown",
            VehicleTypeIcon = vehicle.VehicleTypeRef?.Icon ?? "Unknown",
            BadgeColor = vehicle.VehicleTypeRef?.BadgeColor ?? "Unknown",
            BadgeTextColor = vehicle.VehicleTypeRef?.BadgeTextColor ?? "Unknown",
            RequiredSpots = vehicle.VehicleTypeRef?.RequiredSpots ?? 1,
            ParkingSpotId = activeSpotId,
            OwnerEmail = vehicle.Owner?.Email ?? "No Owner"
        };

        return View(viewModel);
    }

    // POST: AdminVehicles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool isParked = await _context.ParkingSessions
            .AnyAsync(ps => ps.VehicleId == id && ps.CheckOutTime == null);

        if (isParked)
        {
            TempData["ErrorMessage"] = "Cannot delete: This vehicle is currently parked in the garage.";
            return RedirectToAction(nameof(Index));
        }

        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Vehicle {vehicle.RegistrationNumber} was successfully deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool VehicleExists(int? id)
    {
        return _context.Vehicles.Any(e => e.Id == id);
    }
}