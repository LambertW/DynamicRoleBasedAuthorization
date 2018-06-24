using DynamicRoleBasedAuthorization.OriginalWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Data
{
    public class SeedData
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SeedData> _logger;
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;

        public SeedData(ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMvcControllerDiscovery mvcControllerDiscovery,
            ILogger<SeedData> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _mvcControllerDiscovery = mvcControllerDiscovery;
            _logger = logger;
        }

        public async Task Initialize()
        {
            _dbContext.Database.EnsureCreated();

            if (_dbContext.Users.Any())
                return;

            await CreateDefaultUserAndRoleForApplication();
        }

        private async Task CreateDefaultUserAndRoleForApplication()
        {
            const string administratorRole = "Administrator";
            const string userName = "admin@demo.com";
            const string defaultPassword = "123qwe!@#QWE";

            await CreateDefaultAdministatorRole(administratorRole);
            var user = await CreateDefaultUser(userName, defaultPassword);

            await AddDefaultRoleToDefaultUser(user, userName, administratorRole);
        }

        private async Task CreateDefaultAdministatorRole(
            string administratorRole)
        {
            _logger.LogInformation($"Create the role `{administratorRole}` for application");
            var role = new IdentityRole(administratorRole);
            var ir = await _roleManager.CreateAsync(role);
            if(ir.Succeeded)
            {
                _logger.LogDebug($"Created the role `{administratorRole}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{administratorRole}` cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            _logger.LogInformation($"Create the role claims for `{administratorRole}` ");
            var controllers = _mvcControllerDiscovery.GetControllers();
            await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Access", JsonConvert.SerializeObject(controllers)));
        }

        private async Task<IdentityUser> CreateDefaultUser(
            string userName, string defaultPassword)
        {
            _logger.LogInformation($"Create default user with userName `{userName}` for application");
            var user = new IdentityUser(userName);

            var result = await _userManager.CreateAsync(user, defaultPassword);
            if(result.Succeeded)
            {
                _logger.LogDebug($"Created default user `{userName}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user `{userName}` cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(result));
                throw exception;
            }

            return user;
        }

        private async Task AddDefaultRoleToDefaultUser(
            IdentityUser defaultUser, 
            string userName, string administratorRole)
        {
            _logger.LogInformation($"Add default user `{userName}` to role `{administratorRole}`");
            var result = await _userManager.AddToRoleAsync(defaultUser, administratorRole);

            if (result.Succeeded)
            {
                _logger.LogDebug($"Added the role `{administratorRole}` to default user `{userName}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRole}` cannot be set for the user `{userName}`");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(result));
                throw exception;
            }
        }

        private string GetIdentityErrorsInCommaSeperatedList(IdentityResult identityResult)
        {
            string errors = null;
            foreach (var identityError in identityResult.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }
            return errors;
        }
    }
}
