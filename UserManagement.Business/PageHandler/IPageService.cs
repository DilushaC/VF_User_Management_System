using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.PageHandler
{
    public interface IPageService
    {
        Task<bool> CreatePageAsync(IFormCollection collection);
        public List<PageModel> GetAllPagesList();
        Task<PageModel> GetPageByIdAsync(int id);
    }
}
