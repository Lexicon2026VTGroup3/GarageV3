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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
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