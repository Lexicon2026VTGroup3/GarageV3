using GarageV3.Models;
using GarageV3.Models.Entities;
using GarageV3.Models.Parking;
using GarageV3.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GarageV3.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 1. Ensure database exists
        // await context.Database.EnsureCreatedAsync();
        // Note: If using EF Core Migrations, use context.Database.MigrateAsync() instead.
        await context.Database.MigrateAsync();

        // 2. Roles & Admin (Must run first for OwnerId dependencies)
        await SeedRolesAndAdminAsync(services);

        // 3. Vehicle Types & Parking Spots (Must run before Vehicles for Type FKs)
        await SeedVehicleTypesAndParkingSpotsAsync(context, services);

        // 4. Vehicles
        await SeedVehiclesAsync(context, services);
    }

    private static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Create Admin and Member roles if missing
        string[] roles = { "Admin", "Member" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin user if missing
        const string adminEmail = "admin@garage.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                PersonalIdentityNumber = "19800101-0000",
                EmailConfirmed = true
            };

            // Seeded dev accounts may use a default password
            var result = await userManager.CreateAsync(adminUser, "Abc123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedVehicleTypesAndParkingSpotsAsync(ApplicationDbContext context, IServiceProvider services)
    {
        if (!await context.VehicleTypes.AnyAsync())
        {
            var vehicleTypes = new[]
            {
            // RequiredSpaceUnits: matches a standard ParkingSpot.CapacityUnits
            // of 3. Car takes a whole spot; Motorcycle/Bicycle share one (3
            // fit); large vehicles need more than one spot's capacity (US12).
            new VehicleTypeEntity { Name = "Car", ShortName = "Car", Icon = "🚗", BadgeColor = "#006AA7", BadgeTextColor = "#ffffff", RequiredSpots = 1, MaxVehiclesPerSpot = 1, RequiredSpaceUnits = 3 },
            new VehicleTypeEntity { Name = "Motorcycle", ShortName = "MC", Icon = "🏍️", BadgeColor = "#FECC02", BadgeTextColor = "#1a1a1a", RequiredSpots = 1, MaxVehiclesPerSpot = 3, RequiredSpaceUnits = 1 },
            new VehicleTypeEntity { Name = "Bus", ShortName = "Bus", Icon = "🚌", BadgeColor = "#1a7a4c", BadgeTextColor = "#ffffff", RequiredSpots = 2, MaxVehiclesPerSpot = 1, RequiredSpaceUnits = 6 },
            new VehicleTypeEntity { Name = "Truck", ShortName = "Truck", Icon = "🚚", BadgeColor = "#2c3e50", BadgeTextColor = "#ffffff", RequiredSpots = 3, MaxVehiclesPerSpot = 1, RequiredSpaceUnits = 6 },
            new VehicleTypeEntity { Name = "Bicycle", ShortName = "Bike", Icon = "🚲", BadgeColor = "#6b7280", BadgeTextColor = "#ffffff", RequiredSpots = 1, MaxVehiclesPerSpot = 5, RequiredSpaceUnits = 1 },
            new VehicleTypeEntity { Name = "Airplane", ShortName = "Plane", Icon = "✈", BadgeColor = "#6b7280", BadgeTextColor = "#ffffff", RequiredSpots = 3, MaxVehiclesPerSpot = 1, RequiredSpaceUnits = 9 },
            new VehicleTypeEntity { Name = "Boat", ShortName = "Boat", Icon = "🚤", BadgeColor = "#0891b2", BadgeTextColor = "#ffffff", RequiredSpots = 2, MaxVehiclesPerSpot = 1, RequiredSpaceUnits = 6 }
        };

            await context.VehicleTypes.AddRangeAsync(vehicleTypes);
            await context.SaveChangesAsync();
        }

        if (!await context.ParkingSpots.AnyAsync())
        {
            var settings = services.GetRequiredService<IOptions<GarageSettings>>().Value;

            var spots = Enumerable.Range(1, settings.TotalParkingSpots)
                .Select(n => new ParkingSpot
                {
                    Number = n,
                    IsOutOfService = false,
                    CapacityUnits = 3,
                    Location = n <= 5 ? "Floor 1 - Section A" :
                               n <= 10 ? "Floor 1 - Section B" :
                               n <= 15 ? "Floor 2 - Section A" : "Floor 2 - Section B"
                });

            await context.ParkingSpots.AddRangeAsync(spots);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedVehiclesAsync(ApplicationDbContext context, IServiceProvider services)
    {
        if (await context.Vehicles.AnyAsync())
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync("admin@garage.com");

        if (adminUser == null)
        {
            return;
        }

        var typesByName = await context.VehicleTypes.ToDictionaryAsync(vt => vt.Name, vt => vt.Id);

        var vehiclesToSeed = new List<Vehicle>
        {
            new Vehicle { VehicleTypeRefId = typesByName["Car"], OwnerId = adminUser.Id, RegistrationNumber = "ABC123", Color = "Black", Brand = "Volvo", Model = "XC60", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-3) },
            new Vehicle { VehicleTypeRefId = typesByName["Motorcycle"], OwnerId = adminUser.Id, RegistrationNumber = "KTM555", Color = "Orange", Brand = "KTM", Model = "Duke 390", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddDays(-1) },
            new Vehicle { VehicleTypeRefId = typesByName["Bus"], OwnerId = adminUser.Id, RegistrationNumber = "BUS010", Color = "Red", Brand = "Scania", Model = "Citywide", NumberOfWheels = 6, ArrivalTime = DateTime.Now.AddHours(-8) },
            new Vehicle { VehicleTypeRefId = typesByName["Truck"], OwnerId = adminUser.Id, RegistrationNumber = "TRK777", Color = "Blue", Brand = "Volvo", Model = "FH16", NumberOfWheels = 10, ArrivalTime = DateTime.Now.AddDays(-2) },
            new Vehicle { VehicleTypeRefId = typesByName["Bicycle"], OwnerId = adminUser.Id, RegistrationNumber = "BIK111", Color = "Yellow", Brand = "Crescent", Model = "Kebne", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddMinutes(-30) },
            new Vehicle { VehicleTypeRefId = typesByName["Airplane"], OwnerId = adminUser.Id, RegistrationNumber = "SAS901", Color = "White", Brand = "Airbus", Model = "A320neo", NumberOfWheels = 3, ArrivalTime = DateTime.Now.AddHours(-15) },
            new Vehicle { VehicleTypeRefId = typesByName["Boat"], OwnerId = adminUser.Id, RegistrationNumber = "BOA999", Color = "White", Brand = "Buster", Model = "Magnum", NumberOfWheels = 0, ArrivalTime = DateTime.Now.AddHours(-12) },
            new Vehicle { VehicleTypeRefId = typesByName["Car"], OwnerId = adminUser.Id, RegistrationNumber = "XYZ789", Color = "White", Brand = "Tesla", Model = "Model Y", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-5) },
            new Vehicle { VehicleTypeRefId = typesByName["Car"], OwnerId = adminUser.Id, RegistrationNumber = "MLB442", Color = "Grey", Brand = "Volkswagen", Model = "Golf", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddMinutes(-45) },
            new Vehicle { VehicleTypeRefId = typesByName["Car"], OwnerId = adminUser.Id, RegistrationNumber = "SWE999", Color = "Silver", Brand = "Polestar", Model = "Polestar 2", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-2) }
        };

        await context.Vehicles.AddRangeAsync(vehiclesToSeed);
        await context.SaveChangesAsync();

        // Seeded vehicles are registered only, not auto-parked, so there's
        // something to try out manually on the "Park a Vehicle" page.
        // (Previously auto-parked via the old ParkingSpotService.AssignSpot —
        // dropped along with that service.)
    }

}