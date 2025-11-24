using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;

namespace UserManagement.Business.ProductHandler
{
    public class ProductService : IProductService
    {
        private readonly _ConnectionService _connectionService;

        public ProductService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }
        public async Task<bool> CreateProductAsync(IFormCollection collection)
        {
            try
            {
                var productName = collection["ProductName"].ToString();
                var description = collection["Description"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Products
                (ProductName, Description, IsActive, CreatedDate)
                VALUES
                (@ProductName, @Description, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();
                parameters.Add("ProductName", productName, DbType.String);
                parameters.Add("Description", description, DbType.String);
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
