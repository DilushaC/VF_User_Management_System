using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class PagePermissionModel
    {
        public int PageId { get; set; }
        public string PageUrl { get; set; } = null!;
        public bool CanEdit { get; set; } = false;
    }

}
