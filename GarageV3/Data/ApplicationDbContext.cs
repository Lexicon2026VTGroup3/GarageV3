using GarageV3.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<GarageV3.Models.Entities.ParkedVehicle> ParkedVehicle { get; set; } = default!;

    public DbSet<ParkedVehicle> ParkedVehicles { get; set; }

    public DbSet<ParkingSession> ParkingSessions { get; set; } = default!;
}