using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.RolePagePermission
{
    public interface IRolePagePermissionService
    {
        Task<bool> CreateRolePagePermissionAsync(IFormCollection collection);
        public List<RolePagePermissionModel> GetAllRolePagePermissionList();
        Task<RolePagePermissionModel> GetRolePagePermissionByIdAsync(int id);
    }
}
