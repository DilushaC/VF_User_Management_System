using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Business.PageHandler
{
    public interface IPageService
    {
        Task<bool> CreatePageAsync(IFormCollection collection);
    }
}
