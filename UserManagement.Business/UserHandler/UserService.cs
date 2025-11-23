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
using UserManagement.Business.DatatableHandler;
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

        public List<UserModel> GetAllUsersList()
        {
            try
            {
                string Query = @"
                                SELECT 
                                    U.*, 
                                    B.BranchName AS PrimaryBranchName, 
                                    D.DepartmentName AS PrimaryDepartmentName, 
                                    G.DesignationName
                                FROM 
                                    Users U
                                INNER JOIN 
                                    Branch B ON U.PrimaryBranchId = B.Id
                                INNER JOIN 
                                    Department D ON U.PrimaryDepartmentId = D.Id
                                INNER JOIN 
                                    Designation G ON U.DesignationId = G.Id";

                var Data = _connectionService.Return(Query);

                List<UserModel> usersList = new List<UserModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    UserModel usersModel = new UserModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        UserName = BRow["UserName"] == DBNull.Value ? string.Empty : BRow["UserName"].ToString(),
                        FirstName = BRow["FirstName"] == DBNull.Value ? string.Empty : BRow["FirstName"].ToString(),
                        LastName = BRow["LastName"] == DBNull.Value ? string.Empty : BRow["LastName"].ToString(),
                        Email = BRow["Email"] == DBNull.Value ? string.Empty : BRow["Email"].ToString(),
                        Phone = BRow["Phone"] == DBNull.Value ? string.Empty : BRow["Phone"].ToString(),
                        PrimaryBranchId = BRow["PrimaryBranchId"] == DBNull.Value ? 0 : (int)BRow["PrimaryBranchId"],
                        PrimaryDepartmentId = BRow["PrimaryDepartmentId"] == DBNull.Value ? 0 : (int)BRow["PrimaryDepartmentId"],
                        DesignationId = BRow["DesignationId"] == DBNull.Value ? 0 : (int)BRow["DesignationId"],
                        PrimaryBranchName = BRow["PrimaryBranchName"] == DBNull.Value ? string.Empty : BRow["PrimaryBranchName"].ToString(),
                        PrimaryDepartmentName = BRow["PrimaryDepartmentName"] == DBNull.Value ? string.Empty : BRow["PrimaryDepartmentName"].ToString(),
                        DesignationName = BRow["DesignationName"] == DBNull.Value ? string.Empty : BRow["DesignationName"].ToString(),

                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),

                        LastLoginDate = BRow["LastLoginDate"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(BRow["LastLoginDate"]),
                    };
                    usersList.Add(usersModel);
                }
                return usersList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserModel> GetUserByIdAsync(int id)
        {
            try
            {
                string Query = @"
                SELECT 
                    U.*, 
                    B.BranchName AS PrimaryBranchName, 
                    D.DepartmentName AS PrimaryDepartmentName, 
                    G.DesignationName
                FROM 
                    Users U
                LEFT JOIN 
                    Branch B ON U.PrimaryBranchId = B.Id
                LEFT JOIN 
                    Department D ON U.PrimaryDepartmentId = D.Id
                LEFT JOIN 
                    Designation G ON U.DesignationId = G.Id
                WHERE 
                    U.Id = @Id"; 

                DataTable Data = await _connectionService.SingleQueryReturn(Query, id);

                if (Data == null || Data.Rows.Count == 0)
                {
                    return null; 
                }

                DataRow BRow = Data.Rows[0];

                UserModel userModel = new UserModel()
                {
                    Id = Convert.ToInt32(BRow["Id"]),
                    UserName = BRow["UserName"] == DBNull.Value ? string.Empty : BRow["UserName"].ToString(),
                    FirstName = BRow["FirstName"] == DBNull.Value ? string.Empty : BRow["FirstName"].ToString(),
                    LastName = BRow["LastName"] == DBNull.Value ? string.Empty : BRow["LastName"].ToString(),
                    Email = BRow["Email"] == DBNull.Value ? string.Empty : BRow["Email"].ToString(),
                    Phone = BRow["Phone"] == DBNull.Value ? string.Empty : BRow["Phone"].ToString(),

                    PrimaryBranchId = BRow["PrimaryBranchId"] == DBNull.Value ? 0 : (int)BRow["PrimaryBranchId"],
                    PrimaryDepartmentId = BRow["PrimaryDepartmentId"] == DBNull.Value ? 0 : (int)BRow["PrimaryDepartmentId"],
                    DesignationId = BRow["DesignationId"] == DBNull.Value ? 0 : (int)BRow["DesignationId"],

                    PrimaryBranchName = BRow["PrimaryBranchName"] == DBNull.Value ? string.Empty : BRow["PrimaryBranchName"].ToString(),
                    PrimaryDepartmentName = BRow["PrimaryDepartmentName"] == DBNull.Value ? string.Empty : BRow["PrimaryDepartmentName"].ToString(),
                    DesignationName = BRow["DesignationName"] == DBNull.Value ? string.Empty : BRow["DesignationName"].ToString(),

                    IsActive = Convert.ToBoolean(BRow["IsActive"]),
                    CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),

                    LastLoginDate = BRow["LastLoginDate"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(BRow["LastLoginDate"]),
                };

                return userModel;
            }
            catch (Exception ex)
            {
                // Log the exception (recommended)
                throw new Exception($"Failed to retrieve user with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateUserAsync(IFormCollection collection)
        {
            try
            {
                var userId = Convert.ToInt32(collection["Id"]);
                var userName = collection["UserName"].ToString();
                var firstName = collection["FirstName"].ToString();
                var lastName = collection["LastName"].ToString();
                var email = collection["Email"].ToString();
                var phone = collection["Phone"].ToString();
                var designationId = collection["DesignationId"].ToString();
                var primaryBranchId = collection["PrimaryBranchId"].ToString();
                var primaryDepartmentId = collection["PrimaryDepartmentId"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Users
                    SET 
                        UserName = @UserName,
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        Phone = @Phone,
                        DesignationId = @DesignationId,
                        PrimaryBranchId = @PrimaryBranchId,
                        PrimaryDepartmentId = @PrimaryDepartmentId,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @UserId;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId, DbType.Int32);
                parameters.Add("UserName", userName, DbType.String);
                parameters.Add("FirstName", firstName, DbType.String);
                parameters.Add("LastName", lastName, DbType.String);
                parameters.Add("Email", email, DbType.String);
                parameters.Add("Phone", phone, DbType.String);
                parameters.Add("DesignationId", Convert.ToInt32(designationId), DbType.Int32);
                parameters.Add("PrimaryBranchId", Convert.ToInt32(primaryBranchId), DbType.Int32);
                parameters.Add("PrimaryDepartmentId", Convert.ToInt32(primaryDepartmentId), DbType.Int32);
                parameters.Add("IsActive", isActive, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;
            }
            catch(Exception ex)
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
