using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.ProductHandler
{
    public interface IProductService
    {
        Task<bool> CreateProductAsync(IFormCollection collection);
        public List<ProductModel> GetAllProductList();
        public List<ProductModel> GetAllActiveProductList();
        Task<ProductModel> GetProductByIdAsync(int id);
        Task<bool> UpdateProductAsync(IFormCollection collection);
        Task<bool> DeleteProductAsync(IFormCollection collection);
        Task<bool> CheckProductNameExists(IFormCollection collection);
    }
}
