using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsAdminAccount { get; set; }
    }
}
