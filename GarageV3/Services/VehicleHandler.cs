using Microsoft.EntityFrameworkCore;
using GarageV3.Models.Entities;
using GarageV3.Data;

namespace GarageV3.Services
{
    public class VehicleHandler : IVehicleHandler
    {
        private readonly ApplicationDbContext _context;

        public VehicleHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsExisting(string regNumber)
        {
            // Normalize registration number (Trim + ToUpper)
            regNumber = regNumber.Trim().ToUpperInvariant();
            return _context.ParkedVehicle.Any(v => v.RegistrationNumber == regNumber);
        }
    }
}