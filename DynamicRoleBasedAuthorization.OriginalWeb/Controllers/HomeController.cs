using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleBasedAuthorization.OriginalWeb.Models;
using DynamicRoleBasedAuthorization.OriginalWeb.Services;
using DynamicRoleBasedAuthorization.OriginalWeb.Data;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IMvcControllerDiscovery mvcControllerDiscovery)
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var seedData = (SeedData)HttpContext.RequestServices.GetService(typeof(SeedData));
            await seedData.Initialize();

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
