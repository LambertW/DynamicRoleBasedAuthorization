using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicRoleBasedAuthorization.OriginalWeb.Models.DynamicAuthorization
{
    public class RoleViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "{0}最少需要{1}个字符。")]
        [Display(Name = "角色名称")]
        public string Name { get; set; }

        public IEnumerable<MvcControllerInfo> SelectedControllers { get; set; }
    }
}
