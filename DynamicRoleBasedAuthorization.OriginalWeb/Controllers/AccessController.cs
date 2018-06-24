using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicRoleBasedAuthorization.OriginalWeb.Data;
using DynamicRoleBasedAuthorization.OriginalWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Controllers
{
    [DisplayName("Access Management")]
    public class AccessController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccessController(
            ApplicationDbContext dbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [DisplayName("User List")]
        public async Task<IActionResult> Index()
        {
            var query = await (
                from user in _dbContext.Users
                join ur in _dbContext.UserRoles on user.Id equals ur.UserId into UserRoles
                from userRole in UserRoles.DefaultIfEmpty()
                join rle in _dbContext.Roles on userRole.RoleId equals rle.Id into Roles
                from role in Roles.DefaultIfEmpty()
                select new { user, userRole, role }
                ).ToListAsync();

            var userList = new List<UserRoleViewModel>();

            foreach (var grp in query.GroupBy(q => q.user.Id))
            {
                var first = grp.First();
                userList.Add(new UserRoleViewModel
                {
                    UserId = first.user.Id,
                    UserName = first.user.UserName,
                    Roles = first.role != null ? grp.Select(g => g.role).Select(r => r.Name) : new List<string>()
                });
            }

            return View(userList);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userViewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles
            };

            var roles = await _roleManager.Roles.ToListAsync();
            ViewData["Roles"] = roles;

            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRoleViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                ViewData["Roles"] = await _roleManager.Roles.ToListAsync();
                return View(viewModel);
            }

            var user = await _dbContext.Users.FindAsync(viewModel.UserId);
            if(user == null)
            {
                ModelState.AddModelError("", "用户不存在");
                ViewData["Roles"] = await _roleManager.Roles.ToListAsync();
                return View();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRolesAsync(user, viewModel.Roles);

            return RedirectToAction(nameof(Index));
        }
    }
}