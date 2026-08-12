using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminParkingSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminParkingSpotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminParkingSpots
        public async Task<IActionResult> Index()
        {
            var entities = await _context.ParkingSpots
                .OrderBy(p => p.Number)
                .ToListAsync();

            var viewModels = entities.Select(v => new AdminParkingSpotViewModel
            {
                Id = v.Id,
                Number = v.Number,
                Location = v.Location,
                IsOutOfService = v.IsOutOfService
            }).ToList();

            return View(viewModels);
        }

        // GET: AdminParkingSpots/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AdminParkingSpotViewModel
            {
                Number = 1,
                IsOutOfService = false
            };

            return View(viewModel);
        }

        // POST: AdminParkingSpots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminParkingSpotViewModel viewModel)
        {
            if (await _context.ParkingSpots.AnyAsync(p => p.Number == viewModel.Number))
            {
                ModelState.AddModelError("Number", $"Parking spot #{viewModel.Number} already exists.");
            }

            if (ModelState.IsValid)
            {
                var parkingSpotEntity = new ParkingSpot
                {
                    Number = viewModel.Number,
                    Location = viewModel.Location,
                    IsOutOfService = viewModel.IsOutOfService
                };

                _context.ParkingSpots.Add(parkingSpotEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Parking spot #{parkingSpotEntity.Number} was successfully created.";

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: AdminParkingSpots/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.ParkingSpots.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var viewModel = new AdminParkingSpotViewModel
            {
                Id = entity.Id,
                Number = entity.Number,
                Location = entity.Location,
                IsOutOfService = entity.IsOutOfService
            };

            return View(viewModel);
        }

        // POST: AdminParkingSpots/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminParkingSpotViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (await _context.ParkingSpots.AnyAsync(p => p.Number == viewModel.Number && p.Id != id))
            {
                ModelState.AddModelError("Number", $"Parking spot #{viewModel.Number} already exists.");
            }

            if (ModelState.IsValid)
            {
                var entity = await _context.ParkingSpots.FindAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }

                entity.Number = viewModel.Number;
                entity.Location = viewModel.Location;
                entity.IsOutOfService = viewModel.IsOutOfService;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Parking spot #{entity.Number} was successfully updated.";

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: AdminParkingSpots/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.ParkingSpots
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            var viewModel = new AdminParkingSpotViewModel
            {
                Id = entity.Id,
                Number = entity.Number,
                Location = entity.Location,
                IsOutOfService = entity.IsOutOfService
            };

            return View(viewModel);
        }

        // POST: AdminParkingSpots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.ParkingSpots.FindAsync(id);
            if (entity != null)
            {
                _context.ParkingSpots.Remove(entity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Parking spot #{entity.Number} was successfully deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingSpotExists(int id)
        {
            return _context.ParkingSpots.Any(e => e.Id == id);
        }
    }
}