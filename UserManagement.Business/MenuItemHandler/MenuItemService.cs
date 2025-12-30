using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Business.PageHandler;
using UserManagement.Data.Models;

namespace UserManagement.Business.MenuItemHandler
{
    public class MenuItemService : IMenuItemService
    {
        private readonly _ConnectionService _connectionService;

        public MenuItemService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }
        public List<MenuItem> GetAllMenuItemsList()
        {
            try
            {
                string query = @"
                    SELECT 
                        Id,
                        MenuTitle,
                        ParentMenuId,
                        PageId,
                        IconClass,
                        DisplayOrder,
                        IsActive,
                        ProductId
                    FROM MenuItems;
                ";

                var data = _connectionService.Return(query);

                List<MenuItem> menuItems = new List<MenuItem>();

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var row = data.Rows[i];

                    MenuItem model = new MenuItem()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        MenuTitle = row["MenuTitle"].ToString(),
                        ParentMenuItemId = row["ParentMenuId"] == DBNull.Value
                                        ? null
                                        : Convert.ToInt32(row["ParentMenuId"]),
                        PageId = row["PageId"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(row["PageId"]),
                        IconClass = row["IconClass"] == DBNull.Value
                                        ? string.Empty
                                        : row["IconClass"].ToString(),
                        DisplayOrder = row["DisplayOrder"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(row["DisplayOrder"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        ProductId = Convert.ToInt32(row["ProductId"])
                    };

                    menuItems.Add(model);
                }

                return menuItems;
            }
            catch
            {
                throw;
            }
        }

    }
}
