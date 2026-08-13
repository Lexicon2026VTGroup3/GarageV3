using GarageV3.Data;
using GarageV3.Models;
using GarageV3.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageV3.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: UserManagement
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userList = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                PersonalIdentityNumber = user.PersonalIdentityNumber ?? string.Empty,
                Roles = roles
            });
        }

        return View(userList);
    }

    // GET: UserManagement/Members Task 9
    public async Task<IActionResult> Members(
        string? search,
        string vehicleFilter = "all",
        string parkingFilter = "all")
    {
        var memberUsers = await _userManager.GetUsersInRoleAsync("Member");

        var memberIds = memberUsers
            .Select(user => user.Id)
            .ToList();

        var membersQuery = _context.Users
            .Where(user => memberIds.Contains(user.Id));

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            membersQuery = membersQuery.Where(user =>
                EF.Functions.Like(user.FirstName, $"%{search}%") ||
                EF.Functions.Like(user.LastName, $"%{search}%") ||
                EF.Functions.Like(user.UserName ?? "", $"%{search}%") ||
                EF.Functions.Like(user.Email ?? "", $"%{search}%") ||
                EF.Functions.Like(user.PersonalIdentityNumber ?? "", $"%{search}%"));
        }

        // Vehicle filter
        if (vehicleFilter == "withVehicles")
        {
            membersQuery = membersQuery.Where(user =>
                _context.Vehicles.Any(vehicle =>
                    vehicle.OwnerId == user.Id));
        }
        else if (vehicleFilter == "withoutVehicles")
        {
            membersQuery = membersQuery.Where(user =>
                !_context.Vehicles.Any(vehicle =>
                    vehicle.OwnerId == user.Id));
        }

        // Active parking filter
        if (parkingFilter == "active")
        {
            membersQuery = membersQuery.Where(user =>
                _context.ParkingSessions.Any(session =>
                    session.Vehicle != null &&
                    session.Vehicle.OwnerId == user.Id &&
                    session.CheckOutTime == null));
        }
        else if (parkingFilter == "inactive")
        {
            membersQuery = membersQuery.Where(user =>
                !_context.ParkingSessions.Any(session =>
                    session.Vehicle != null &&
                    session.Vehicle.OwnerId == user.Id &&
                    session.CheckOutTime == null));
        }
        var members = await membersQuery
            .Select(user => new MemberOverviewViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PersonalIdentityNumber = user.PersonalIdentityNumber ?? string.Empty,

                RegisteredVehiclesCount = _context.Vehicles
                    .Count(vehicle => vehicle.OwnerId == user.Id),

                ActiveParkingTotalCost = _context.ParkingSessions
                    .Where(session =>
                        session.Vehicle != null &&
                        session.Vehicle.OwnerId == user.Id &&
                        session.CheckOutTime == null)
                    .Sum(session =>
                        ((decimal)EF.Functions.DateDiffMinute(
                            session.ArriveTime,
                            DateTime.UtcNow
                        ) / 60m) * session.HourlyRateAtCheckIn)
            })
            .AsNoTracking()
            .ToListAsync();

        ViewData["Search"] = search;
        ViewData["VehicleFilter"] = vehicleFilter;
        ViewData["ParkingFilter"] = parkingFilter;

        return View(members);
    }
    // GET: UserManagement/Details/{id} Task 9
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var member = await _context.Users
            .Where(user => user.Id == id)
            .Select(user => new MemberDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PersonalIdentityNumber = user.PersonalIdentityNumber ?? string.Empty,

                Vehicles = _context.Vehicles
                    .Where(vehicle => vehicle.OwnerId == user.Id)
                    .Select(vehicle => new MemberVehicleViewModel
                    {
                        Id = vehicle.Id,
                        RegistrationNumber = vehicle.RegistrationNumber,
                        VehicleTypeName = vehicle.VehicleTypeRef != null
                            ? vehicle.VehicleTypeRef.Name
                            : "Unknown",
                        Color = vehicle.Color,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        NumberOfWheels = vehicle.NumberOfWheels,
                        ArrivalTime = vehicle.ArrivalTime,
                        ParkingSpotNumber = _context.ParkingSessions
                            .Where(s => s.VehicleId == vehicle.Id && s.CheckOutTime == null)
                            .Select(s => (int?)s.ParkingSpotId)
                            .FirstOrDefault()
                    })
                    .ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }
    // GET: UserManagement/EditRoles/5
    public async Task<IActionResult> EditRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.ToListAsync();

        var model = new ManageUserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Roles = allRoles.Select(role => new RoleSelectionViewModel
            {
                RoleName = role.Name!,
                IsSelected = currentRoles.Contains(role.Name!)
            }).ToList()
        };

        return View(model);
    }

    // POST: UserManagement/EditRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(ManageUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);

        // Security check: Prevent user from modifying their own Admin role status
        if (user.Id == currentUserId)
        {
            var adminRoleSelection = model.Roles.FirstOrDefault(r => r.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            var currentlyIsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (adminRoleSelection != null && adminRoleSelection.IsSelected != currentlyIsAdmin)
            {
                ModelState.AddModelError(string.Empty, "You cannot assign or remove the Admin role for your own account.");
                return View(model);
            }
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);

        // 1. Remove roles no longer selected
        var rolesToRemove = userRoles.Except(selectedRoles);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        // 2. Add newly selected roles (prevents duplicate error)
        var rolesToAdd = selectedRoles.Except(userRoles);
        await _userManager.AddToRolesAsync(user, rolesToAdd);

        TempData["SuccessMessage"] = $"Successfully saved changes to {user.UserName}.";

        return RedirectToAction(nameof(Index));
    }
}