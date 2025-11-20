using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class BranchModel
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
