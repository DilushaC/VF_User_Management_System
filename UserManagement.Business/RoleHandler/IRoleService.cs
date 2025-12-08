using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.RoleHandler
{
    public interface IRoleService
    {
        Task<bool> CreateRoleAsync(IFormCollection collection);
        public List<RoleModel> GetAllRolesList();
        Task<RoleModel> GetRoleByIdAsync(int id);
        Task<bool> UpdateRoleAsync(IFormCollection collection);
        Task<bool> CheckRoleNameExists(IFormCollection collection);
    }
}
