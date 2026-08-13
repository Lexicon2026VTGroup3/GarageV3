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

    public DbSet<Vehicle> Vehicles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Vehicle>()
            .ToTable("Vehicles");

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.PersonalIdentityNumber)
            .IsUnique();

        builder.Entity<ParkingSession>()
            .Property(s => s.HourlyRateAtCheckIn)
            .HasColumnType("decimal(10,2)");

        builder.Entity<ParkingSession>()
            .Property(s => s.TotalPrice)
            .HasColumnType("decimal(10,2)");

        builder.Entity<ParkingAllocation>()
            .HasOne(a => a.ParkingSession)
            .WithMany(s => s.Allocations)
            .HasForeignKey(a => a.ParkingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ParkingAllocation>()
            .HasOne(a => a.ParkingSpot)
            .WithMany()
            .HasForeignKey(a => a.ParkingSpotId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<ParkingSession> ParkingSessions { get; set; }

    public DbSet<VehicleTypeEntity> VehicleTypes { get; set; }

    public DbSet<ParkingSpot> ParkingSpots { get; set; }

    public DbSet<ParkingAllocation> ParkingAllocations { get; set; }
}