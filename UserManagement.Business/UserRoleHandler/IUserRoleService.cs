using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.UserRoleHandler
{
    public interface IUserRoleService
    {
        Task<bool> CreateUserRoleAsync(IFormCollection collection);
        public List<UserRoleModel> GetAllUserRolesList();
        Task<UserRoleModel> GetUserRoleByIdAsync(int id);
        Task<bool> UpdateUserRoleAsync(IFormCollection collection);
    }
}
