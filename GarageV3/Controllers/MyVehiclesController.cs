using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Services.Interfaces;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MyVehiclesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleHandler _vehicleHandler;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyVehiclesController(
        ApplicationDbContext context,
        IVehicleHandler vehicleHandler,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _vehicleHandler = vehicleHandler;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var activeSessions = await _context.ParkingSessions
        .Where(ps => ps.CheckOutTime == null)
        .ToDictionaryAsync(ps => ps.VehicleId, ps => ps.ParkingSpotId);

        var activeSessionIds = await _context.ParkingSessions
        .Where(ps => ps.CheckOutTime == null)
        .ToDictionaryAsync(ps => ps.VehicleId, ps => ps.Id);

        var vehicles = await _context.Vehicles
            .Where(v => v.OwnerId == userId)
            .Include(v => v.VehicleTypeRef)
            .Select(v => new MyVehiclesIndexViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Brand = v.Brand,
                Model = v.Model,
                Color = v.Color,
                NumberOfWheels = v.NumberOfWheels,
                ArrivalTime = v.ArrivalTime,
                VehicleTypeName = v.VehicleTypeRef != null ? v.VehicleTypeRef.Name : "Unknown",
                VehicleTypeIcon = v.VehicleTypeRef != null ? v.VehicleTypeRef.Icon : "Unknown",

                ParkingSpotId = activeSessions.ContainsKey(v.Id) ? activeSessions[v.Id] : (int?)null,

                ActiveParkingSessionId = activeSessionIds.ContainsKey(v.Id) ? activeSessionIds[v.Id] : (int?)null
            })
            .ToListAsync();

        return View(vehicles);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

        if (vehicle == null) return NotFound();

        return View(vehicle);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new ParkedVehicleFormViewModel
        {
            VehicleTypes = await _context.VehicleTypes
                .Select(vt => new SelectListItem
                {
                    Value = vt.Id.ToString(),
                    Text = vt.Icon + " " + vt.Name
                })
                .ToListAsync()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParkedVehicleFormViewModel viewModel)
    {
        viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();

        int.TryParse(viewModel.VehicleTypeId, out int typeId);
        var vehicleTypeEntity = await _context.VehicleTypes.FindAsync(typeId);

        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleTypeId", "Selected vehicle type is not recognized.");
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            ModelState.AddModelError(string.Empty, "You must be logged in to register a vehicle.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var vehicle = new Vehicle
                {
                    VehicleTypeRefId = vehicleTypeEntity!.Id,
                    OwnerId = userId!,
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
                ModelState.AddModelError(string.Empty, "Could not register vehicle.");
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

        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == userId);

        if (vehicle == null) return NotFound();

        bool isParked = await _context.ParkingSessions
            .AnyAsync(ps => ps.VehicleId == id && ps.CheckOutTime == null);

        var vm = new ParkedVehicleFormViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            VehicleTypeId = vehicle.VehicleTypeRefId.ToString(),
            Color = vehicle.Color,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            NumberOfWheels = vehicle.NumberOfWheels,
            ArrivalTime = vehicle.ArrivalTime,
            IsParked = isParked,

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ParkedVehicleFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        vm.RegistrationNumber = vm.RegistrationNumber.Trim().ToUpper();
        var userId = _userManager.GetUserId(User);

        var original = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == userId);

        if (original == null) return NotFound();

        if (original.RegistrationNumber != vm.RegistrationNumber)
        {
            if (await _vehicleHandler.IsExistingAsync(vm.RegistrationNumber, id))
            {
                ModelState.AddModelError("RegistrationNumber", "Registration number already exists.");
            }
        }

        int.TryParse(vm.VehicleTypeId, out int typeId);
        var vehicleTypeEntity = await _context.VehicleTypes.FindAsync(typeId);

        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleTypeId", "Vehicle type not recognized.");
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

                TempData["SuccessMessage"] = $"Successfully updated {vm.RegistrationNumber}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not save changes.");
            }
        }

        vm.VehicleTypes = await _context.VehicleTypes
            .Select(vt => new SelectListItem
            {
                Value = vt.Id.ToString(),
                Text = vt.Icon + " " + vt.Name
            })
            .ToListAsync();

        return View(vm);
    }

// GET: MyVehicles/Delete/5
public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    // POST: MyVehicles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);

        bool isParked = await _context.ParkingSessions
            .AnyAsync(ps => ps.VehicleId == id && ps.CheckOutTime == null);

        if (isParked)
        {
            TempData["ErrorMessage"] = "Cannot delete: This vehicle is currently parked in the garage.";
            return RedirectToAction(nameof(Index));
        }

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == userId);

        if (vehicle == null)
        {
            return NotFound();
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Successfully deleted vehicle {vehicle.RegistrationNumber}.";

        return RedirectToAction(nameof(Index));
    }

    private bool VehicleExists(int? id)
    {
        return _context.Vehicles.Any(e => e.Id == id);
    }
}
