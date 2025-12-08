using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.UserHandler
{
    public interface IUserService
    {
        Task<UserModel> ValidateUserAsync(string username, string password);
        Task<bool> CreateUserAsync(IFormCollection collection);
        Task<bool> UpdateUserAsync(IFormCollection collection);
        public List<UserModel> GetAllUsersList();
        public List<UserModel> GetAllActiveUsersList();
        Task<UserModel> GetUserByIdAsync(int id);
        Task<UserModel> GetPagesByUserId(int id);
        Task<bool> CheckUserNameExists(IFormCollection collection);

    }
}
