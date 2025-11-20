using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        public async Task<bool> CreateUserAsync(IFormCollection collection)
        {
            try
            {
                // Extract values from collection
                var userName = collection["UserName"].ToString();
                var firstName = collection["FirstName"].ToString();
                var lastName = collection["LastName"].ToString();
                var email = collection["Email"].ToString();
                var phone = collection["Phone"].ToString();
                var designationId = collection["DesignationId"].ToString();
                var primaryBranchId = collection["PrimaryBranchId"].ToString();
                var primaryDepartmentId = collection["PrimaryDepartmentId"].ToString();

                var password = "1234";
                string encryptedPassword = _passwordHelper.ComputeHmac(password);

                string sql = @"
                INSERT INTO Users
                (UserName, Password, FirstName, LastName, Email, Phone, DesignationId, PrimaryBranchId, PrimaryDepartmentId, IsActive, CreatedDate)
                VALUES
                (@UserName, @Password, @FirstName, @LastName, @Email, @Phone, @DesignationId, @PrimaryBranchId, @PrimaryDepartmentId,@IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();
                parameters.Add("UserName", userName, DbType.String);
                parameters.Add("Password", encryptedPassword, DbType.String);
                parameters.Add("FirstName", firstName, DbType.String);
                parameters.Add("LastName", lastName, DbType.String);
                parameters.Add("Email", email, DbType.String);
                parameters.Add("Phone", phone, DbType.String);
                parameters.Add("DesignationId", Convert.ToInt32(designationId), DbType.Int32);
                parameters.Add("PrimaryBranchId", Convert.ToInt32(primaryBranchId), DbType.Int32);
                parameters.Add("PrimaryDepartmentId", Convert.ToInt32(primaryDepartmentId), DbType.Int32);
                parameters.Add("IsActive", true, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
            }
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
