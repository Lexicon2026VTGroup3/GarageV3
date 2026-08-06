using Microsoft.EntityFrameworkCore;
using GarageV3.Models.Entities;
using GarageV3.Data;
using GarageV3.Services.Interfaces;

namespace GarageV3.Services
{
    public class VehicleHandler : IVehicleHandler
    {
        private readonly ApplicationDbContext _context;

        public VehicleHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsExistingAsync(string regNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(regNumber))
                return false;

            var normalizedRegNumber = regNumber.Trim().ToUpperInvariant();

            // If editing, we don't want to check for duplicates against the same record
            return await _context.ParkedVehicles
                .AnyAsync(v => v.RegistrationNumber == normalizedRegNumber &&
                      (!excludeId.HasValue || v.Id != excludeId.Value));
        }
    }
}