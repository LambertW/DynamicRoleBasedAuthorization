using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicRoleBasedAuthorization.OriginalWeb.Models;
using DynamicRoleBasedAuthorization.OriginalWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(IMvcControllerDiscovery mvcControllerDiscovery,
            RoleManager<IdentityRole> roleManager
            )
        {
            _mvcControllerDiscovery = mvcControllerDiscovery;
            _roleManager = roleManager;
        }

        [DisplayName("List")]
        public async Task<ActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            return View(roles);
        }

        public ActionResult Create()
        {
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
                return View(viewModel);
            }

            var role = new IdentityRole { Name = viewModel.Name };
            var result = await _roleManager.CreateAsync(role);
            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

                return View(viewModel);
            }

            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                foreach (var controller in viewModel.SelectedControllers)
                {
                    foreach (var action in controller.Actions)
                    {
                        action.ControllerId = controller.Id;
                    }
                }

                var accessJson = JsonConvert.SerializeObject(viewModel.SelectedControllers);
                var claimResult = await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Access", accessJson));

                if (!claimResult.Succeeded)
                {
                    foreach (var error in claimResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

                    return View(viewModel);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var claims = await _roleManager.GetClaimsAsync(role);

            var viewModel = new RoleViewModel
            {
                Name = role.Name,
                SelectedControllers = claims.Any(t => t.Type == "Access") ?
                    JsonConvert.DeserializeObject<IEnumerable<MvcControllerInfo>>(claims.First(t => t.Type == "Access").Value)
                    : new List<MvcControllerInfo>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, RoleViewModel viewModel)
        {
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                ModelState.AddModelError("", "角色不存在");
                return View();
            }

            role.Name = viewModel.Name;
            await _roleManager.UpdateAsync(role);

            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                foreach (var controller in viewModel.SelectedControllers)
                {
                    foreach (var action in controller.Actions)
                    {
                        action.ControllerId = controller.Id;
                    }
                }

                var accessJson = JsonConvert.SerializeObject(viewModel.SelectedControllers);

                var existClaims = await _roleManager.GetClaimsAsync(role);
                if(existClaims.Any(c => c.Type == "Access"))
                {
                    await _roleManager.RemoveClaimAsync(role, existClaims.First(c => c.Type == "Access"));
                }
                var newClaim = new System.Security.Claims.Claim("Access", accessJson);
                var claimResult = await _roleManager.AddClaimAsync(role, newClaim);
                

                if (!claimResult.Succeeded)
                {
                    foreach (var error in claimResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(viewModel);
                }
            }

            return RedirectToAction(nameof(Index));
        }

    }
}