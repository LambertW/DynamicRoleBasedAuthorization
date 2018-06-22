using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicRoleBasedAuthorization.OriginalWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Controllers
{
    public class RoleController : Controller
    {
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;

        public RoleController(IMvcControllerDiscovery mvcControllerDiscovery)
        {
            _mvcControllerDiscovery = mvcControllerDiscovery;
        }

        public ActionResult Create()
        {
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

            return View();
        }
    }
}