using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class UserRegisterModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int PrimaryBranchId { get; set; }
        public int PrimaryDepartmentId { get; set; }
    }
}
