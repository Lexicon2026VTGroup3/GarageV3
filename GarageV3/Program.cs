using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Models.Parking;
using GarageV3.Services;
using GarageV3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GarageV3.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IVehicleHandler, VehicleHandler>();

builder.Services.AddScoped<IParkingSessionService, ParkingSessionService>();

builder.Services.AddScoped<IParkingSpotService, ParkingSpotService>();

builder.Services.AddSingleton<GarageFeeService>();

// Del 2: garage parking spot settings + service
builder.Services.Configure<GarageSettings>(
    builder.Configuration.GetSection(GarageSettings.SectionName));

var app = builder.Build();

// Auto Migration: Update DB file and table automatically when the application starts.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated(); // Create DB file and table if they do not exist

        AddSeedData(context, services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occured during Database migration.");
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ParkedVehicles}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

// Seed roles and admin user on app startup
try
{
    await DbInitializer.SeedRolesAndAdminAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding the database.");
}

app.Run();


void AddSeedData(ApplicationDbContext context, IServiceProvider services)
{
    if (context.ParkedVehicles.Any())
    {
        return;
    }

    var vehiclesToSeed = new List<ParkedVehicle>
    {
        new ParkedVehicle { VehicleType = VehicleType.Car, RegistrationNumber = "ABC123", Color = "Black", Brand = "Volvo", Model = "XC60", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-3) },
        new ParkedVehicle { VehicleType = VehicleType.Motorcycle, RegistrationNumber = "KTM555", Color = "Orange", Brand = "KTM", Model = "Duke 390", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddDays(-1) },
        new ParkedVehicle { VehicleType = VehicleType.Bus, RegistrationNumber = "BUS010", Color = "Red", Brand = "Scania", Model = "Citywide", NumberOfWheels = 6, ArrivalTime = DateTime.Now.AddHours(-8) },
        new ParkedVehicle { VehicleType = VehicleType.Truck, RegistrationNumber = "TRK777", Color = "Blue", Brand = "Volvo", Model = "FH16", NumberOfWheels = 10, ArrivalTime = DateTime.Now.AddDays(-2) },
        new ParkedVehicle { VehicleType = VehicleType.Bicycle, RegistrationNumber = "BIK111", Color = "Yellow", Brand = "Crescent", Model = "Kebne", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddMinutes(-30) },
        new ParkedVehicle { VehicleType = VehicleType.Airplane, RegistrationNumber = "SAS901", Color = "White", Brand = "Airbus", Model = "A320neo", NumberOfWheels = 3, ArrivalTime = DateTime.Now.AddHours(-15) },
        new ParkedVehicle { VehicleType = VehicleType.Boat, RegistrationNumber = "BOA999", Color = "White", Brand = "Buster", Model = "Magnum", NumberOfWheels = 0, ArrivalTime = DateTime.Now.AddHours(-12) },
        new ParkedVehicle { VehicleType = VehicleType.Car, RegistrationNumber = "XYZ789", Color = "White", Brand = "Tesla", Model = "Model Y", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-5) },
        new ParkedVehicle { VehicleType = VehicleType.Car, RegistrationNumber = "MLB442", Color = "Grey", Brand = "Volkswagen", Model = "Golf", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddMinutes(-45) },
        new ParkedVehicle { VehicleType = VehicleType.Car, RegistrationNumber = "SWE999", Color = "Silver", Brand = "Polestar", Model = "Polestar 2", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-2) }
    };

    context.ParkedVehicles.AddRange(vehiclesToSeed);
    context.SaveChanges();

    var parkingService = services.GetRequiredService<IParkingSpotService>();

    foreach (var vehicle in vehiclesToSeed)
    {
        parkingService.AssignSpot(vehicle.VehicleType, vehicle.Id);
    }
}