using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.PageHandler
{
    public interface IMenuItemService
    {
        public List<MenuItem> GetAllMenuItemsList();
        public List<MenuCategory> GetAllMenuCategoryList();
        Task<bool> CreateMenuItemAsync(IFormCollection collection);
        public List<MenuItem> GetAllMenuList();
        Task<MenuItem> GetMenuByIdAsync(int id);
        Task<bool> UpdateMenuAsync(IFormCollection collection);
    }
}
