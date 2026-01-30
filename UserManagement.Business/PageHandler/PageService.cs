using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Business.Helpers;
using UserManagement.Data.Models;

namespace UserManagement.Business.PageHandler
{
    public class PageService : IPageService
    {
        private readonly _ConnectionService _connectionService;

        public PageService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public List<PageModel> GetAllPagesList()
        {
            try
            {
                string Query = @"
                    SELECT 
                        p.Id,
                        p.PageName,
                        p.ProductId,
                        pr.ProductName,
                        p.IsActive
                    FROM Pages p
                    LEFT JOIN Products pr ON p.ProductId = pr.Id;
                ";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<PageModel> pagesList = new List<PageModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];

                    PageModel bModel = new PageModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        PageName = BRow["PageName"].ToString(),

                        // ✅ ADD THIS
                        ProductId = BRow["ProductId"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(BRow["ProductId"]),

                        ProductName = BRow["ProductName"] == DBNull.Value
                                        ? string.Empty
                                        : BRow["ProductName"].ToString(),

                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                    };

                    pagesList.Add(bModel);
                }

                return pagesList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<PageModel>> GetPagesByProduct(int ProductId)
        {
            try
            {
                string query = @"
                    SELECT 
                        p.Id,
                        p.PageName,
                        p.ProductId,
                        pr.ProductName,
                        p.IsActive
                    FROM Pages p
                    LEFT JOIN Products pr ON p.ProductId = pr.Id
                    WHERE p.ProductId = @Id
                      AND p.IsActive = 1;   -- Only active pages
                ";

                DataTable data = await _connectionService.SingleQueryReturn(query, ProductId);

                List<PageModel> pagesList = new List<PageModel>();

                if (data == null || data.Rows.Count == 0)
                    return pagesList;

                foreach (DataRow row in data.Rows)
                {
                    pagesList.Add(new PageModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        PageName = row["PageName"].ToString(),
                        ProductId = Convert.ToInt32(row["ProductId"]),
                        ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString(),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                return pagesList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading pages list", ex);
            }
        }



        public async Task<bool> CreatePageAsync(IFormCollection collection)
        {
            try
            {
                var pageName = collection["PageName"].ToString();
                var productId = collection["ProductId"].ToString();
                var description = collection["Description"].ToString();
                var pageUrl = collection["PageUrl"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Pages
                (PageName, ProductId, Description, PageUrl, IsActive, CreatedDate )
                VALUES
                (@PageName,@ProductId, @Description, @PageUrl, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("PageName", pageName, DbType.String);
                parameters.Add("ProductId", Convert.ToInt32(productId), DbType.Int32);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("PageUrl", pageUrl, DbType.String);
                parameters.Add("IsActive", isActive, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
            }
        }

        public async Task<PageModel> GetPageByIdAsync(int id)
        {
            try
            {
                string query = @"
                SELECT * FROM Pages
                WHERE Id = @Id;";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                PageModel model = new PageModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    PageName = row["PageName"] == DBNull.Value ? string.Empty : row["PageName"].ToString(),
                    PageUrl = row["PageUrl"] == DBNull.Value ? string.Empty : row["PageUrl"].ToString(),
                    ProductId = row["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProductId"]),
                    Description = row["Description"] == DBNull.Value ? string.Empty : row["Description"].ToString(),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(row["CreatedDate"])
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve page with ID {id}.", ex);
            }
        }


        public async Task<bool> UpdatePageAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var pageName = collection["PageName"].ToString();
                var productId = collection["ProductId"].ToString();
                var pageUrl = collection["PageUrl"].ToString();
                var description = collection["Description"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Pages
                    SET 
                        PageName = @PageName,
                        ProductId = @ProductId,
                        PageUrl = @PageUrl,
                        Description = @Description,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("PageName", pageName, DbType.String);
                parameters.Add("PageUrl", pageUrl, DbType.String);
                parameters.Add("ProductId", Convert.ToInt32(productId), DbType.Int32);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("IsActive", isActive, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CheckPageNameExists(IFormCollection collection)
        {
            try
            {
                var pageName = collection["PageName"].ToString();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Pages
                    WHERE PageName = @PageName
                ";

                var parameters = new DynamicParameters();
                parameters.Add("PageName", pageName, DbType.String);

                // Synchronous call wrapped in Task.FromResult
                var result = _connectionService.ExecuteScalar(sql, parameters);

                int count = Convert.ToInt32(result);

                return await Task.FromResult(count > 0);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> CheckPageLevelExists(IFormCollection collection)
        {
            try
            {
                // Get values from the form and convert to int
                if (!int.TryParse(collection["ProductId"], out int productId))
                    return false;

                if (!int.TryParse(collection["DisplayOrder"], out int displayOrder))
                    return false;

                int id = 0;
                if (int.TryParse(collection["Id"], out int parsedId))
                {
                    id = parsedId;
                }

                // SQL: Check in MenuItems table
                string sql = @"
                        SELECT COUNT(*)
                        FROM MenuItems
                        WHERE ProductId = @ProductId
                        AND DisplayOrder = @DisplayOrder
                    ";

                var parameters = new DynamicParameters();
                parameters.Add("@ProductId", productId, DbType.Int32);
                parameters.Add("@DisplayOrder", displayOrder, DbType.Int32);

                // Execute scalar synchronously wrapped in Task.Run for async
                var count = await Task.Run(() =>
                {
                    var result = _connectionService.ExecuteScalar(sql, parameters);
                    return Convert.ToInt32(result);
                });

                return count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<bool> CheckPageLevelForExistingMenu(IFormCollection collection)
        {
            try
            {
                if (!int.TryParse(collection["ProductId"], out int productId))
                    return false;

                if (!int.TryParse(collection["DisplayOrder"], out int displayOrder))
                    return false;

                int id = 0;
                int.TryParse(collection["Id"], out id);

                // 🔹 STEP 1: Check SAME RECORD (Id + ProductId + DisplayOrder)
                string sameRecordSql = @"
                    SELECT COUNT(*)
                    FROM MenuItems
                    WHERE Id = @Id
                      AND ProductId = @ProductId
                      AND DisplayOrder = @DisplayOrder
                ";

                var sameParams = new DynamicParameters();
                sameParams.Add("@Id", id);
                sameParams.Add("@ProductId", productId);
                sameParams.Add("@DisplayOrder", displayOrder);

                int sameCount = Convert.ToInt32(
                    await Task.Run(() => _connectionService.ExecuteScalar(sameRecordSql, sameParams))
                );

                // ✅ Same record → allow update
                if (sameCount > 0)
                    return false;

                // 🔹 STEP 2: Check DUPLICATE (same product + display order, different Id)
                string duplicateSql = @"
                    SELECT COUNT(*)
                    FROM MenuItems
                    WHERE ProductId = @ProductId
                      AND DisplayOrder = @DisplayOrder
                      AND Id <> @Id
                ";

                var dupParams = new DynamicParameters();
                dupParams.Add("@ProductId", productId);
                dupParams.Add("@DisplayOrder", displayOrder);
                dupParams.Add("@Id", id);

                int dupCount = Convert.ToInt32(
                    await Task.Run(() => _connectionService.ExecuteScalar(duplicateSql, dupParams))
                );

                // ❌ Duplicate exists → block
                return dupCount > 0;
            }
            catch
            {
                return false;
            }
        }




    }
}
