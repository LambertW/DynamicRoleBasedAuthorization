using DynamicRoleBasedAuthorization.OriginalWeb.Data;
using DynamicRoleBasedAuthorization.OriginalWeb.Models;
using DynamicRoleBasedAuthorization.OriginalWeb.Models.DynamicAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Filters
{
    public class DynamicAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DynamicAuthorizationFilter(ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!IsProtectedAction(context))
                return;

            if(!IsUserAuthenticated(context))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var actionId = GetActionId(context);
            var userName = context.HttpContext.User.Identity.Name;

            var user = await _userManager.FindByNameAsync(
                context.HttpContext.User.Identity.Name);
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var claims = await _roleManager.GetClaimsAsync(role);
                if (!claims.Any(t => t.Type == "Access"))
                    continue;

                var accessClaim = claims.First(t => t.Type == "Access");
                var accessList = JsonConvert.DeserializeObject<IEnumerable<MvcControllerInfo>>(accessClaim.Value);
                if (accessList.SelectMany(c => c.Actions).Any(a => a.Id == actionId))
                    return;
            }

            context.Result = new ForbidResult();
        }

        private string GetActionId(AuthorizationFilterContext context)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var area = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue;
            var controller = controllerActionDescriptor.ControllerName;
            var action = controllerActionDescriptor.ActionName;

            return $"{area}:{controller}:{action}";
        }

        private bool IsUserAuthenticated(AuthorizationFilterContext context)
        {
            return context.HttpContext.User.Identity.IsAuthenticated;
        }

        private bool IsProtectedAction(AuthorizationFilterContext context)
        {
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
                return false;

            if (!(context.ActionDescriptor is ControllerActionDescriptor))
                return false;

            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
            var actionMethodInfo = controllerActionDescriptor.MethodInfo;

            var authroizeAttribute = controllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>();
            if (authroizeAttribute != null)
                return true;

            authroizeAttribute = actionMethodInfo.GetCustomAttribute<AuthorizeAttribute>();
            if (authroizeAttribute != null)
                return true;

            return false;
        }


    }
}
