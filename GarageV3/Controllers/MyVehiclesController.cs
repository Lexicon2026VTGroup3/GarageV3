
using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
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

    // GET: VEHICLES
    public async Task<IActionResult> Index()    
    {
        var userId = _userManager.GetUserId(User);

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
                VehicleTypeName = v.VehicleTypeRef != null ? v.VehicleTypeRef.Name : string.Empty,
                AssignedSpotNumber = v.AssignedSpotNumber
            })
            .ToListAsync();

        return View(vehicles);
    }

    // GET: MyVehicles/Details/5
    public async Task<IActionResult> Details(int? id)
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

    // GET: VEHICLES/Create
    public IActionResult Create()
    {
        var viewModel = new ParkedVehicleFormViewModel
        {
            VehicleTypes = Enum.GetValues(typeof(VehicleType))
                              .Cast<VehicleType>()
                              .Select(t => new SelectListItem
                              {
                                  Value = t.ToString(),
                                  Text = t.ToString()
                              })
        };
        return View(viewModel);
    }

    // POST: VEHICLES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParkedVehicleFormViewModel viewModel)
    {
        viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();

        var vehicleTypeEntity = await _context.VehicleTypes
            .FirstOrDefaultAsync(vt => vt.EnumValue == viewModel.VehicleType);

        if (vehicleTypeEntity == null)
        {
            ModelState.AddModelError("VehicleType", "Selected vehicle type is not recognized.");
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
                ModelState.AddModelError(string.Empty, "Could not register vehicle. Please check all fields.");
                Console.WriteLine("DB ERROR: " + ex.Message);
            }
        }

        viewModel.VehicleTypes = Enum.GetValues(typeof(VehicleType))
            .Cast<VehicleType>()
            .Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = v.ToString()
            });

        return View(viewModel);
    }

    // GET: MyVehicles/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == userId);

        if (vehicle == null)
        {
            return NotFound();
        }

        var vm = new ParkedVehicleFormViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            VehicleType = vehicle.VehicleTypeRef!.EnumValue,
            Color = vehicle.Color,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            NumberOfWheels = vehicle.NumberOfWheels,
            ArrivalTime = vehicle.ArrivalTime,

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

    // POST: MyVehicles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ParkedVehicleFormViewModel vm)
    {
        if (id != vm.Id)
        {
            return NotFound();
        }

        vm.RegistrationNumber = vm.RegistrationNumber.Trim().ToUpper();

        var userId = _userManager.GetUserId(User);

        var original = await _context.Vehicles
            .Include(v => v.VehicleTypeRef)
            .FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == userId);

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

                TempData["SuccessMessage"] = $"Successfully updated vehicle {vm.RegistrationNumber}.";
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

    private async Task<VehicleTypeEntity?> GetVehicleTypeEntityAsync(VehicleType type)
    {
        return await _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.EnumValue == type);
    }
}
