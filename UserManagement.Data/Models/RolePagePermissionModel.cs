using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class RolePagePermissionModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PageId { get; set; }
        public bool CanEdit { get; set; }
        public string RoleName { get; set; }
        public string PageName { get; set; }
    }
}
