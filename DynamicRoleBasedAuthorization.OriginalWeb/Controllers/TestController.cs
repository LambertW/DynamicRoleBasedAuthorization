using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize]
        public IActionResult Get()
        {
            return new ObjectResult(new string[] { "a", "b", "c" });
        }

        [Authorize]
        [Route("Get1")]
        public IActionResult Get1()
        {
            return new ObjectResult(new string[] { "a", "b", "c" });
        }
    }
}