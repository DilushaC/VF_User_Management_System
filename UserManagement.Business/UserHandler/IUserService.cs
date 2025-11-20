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
    }
}
