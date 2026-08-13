using GarageV3.Data;
using GarageV3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Controllers
{
    // Public landing page. Reachable by Anonymous/Member/Admin alike, but
    // only ever shows aggregate counts — never vehicle details.
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var spots = await _context.ParkingSpots
                .AsNoTracking()
                .ToListAsync();

            var usedUnitsBySpot = await _context.ParkingAllocations
                .Where(a => a.ParkingSession!.CheckOutTime == null)
                .GroupBy(a => a.ParkingSpotId)
                .Select(g => new { SpotId = g.Key, Used = g.Sum(a => a.UnitsUsed) })
                .ToDictionaryAsync(x => x.SpotId, x => x.Used);

            int outOfService = spots.Count(s => s.IsOutOfService);

            int free = spots.Count(s =>
                !s.IsOutOfService &&
                (s.CapacityUnits - usedUnitsBySpot.GetValueOrDefault(s.Id, 0)) > 0);

            int occupied = spots.Count - outOfService - free;

            var viewModel = new HomeViewModel
            {
                TotalSpots = spots.Count,
                FreeSpots = free,
                OccupiedSpots = occupied,
                OutOfServiceSpots = outOfService
            };

            return View(viewModel);
        }
    }
}