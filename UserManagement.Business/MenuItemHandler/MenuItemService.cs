using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<bool> CreateMenuItemAsync(IFormCollection collection)
        {
            try
            {
                // ===== READ VALUES =====
                string menuTitle = collection["MenuTitle"];
                string parentIdStr = collection["ParentMenuId"];
                string pageIdStr = collection["PageId"];
                string iconClass = collection["IconClass"];
                string displayOrderStr = collection["DisplayOrder"];
                string productIdStr = collection["ProductId"];
                string isActiveStr = collection["IsActive"];
                string isMainMenuStr = collection["IsMainMenu"];

                bool isMainMenu = isMainMenuStr == "on" || isMainMenuStr == "true";

                // ===== PARSE SAFELY =====
                int displayOrder = int.TryParse(displayOrderStr, out var d) ? d : 1;
                int productId = int.TryParse(productIdStr, out var p) ? p : 0;
                bool isActive = isActiveStr == "true" || isActiveStr == "on";

                int? parentMenuId = null;
                int pageId;

                if (isMainMenu)
                {
                    parentMenuId = null;
                    pageId = 1;                // Forced
                    iconClass = string.IsNullOrWhiteSpace(iconClass) ? null : iconClass;
                }
                else
                {
                    parentMenuId = int.TryParse(parentIdStr, out var pid) ? pid : null;
                    pageId = int.TryParse(pageIdStr, out var pgid) ? pgid : 0;
                    iconClass = null;
                }

                string sql = @"
                    INSERT INTO MenuItems
                    (MenuTitle, ParentMenuId, PageId, IconClass, DisplayOrder, ProductId, IsActive)
                    VALUES
                    (@MenuTitle, @ParentMenuId, @PageId, @IconClass, @DisplayOrder, @ProductId, @IsActive);
                ";

                var parameters = new DynamicParameters();
                parameters.Add("@MenuTitle", menuTitle, DbType.String);
                parameters.Add("@ParentMenuId", parentMenuId, DbType.Int32);
                parameters.Add("@PageId", pageId, DbType.Int32);
                parameters.Add("@IconClass", iconClass, DbType.String);
                parameters.Add("@DisplayOrder", displayOrder, DbType.Int32);
                parameters.Add("@ProductId", productId, DbType.Int32);
                parameters.Add("@IsActive", isActive, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                // 🔥 DO NOT SWALLOW ERRORS
                // Log this properly
                // _logger.LogError(ex, "Error creating menu item");
                throw;
            }
        }


    }
}
