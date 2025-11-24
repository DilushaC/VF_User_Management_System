using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class PageModel
    {
        public int Id { get; set; }
        public string PageName { get; set; }
        public int ProductId { get; set; }
        public string PageUrl { get; set; }
        public string PageCode { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
