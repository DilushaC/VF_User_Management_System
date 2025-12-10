using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class MenuItemModel
    {
        public string MenuTitle { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Permission { get; set; } = "false"; 
    }

}
