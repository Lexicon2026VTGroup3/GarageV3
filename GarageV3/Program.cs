using GarageV3.Data;
using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using GarageV3.Models.Parking;
using GarageV3.Services;
using GarageV3.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

//builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
//{
//    options.TokenLifespan = TimeSpan.FromHours(3);
//});
builder.Services.AddTransient<IEmailSender, DevelopmentEmailSender>();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
}).AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IVehicleHandler, VehicleHandler>();
builder.Services.AddScoped<GarageFeeService>();

builder.Services.Configure<GarageSettings>(
    builder.Configuration.GetSection(GarageSettings.SectionName));
builder.Services.AddScoped<IParkingSpotService, ParkingSpotService>();
builder.Services.AddScoped<IParkingSessionService, ParkingSessionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        // Roles and the admin user must exist BEFORE vehicles are seeded,
        // since every vehicle now requires an OwnerId.
        await DbInitializer.SeedRolesAndAdminAsync(app.Services);

        AddVehicleTypeAndParkingSpotSeedData(context, services);
        await AddSeedDataAsync(context, services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occured during Database migration.");
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");
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

app.Run();


async Task AddSeedDataAsync(ApplicationDbContext context, IServiceProvider services)
{
    if (context.Vehicles.Any())
    {
        return;
    }

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var adminUser = await userManager.FindByEmailAsync("admin@garage.com");

    if (adminUser == null)
    {
        // Admin seeding didn't run yet for some reason; skip vehicle seeding rather than crash.
        return;
    }

    var typesByEnum = context.VehicleTypes.ToDictionary(vt => vt.EnumValue, vt => vt.Id);

    var vehiclesToSeed = new List<Vehicle>
    {
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Car], OwnerId = adminUser.Id, RegistrationNumber = "ABC123", Color = "Black", Brand = "Volvo", Model = "XC60", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-3) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Motorcycle], OwnerId = adminUser.Id, RegistrationNumber = "KTM555", Color = "Orange", Brand = "KTM", Model = "Duke 390", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddDays(-1) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Bus], OwnerId = adminUser.Id, RegistrationNumber = "BUS010", Color = "Red", Brand = "Scania", Model = "Citywide", NumberOfWheels = 6, ArrivalTime = DateTime.Now.AddHours(-8) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Truck], OwnerId = adminUser.Id, RegistrationNumber = "TRK777", Color = "Blue", Brand = "Volvo", Model = "FH16", NumberOfWheels = 10, ArrivalTime = DateTime.Now.AddDays(-2) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Bicycle], OwnerId = adminUser.Id, RegistrationNumber = "BIK111", Color = "Yellow", Brand = "Crescent", Model = "Kebne", NumberOfWheels = 2, ArrivalTime = DateTime.Now.AddMinutes(-30) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Airplane], OwnerId = adminUser.Id, RegistrationNumber = "SAS901", Color = "White", Brand = "Airbus", Model = "A320neo", NumberOfWheels = 3, ArrivalTime = DateTime.Now.AddHours(-15) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Boat], OwnerId = adminUser.Id, RegistrationNumber = "BOA999", Color = "White", Brand = "Buster", Model = "Magnum", NumberOfWheels = 0, ArrivalTime = DateTime.Now.AddHours(-12) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Car], OwnerId = adminUser.Id, RegistrationNumber = "XYZ789", Color = "White", Brand = "Tesla", Model = "Model Y", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-5) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Car], OwnerId = adminUser.Id, RegistrationNumber = "MLB442", Color = "Grey", Brand = "Volkswagen", Model = "Golf", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddMinutes(-45) },
        new Vehicle { VehicleTypeRefId = typesByEnum[VehicleType.Car], OwnerId = adminUser.Id, RegistrationNumber = "SWE999", Color = "Silver", Brand = "Polestar", Model = "Polestar 2", NumberOfWheels = 4, ArrivalTime = DateTime.Now.AddHours(-2) }
    };

    context.Vehicles.AddRange(vehiclesToSeed);
    context.SaveChanges();

    var parkingService = services.GetRequiredService<IParkingSpotService>();

    foreach (var vehicle in vehiclesToSeed)
    {
        var enumValue = typesByEnum.First(kvp => kvp.Value == vehicle.VehicleTypeRefId).Key;
        parkingService.AssignSpot(enumValue, vehicle.Id);
    }
}

void AddVehicleTypeAndParkingSpotSeedData(ApplicationDbContext context, IServiceProvider services)
{
    if (!context.VehicleTypes.Any())
    {
        var vehicleTypes = new[]
        {
            new VehicleTypeEntity { Name = "Car", EnumValue = VehicleType.Car },
            new VehicleTypeEntity { Name = "Motorcycle", EnumValue = VehicleType.Motorcycle },
            new VehicleTypeEntity { Name = "Bus", EnumValue = VehicleType.Bus },
            new VehicleTypeEntity { Name = "Truck", EnumValue = VehicleType.Truck },
            new VehicleTypeEntity { Name = "Bicycle", EnumValue = VehicleType.Bicycle },
            new VehicleTypeEntity { Name = "Airplane", EnumValue = VehicleType.Airplane },
            new VehicleTypeEntity { Name = "Boat", EnumValue = VehicleType.Boat }
        };

        context.VehicleTypes.AddRange(vehicleTypes);
        context.SaveChanges();
    }

    if (!context.ParkingSpots.Any())
    {
        var settings = services.GetRequiredService<IOptions<GarageSettings>>().Value;

        var spots = Enumerable.Range(1, settings.TotalParkingSpots)
            .Select(n => new ParkingSpot { Number = n, IsOutOfService = false });

        context.ParkingSpots.AddRange(spots);
        context.SaveChanges();
    }
}