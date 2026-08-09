
using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public MyVehiclesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
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

    // POST: VEHICLES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,VehicleTypeRefId,VehicleTypeRef,OwnerId,Owner,RegistrationNumber,Color,Brand,Model,NumberOfWheels,ArrivalTime,AssignedSpotNumber")] Vehicle vehicle)
    {
        if (id != vehicle.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(vehicle.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(vehicle);
    }

    // GET: VEHICLES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(m => m.Id == id);
        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    // POST: VEHICLES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool VehicleExists(int? id)
    {
        return _context.Vehicles.Any(e => e.Id == id);
    }
}
