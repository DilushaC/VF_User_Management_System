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
        Task<ProductModel> GetProductByIdAsync(int id);
        Task<bool> UpdateProductAsync(IFormCollection collection);
    }
}
