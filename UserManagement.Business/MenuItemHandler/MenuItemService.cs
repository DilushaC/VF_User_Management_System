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
                string menuTitle = collection["MenuTitle"];
                string parentIdStr = collection["ParentMenuItemId"];
                string pageIdStr = collection["PageId"];
                string iconClass = collection["IconClass"];
                string displayOrderStr = collection["DisplayOrder"];
                string productIdStr = collection["ProductId"];
                string isActiveStr = collection["IsActive"];
                string isMainMenuStr = collection["IsMainMenu"];

                bool isMainMenu = isMainMenuStr == "on" || isMainMenuStr == "true";

                int displayOrder = int.TryParse(displayOrderStr, out var d) ? d : 1;
                int productId = int.TryParse(productIdStr, out var p) ? p : 0;
                bool isActive = isActiveStr == "true" || isActiveStr == "on";

                int? parentMenuId = null;
                int? pageId = null;  

                if (isMainMenu)
                {
                    parentMenuId = null;
                    pageId = null;    
                    iconClass = string.IsNullOrWhiteSpace(iconClass) ? null : iconClass;
                }
                else
                {
                    parentMenuId = int.TryParse(parentIdStr, out var pid) ? pid : null;
                    pageId = int.TryParse(pageIdStr, out var pgid) ? pgid : null;
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
                parameters.Add("@PageId", pageId, DbType.Int32);   // NULL allowed
                parameters.Add("@IconClass", iconClass, DbType.String);
                parameters.Add("@DisplayOrder", displayOrder, DbType.Int32);
                parameters.Add("@ProductId", productId == 0 ? null : productId, DbType.Int32);
                parameters.Add("@IsActive", isActive, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);
                return rows > 0;
            }
            catch
            {
                throw;
            }
        }


        public List<MenuItem> GetAllMenuList()
        {
            try
            {
                string query = @"
                SELECT  
                    m.Id,
                    m.MenuTitle,
                    m.ParentMenuId,
                    pm.MenuTitle AS ParentMenuTitle,
                    m.PageId,
                    ISNULL(p.PageName, '') AS PageName,
                    m.IconClass,
                    ISNULL(m.DisplayOrder, 0) AS DisplayOrder,
                    m.IsActive,
                    m.ProductId,
                    ISNULL(pr.ProductName, '') AS ProductName
                FROM MenuItems m
                LEFT JOIN MenuItems pm ON m.ParentMenuId = pm.Id   -- Self join to get parent title
                LEFT JOIN Pages p ON m.PageId = p.Id
                LEFT JOIN Products pr ON m.ProductId = pr.Id

                ";

                var data = _connectionService.Return(query);

                List<MenuItem> menuList = new List<MenuItem>();

                if (data == null || data.Rows.Count == 0)
                    return menuList;

                foreach (DataRow row in data.Rows)
                {
                    menuList.Add(new MenuItem
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        MenuTitle = row["MenuTitle"]?.ToString() ?? string.Empty,
                        ParentMenuItemId = row["ParentMenuId"] != DBNull.Value
                        ? Convert.ToInt32(row["ParentMenuId"])
                        : (int?)null,
                        ParentMenuTitle = row.Table.Columns.Contains("ParentMenuTitle") && row["ParentMenuTitle"] != DBNull.Value
                        ? row["ParentMenuTitle"].ToString()
                        : string.Empty,

                        PageId = row["PageId"] != DBNull.Value
                                            ? Convert.ToInt32(row["PageId"])
                                            : (int?)null,
                        PageName = row["PageName"]?.ToString() ?? string.Empty,
                        IconClass = row["IconClass"]?.ToString() ?? string.Empty,
                        DisplayOrder = row["DisplayOrder"] != DBNull.Value
                                            ? Convert.ToInt32(row["DisplayOrder"])
                                            : 0,
                        IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                        ProductId = row["ProductId"] != DBNull.Value
                                            ? Convert.ToInt32(row["ProductId"])
                                            : (int?)null,
                        ProductName = row["ProductName"]?.ToString() ?? string.Empty
                    });
                }

                return menuList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllMenuList: " + ex.Message);
                throw;
            }
        }

        public async Task<MenuItem> GetMenuByIdAsync(int id)
        {
            try
            {
                string query = @"
                SELECT * FROM MenuItems
                WHERE Id = @Id;";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                MenuItem model = new MenuItem()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    MenuTitle = row["MenuTitle"] == DBNull.Value ? string.Empty : row["MenuTitle"].ToString(),
                    ParentMenuItemId = row["ParentMenuId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ParentMenuId"]),
                    PageId = row["PageId"] == DBNull.Value ? 0 : Convert.ToInt32(row["PageId"]),
                    IconClass = row["IconClass"] == DBNull.Value ? string.Empty : row["IconClass"].ToString(),
                    DisplayOrder = row["DisplayOrder"] == DBNull.Value ? 0 : Convert.ToInt32(row["DisplayOrder"]),
                    ProductId = row["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProductId"]),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve page with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateMenuAsync(IFormCollection collection)
        {
            try
            {
                int id = Convert.ToInt32(collection["Id"]);
                string menuTitle = collection["MenuTitle"];
                string parentMenuStr = collection["ParentMenuItemId"];
                string pageIdStr = collection["PageId"];
                string iconClass = collection["IconClass"];
                string displayOrderStr = collection["DisplayOrder"];
                string productIdStr = collection["ProductId"];
                bool isActive = collection["IsActive"] == "true" || collection["IsActive"] == "on";

                int displayOrder = int.TryParse(displayOrderStr, out var d) ? d : 1;

                int? parentMenuId = null;
                int? pageId = null;
                int? productId = null;

                if (int.TryParse(parentMenuStr, out var pmid))
                    parentMenuId = pmid;

                if (int.TryParse(pageIdStr, out var pgid))
                    pageId = pgid;

                if (int.TryParse(productIdStr, out var prid))
                    productId = prid;

                string sql = @"
                    UPDATE MenuItems
                    SET 
                        MenuTitle = @MenuTitle,
                        ParentMenuId = @ParentMenuId,
                        PageId = @PageId,
                        IconClass = @IconClass,
                        DisplayOrder = @DisplayOrder,
                        ProductId = @ProductId,
                        IsActive = @IsActive
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("@Id", id, DbType.Int32);
                parameters.Add("@MenuTitle", menuTitle, DbType.String);
                parameters.Add("@ParentMenuId", parentMenuId, DbType.Int32);
                parameters.Add("@PageId", pageId, DbType.Int32);
                parameters.Add("@IconClass", string.IsNullOrWhiteSpace(iconClass) ? null : iconClass, DbType.String);
                parameters.Add("@DisplayOrder", displayOrder, DbType.Int32);
                parameters.Add("@ProductId", productId, DbType.Int32);
                parameters.Add("@IsActive", isActive, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);
                return rows > 0;
            }
            catch
            {
                return false;
            }
        }

    }
}
