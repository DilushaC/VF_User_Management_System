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
                        p.PageCode,
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
                        ProductName = BRow["ProductName"] == DBNull.Value ? string.Empty : BRow["ProductName"].ToString(),
                        PageCode = BRow["PageCode"].ToString(),
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

        public async Task<bool> CreatePageAsync(IFormCollection collection)
        {
            try
            {
                var pageName = collection["PageName"].ToString();
                var productId = collection["ProductId"].ToString();
                var description = collection["Description"].ToString();
                var pageUrl = collection["PageUrl"].ToString();
                var pageCode = collection["PageCode"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Pages
                (PageName, ProductId, Description, PageUrl, PageCode, IsActive, CreatedDate )
                VALUES
                (@PageName,@ProductId, @Description, @PageUrl, @PageCode, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("PageName", pageName, DbType.String);
                parameters.Add("ProductId", Convert.ToInt32(productId), DbType.Int32);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("PageUrl", pageUrl, DbType.String);
                parameters.Add("PageCode", pageCode, DbType.String);
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

    }
}
