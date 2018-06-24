using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Models.DynamicAuthorization
{
    public class UserRoleViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }

        public IEnumerable<string> Roles { get; set; }

    }
}
