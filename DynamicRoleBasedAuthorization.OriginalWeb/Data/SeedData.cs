using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Data
{
    public class SeedData
    {
        private ApplicationDbContext _dbContext;
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ILogger<SeedData> _logger;

        public SeedData(ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SeedData> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task Initialize()
        {
            _dbContext.Database.EnsureCreated();

            if (_dbContext.Users.Any())
                return;

            await CreateDefaultUserAndRoleForApplication(_userManager, _roleManager, _logger);
        }

        private async Task CreateDefaultUserAndRoleForApplication(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SeedData> logger)
        {
            const string administratorRole = "Administrator";
            const string userName = "admin@demo.com";
            const string defaultPassword = "123qwe!@#QWE";

            await CreateDefaultAdministatorRole(roleManager, logger, administratorRole);
            var user = await CreateDefaultUser(userManager, logger, userName, defaultPassword);

            await AddDefaultRoleToDefaultUser(userManager, logger, user, userName, administratorRole);
        }

        private async Task CreateDefaultAdministatorRole(
            RoleManager<IdentityRole> roleManager, 
            ILogger<SeedData> logger,
            string administratorRole)
        {
            logger.LogInformation($"Create the role `{administratorRole}` for application");
            var ir = await roleManager.CreateAsync(new IdentityRole(administratorRole));
            if(ir.Succeeded)
            {
                logger.LogDebug($"Created the role `{administratorRole}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{administratorRole}` cannot be created");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private async Task<IdentityUser> CreateDefaultUser(
            UserManager<IdentityUser> userManager, ILogger<SeedData> logger,
            string userName, string defaultPassword)
        {
            logger.LogInformation($"Create default user with userName `{userName}` for application");
            var user = new IdentityUser(userName);

            var result = await userManager.CreateAsync(user, defaultPassword);
            if(result.Succeeded)
            {
                logger.LogDebug($"Created default user `{userName}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user `{userName}` cannot be created");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(result));
                throw exception;
            }

            return user;
        }

        private async Task AddDefaultRoleToDefaultUser(
            UserManager<IdentityUser> userManager, ILogger<SeedData> logger,
            IdentityUser defaultUser, 
            string userName, string administratorRole)
        {
            logger.LogInformation($"Add default user `{userName}` to role `{administratorRole}`");
            var result = await userManager.AddToRoleAsync(defaultUser, administratorRole);

            if (result.Succeeded)
            {
                logger.LogDebug($"Added the role `{administratorRole}` to default user `{userName}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRole}` cannot be set for the user `{userName}`");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(result));
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
