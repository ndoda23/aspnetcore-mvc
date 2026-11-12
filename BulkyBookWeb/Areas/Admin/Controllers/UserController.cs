using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _auditLogService;

        public UserController(IApplicationUserService applicationUserService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuditLogService auditLogService)
        {
            _applicationUserService = applicationUserService;
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RoleManagement(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var vm = new RoleManagementVM
            {
                ApplicationUser = user,
                RoleList = _roleManager.Roles.Select(r => new SelectListItem { 
                    Text= r.Name,
                    Value=r.Name
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> RoleManagement(RoleManagementVM vm)
        {

            var user = await _userManager.FindByIdAsync(vm.ApplicationUser.Id);
            if (user == null) return NotFound();

            var oldRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var newRole = vm.ApplicationUser.Role;
            if (!string.IsNullOrEmpty(newRole) && oldRole != newRole)
            {
                if (oldRole != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRole);
                }
                await _userManager.AddToRoleAsync(user, newRole);

                await _auditLogService.LogAsync(User, SD.AuditRoleChanged, SD.EntityUser,
                     user.Id, oldRole, newRole);
            }

            TempData["success"] = "Role updated successfully";
            return RedirectToAction(nameof(Index));


        }

        public async Task<IActionResult> UserPasswordChange(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user== null)
            {
                return NotFound();
            }
            var vm = new AdminChangePasswordVM
            {
                UserId = user.Id,
                UserEmail = user.Email
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UserPasswordChange(AdminChangePasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if(user== null ) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }
            await _auditLogService.LogAsync(User, SD.AuditPasswordReset, SD.EntityUser,
                  user.Id, null, user.Email);

            TempData["success"] = "Password changed successfully";
            return RedirectToAction(nameof(Index));

        }

        #region API CALLS 

        public async Task<IActionResult> GetAll()
        {
            var users = await _applicationUserService.GetAllUsersAsync();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    role = roles.FirstOrDefault(),
                    isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return Json(new { data = result });
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock(string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Json(new { success = false, message = "You cannot lock yourself" });

            bool isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user,
                isLocked ? DateTimeOffset.UtcNow                    // unlock
                         : DateTimeOffset.UtcNow.AddYears(100));    // lock

            await _auditLogService.LogAsync(User,
                isLocked ? SD.AuditUserUnlocked : SD.AuditUserLocked,
                SD.EntityUser, user.Id, null, user.Email);

            return Json(new { success = true, message = isLocked ? "User unlocked" : "User locked" });
        }

        #endregion
    }
}
