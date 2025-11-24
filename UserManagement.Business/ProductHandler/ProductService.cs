using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Data.Models;

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

        public List<ProductModel> GetAllProductList()
        {
            try
            {
                string Query = $"SELECT * FROM Products";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<ProductModel> productList = new List<ProductModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    ProductModel bModel = new ProductModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        ProductName = BRow["ProductName"].ToString(),
                        Description = BRow["Description"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    productList.Add(bModel);
                }
                return productList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            try
            {
                string query = @"
                                SELECT 
                                    Id,
                                    ProductName,
                                    Description,
                                    IsActive,
                                    CreatedDate
                                FROM 
                                    Products
                                WHERE 
                                    Id = @Id";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                ProductModel model = new ProductModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString(),
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
                throw new Exception($"Failed to retrieve designation with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateProductAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var productName = collection["ProductName"].ToString();
                var description = collection["Description"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Products
                    SET 
                        ProductName = @ProductName,
                        Description = @Description,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("ProductName", productName, DbType.String);
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

    }
}
