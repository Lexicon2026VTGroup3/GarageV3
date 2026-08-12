using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVehicleTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminVehicleTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entities = await _context.VehicleTypes.ToListAsync();

            var viewModels = entities.Select(v => new AdminVehicleTypeViewModel
            {
                Id = v.Id,
                Name = v.Name,
                ShortName = v.ShortName,
                Icon = v.Icon,
                BadgeColor = v.BadgeColor,
                BadgeTextColor = v.BadgeTextColor,
                RequiredSpots = v.RequiredSpots,
                MaxVehiclesPerSpot = v.MaxVehiclesPerSpot
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AdminVehicleTypeViewModel
            {
                RequiredSpots = 1,
                MaxVehiclesPerSpot = 1,
                BadgeColor = "#006AA7",
                BadgeTextColor = "#ffffff"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminVehicleTypeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var vehicleTypeEntity = new VehicleTypeEntity
                {
                    Name = viewModel.Name,
                    ShortName = viewModel.ShortName,
                    Icon = viewModel.Icon,
                    BadgeColor = viewModel.BadgeColor,
                    BadgeTextColor = viewModel.BadgeTextColor,
                    RequiredSpots = viewModel.RequiredSpots,
                    MaxVehiclesPerSpot = viewModel.MaxVehiclesPerSpot
                };

                _context.VehicleTypes.Add(vehicleTypeEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Vehicle type '{vehicleTypeEntity.Name}' was successfully created.";

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.VehicleTypes.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var viewModel = new AdminVehicleTypeViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ShortName = entity.ShortName,
                Icon = entity.Icon,
                BadgeColor = entity.BadgeColor,
                BadgeTextColor = entity.BadgeTextColor,
                RequiredSpots = entity.RequiredSpots,
                MaxVehiclesPerSpot = entity.MaxVehiclesPerSpot
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminVehicleTypeViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var entity = await _context.VehicleTypes.FindAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }

                entity.Name = viewModel.Name;
                entity.ShortName = viewModel.ShortName;
                entity.Icon = viewModel.Icon;
                entity.BadgeColor = viewModel.BadgeColor;
                entity.BadgeTextColor = viewModel.BadgeTextColor;
                entity.RequiredSpots = viewModel.RequiredSpots;
                entity.MaxVehiclesPerSpot = viewModel.MaxVehiclesPerSpot;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Vehicle type '{entity.Name}' was successfully updated.";

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleTypeEntity = await _context.VehicleTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehicleTypeEntity == null)
            {
                return NotFound();
            }

            var viewModel = new AdminVehicleTypeViewModel
            {
                Id = vehicleTypeEntity.Id,
                Name = vehicleTypeEntity.Name,
                ShortName = vehicleTypeEntity.ShortName,
                Icon = vehicleTypeEntity.Icon,
                BadgeColor = vehicleTypeEntity.BadgeColor,
                BadgeTextColor = vehicleTypeEntity.BadgeTextColor,
                RequiredSpots = vehicleTypeEntity.RequiredSpots,
                MaxVehiclesPerSpot = vehicleTypeEntity.MaxVehiclesPerSpot
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicleTypeEntity = await _context.VehicleTypes.FindAsync(id);
            if (vehicleTypeEntity != null)
            {
                _context.VehicleTypes.Remove(vehicleTypeEntity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Vehicle type '{vehicleTypeEntity.Name}' was successfully deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleTypeExists(int id)
        {
            return _context.VehicleTypes.Any(e => e.Id == id);
        }
    }
}