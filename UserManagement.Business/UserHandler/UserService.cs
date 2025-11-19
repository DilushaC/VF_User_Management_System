using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Business.Helpers;
using UserManagement.Data.Models;

namespace UserManagement.Business.UserHandler
{
    public class UserService : IUserService
    {
        private readonly _ConnectionService _connectionService;
        private readonly PasswordHelper _passwordHelper;

        public UserService(_ConnectionService connectionService, PasswordHelper passwordHelper)
        {
            _connectionService = connectionService;
            _passwordHelper = passwordHelper;
        }

        public async Task<UserModel?> ValidateUserAsync(string username, string password)
        {
            const string query = @"
                                    SELECT * 
                                    FROM Users 
                                    WHERE UserName = @UserName AND IsActive = 1";

            var parameters = new DynamicParameters();
            parameters.Add("@UserName", username);
            var users = _connectionService.ReturnWithPara(query, parameters)
                                          .AsEnumerable()
                                          .Select(row => new UserModel
                                          {
                                              Id = row.Field<int>("Id"),
                                              UserName = row.Field<string>("UserName"),
                                              Password = row.Field<string>("Password"),
                                              FirstName = row.Field<string>("FirstName"),
                                              LastName = row.Field<string>("LastName"),
                                              Email = row.Field<string>("Email"),
                                              Phone = row.Field<string>("Phone"),
                                              PrimaryBranchId = row.Field<int>("PrimaryBranchId"),
                                              PrimaryDepartmentId = row.Field<int>("PrimaryDepartmentId"),
                                              DesignationId = row.Field<int>("DesignationId"),
                                              IsActive = row.Field<bool>("IsActive"),
                                              CreatedDate = row.Field<DateTime?>("CreatedDate"),
                                              LastLoginDate = row.Field<DateTime?>("LastLoginDate")
                                          })
                        .ToList();

            var user = users.FirstOrDefault();
            if (user == null)
                return null;

            bool isValid = _passwordHelper.VerifyPassword(password, user.Password);
            return isValid ? user : null;
        }


    }
}
