using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int? ParentMenuItemId { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public int PageId { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductId { get; set; }
        public List<int> PageIds { get; set; } = new List<int>();
    }
}
