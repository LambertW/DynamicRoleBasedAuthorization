using DynamicRoleBasedAuthorization.OriginalWeb.Models;
using DynamicRoleBasedAuthorization.OriginalWeb.Models.DynamicAuthorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Services
{
    public interface IMvcControllerDiscovery
    {
        IEnumerable<MvcControllerInfo> GetControllers();
    }
}
