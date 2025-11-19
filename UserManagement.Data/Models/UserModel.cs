using System;

namespace UserManagement.Data.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!; 

        public string FirstName { get; set; } = null!; 

        public string LastName { get; set; } = null!; 

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!; 

        public int PrimaryBranchId { get; set; }

        public int PrimaryDepartmentId { get; set; }

        public int DesignationId { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedDate { get; set; } // nullable

        public DateTime? LastLoginDate { get; set; } // nullable
    }
}
