using ComplaintManagementSystem.Business.Authentication;
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
        private readonly ADAuthentication _aDAuthentication;

        public UserService(_ConnectionService connectionService, PasswordHelper passwordHelper,ADAuthentication aDAuthentication)
        {
            _connectionService = connectionService;
            _passwordHelper = passwordHelper;
            _aDAuthentication = aDAuthentication;
        }

        //public async Task<bool> CreateUserAsync(IFormCollection collection)
        //{
        //    try
        //    {
        //        // Extract values from collection
        //        var userName = collection["UserName"].ToString();
        //        var firstName = collection["FirstName"].ToString();
        //        var lastName = collection["LastName"].ToString();
        //        var email = collection["Email"].ToString();
        //        var phone = collection["Phone"].ToString();
        //        var designationId = collection["DesignationId"].ToString();
        //        var primaryBranchId = collection["PrimaryBranchId"].ToString();
        //        var primaryDepartmentId = collection["PrimaryDepartmentId"].ToString();

        //        var password = "1234";
        //        string encryptedPassword = _passwordHelper.ComputeHmac(password);

        //        string sql = @"
        //        INSERT INTO Users
        //        (UserName, Password, FirstName, LastName, Email, Phone, DesignationId, PrimaryBranchId, PrimaryDepartmentId, IsActive, CreatedDate)
        //        VALUES
        //        (@UserName, @Password, @FirstName, @LastName, @Email, @Phone, @DesignationId, @PrimaryBranchId, @PrimaryDepartmentId,@IsActive, @CreatedDate);
        //    ";

        //        var parameters = new DynamicParameters();
        //        parameters.Add("UserName", userName, DbType.String);
        //        parameters.Add("Password", encryptedPassword, DbType.String);
        //        parameters.Add("FirstName", firstName, DbType.String);
        //        parameters.Add("LastName", lastName, DbType.String);
        //        parameters.Add("Email", email, DbType.String);
        //        parameters.Add("Phone", phone, DbType.String);
        //        parameters.Add("DesignationId", Convert.ToInt32(designationId), DbType.Int32);
        //        parameters.Add("PrimaryBranchId", Convert.ToInt32(primaryBranchId), DbType.Int32);
        //        parameters.Add("PrimaryDepartmentId", Convert.ToInt32(primaryDepartmentId), DbType.Int32);
        //        parameters.Add("IsActive", true, DbType.Boolean);
        //        parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

        //        int rows = _connectionService.ExecuteWithPara(sql, parameters);

        //        return rows > 0;

        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public async Task<(bool IsSuccess, int? UserId)> CreateUserAsync(IFormCollection collection)
        {
            try
            {
                string sql = @"
                    INSERT INTO Users
                    (UserName, FirstName, LastName, Email, Phone,
                     DesignationId, PrimaryBranchId, PrimaryDepartmentId,
                     IsActive, CreatedDate)
                    VALUES
                    (@UserName, @FirstName, @LastName, @Email, @Phone,
                     @DesignationId, @PrimaryBranchId, @PrimaryDepartmentId,
                     @IsActive, @CreatedDate);

                    SELECT CAST(SCOPE_IDENTITY() AS INT) AS UserId;
                ";

                var parameters = new
                {
                    UserName = collection["UserName"].ToString(),
                    FirstName = collection["FirstName"].ToString(),
                    LastName = collection["LastName"].ToString(),
                    Email = collection["Email"].ToString(),
                    Phone = collection["Phone"].ToString(),
                    DesignationId = Convert.ToInt32(collection["DesignationId"]),
                    PrimaryBranchId = Convert.ToInt32(collection["PrimaryBranchId"]),
                    PrimaryDepartmentId = Convert.ToInt32(collection["PrimaryDepartmentId"]),
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                DataTable dt = await _connectionService.SingleQueryReturnId(sql, parameters);

                if (dt.Rows.Count > 0)
                {
                    int userId = Convert.ToInt32(dt.Rows[0]["UserId"]);

                    // multiple role IDs (List<string>)
                    var productIds = collection["ProductIds"].ToList();

                    string sql2 = @"
                        INSERT INTO UserProducts
                        (UserId, ProductId)
                        VALUES
                        (@UserId, @ProductId);
                    ";

                    int insertedCount = 0;

                    foreach (var pid in productIds)
                    {
                        int productId = int.Parse(pid);
                        var parameters2 = new DynamicParameters();
                        parameters2.Add("UserId", userId, DbType.Int32);
                        parameters2.Add("ProductId", productId, DbType.Int32);

                        insertedCount += _connectionService.ExecuteWithPara(sql2, parameters2);
                    }
                    return (true, userId);
                }

                return (false, null);
            }
            catch
            {
                return (false, null);
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

        public List<UserModel> GetAllActiveUsersList()
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
                                    Designation G ON U.DesignationId = G.Id
                                WHERE U.IsActive = 1 ";

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


        public List<UserModel> GetAllActiveUsersWithoutRoles()
        {
            try
            {
                string query = @"
                    SELECT U.Id, U.UserName
                    FROM Users U
                    WHERE U.IsActive = 1
                      AND U.UserName IS NOT NULL
                      AND NOT EXISTS (
                          SELECT 1
                          FROM UserRoles UR
                          WHERE UR.UserId = U.Id
                      )";

                var data = _connectionService.Return(query);

                List<UserModel> userNames = new List<UserModel>();

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var BRow = data.Rows[i];

                    UserModel usersModel = new UserModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        UserName = BRow["UserName"].ToString()
                    };

                    userNames.Add(usersModel);
                }

                return userNames;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<UserModel> GetUserByIdAsync(int id)
        {
            try
            {
                // Get user info with joins
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

                    ProductIds = new List<int>() // initialize
                };

                // Get UserProducts for this user
                string productQuery = "SELECT ProductId FROM UserProducts WHERE UserId = @Id";
                DataTable productData = await _connectionService.SingleQueryReturn(productQuery, id);

                if (productData != null && productData.Rows.Count > 0)
                {
                    foreach (DataRow row in productData.Rows)
                    {
                        userModel.ProductIds.Add(Convert.ToInt32(row["ProductId"]));
                    }
                }

                return userModel;
            }
            catch (Exception ex)
            {
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

                if (rows == 0)
                    return false;

                string deleteSql = "DELETE FROM UserProducts WHERE UserId = @UserId";
                var deleteParams = new DynamicParameters();
                deleteParams.Add("UserId", userId, DbType.Int32);
                _connectionService.ExecuteWithPara(deleteSql, deleteParams);

                var productIds = collection["ProductIds"].ToList();

                string insertSql = @"
                    INSERT INTO UserProducts (UserId, ProductId)
                    VALUES (@UserId, @ProductId);
                ";

                foreach (var pid in productIds)
                {
                    int productId = Convert.ToInt32(pid);
                    var insertParams = new DynamicParameters();
                    insertParams.Add("UserId", userId, DbType.Int32);
                    insertParams.Add("ProductId", productId, DbType.Int32);
                    _connectionService.ExecuteWithPara(insertSql, insertParams);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        //public async Task<UserModel?> ValidateUserAsync(string username, string password, int productId)
        //{
        //    // 1. Authenticate AD
        //    var response = await _aDAuthentication.AuthenticatewithAD(username, password);
        //    if (!response.Status)
        //        return null;

        public async Task<UserModel?> ValidateUserAsync(string username, string password, int productId)
        {
            // 1. Authenticate - check password contains "abc123"
            var response = await _aDAuthentication.AuthenticatewithAD(username, password);
            if (!response.Status)
                return null;

            // 2. Get User
            const string userQuery = @"
                SELECT *
                FROM Users
                WHERE UserName = @UserName AND IsActive = 1";

            var userParams = new DynamicParameters();
            userParams.Add("@UserName", username);

            var userData = _connectionService.ReturnWithPara(userQuery, userParams);
            if (userData == null || userData.Rows.Count == 0)
                return null;

            var userRow = userData.Rows[0];

            var user = new UserModel
            {
                Id = userRow.Field<int>("Id"),
                UserName = userRow.Field<string>("UserName"),
                DisplayName = response.Data.DisplayName,
                DisplayDesignation = response.Data.Title,
                DisplayDepartment = response.Data.Department,
                IsActive = userRow.Field<bool>("IsActive")
            };

            // 3. Get ProductIds
            const string productQuery = @"
                SELECT ProductId
                FROM UserProducts
                WHERE UserId = @UserId";

            var productParams = new DynamicParameters();
            productParams.Add("@UserId", user.Id);

            var productData = _connectionService.ReturnWithPara(productQuery, productParams);
            if (productData != null && productData.Rows.Count > 0)
            {
                user.ProductIds = productData
                    .AsEnumerable()
                    .Select(r => r.Field<int>("ProductId"))
                    .Distinct()
                    .ToList();
            }

            if (!user.ProductIds.Any())
                return user;

            // 4. Get MenuItems with PageUrls properly
            const string menuQuery = @"
                SELECT DISTINCT
                    m.Id,
                    m.MenuTitle,
                    m.ParentMenuId,
                    m.PageId,
                    m.IconClass,
                    m.DisplayOrder,
                    m.IsActive,
                    m.ProductId,
                    m.MenuCategoryId,
                    c.CategoryName,
                    p.PageUrl
                FROM MenuItems m
                LEFT JOIN Pages p 
                    ON m.PageId = p.Id
                LEFT JOIN MenuCategories c 
                    ON m.MenuCategoryId = c.Id
                LEFT JOIN RolePagePermissions rpp
                    ON m.PageId = rpp.PageId
                LEFT JOIN UserRoles ur
                    ON rpp.RoleId = ur.RoleId
                WHERE m.IsActive = 1
                  AND m.ProductId = @ProductId
                  AND (
                        ur.UserId = @UserId
                        OR m.PageId IS NULL
                      )
                ORDER BY m.DisplayOrder";



            var menuParams = new DynamicParameters();
            menuParams.Add("@UserId", user.Id);
            menuParams.Add("@ProductId", productId);


            var menuData = _connectionService.ReturnWithPara(menuQuery, menuParams);

            if (menuData != null && menuData.Rows.Count > 0)
            {
                // Deduplicate by MenuItem Id to avoid duplicates caused by multiple products
                user.MenuItems = menuData.AsEnumerable()
                    .Select(r => new MenuItem
                    {
                        Id = r.Field<int>("Id"),
                        MenuTitle = r.Field<string>("MenuTitle"),
                        ParentMenuItemId = r.Field<int?>("ParentMenuId"),
                        PageId = r.Field<int?>("PageId"),
                        IconClass = r.Field<string?>("IconClass"),
                        DisplayOrder = r.Field<int>("DisplayOrder"),
                        IsActive = r.Field<bool>("IsActive"),
                        ProductId = r.Field<int?>("ProductId"),
                        CategoryId = r.Field<int?>("MenuCategoryId"),
                        CategoryName = r.Field<string?>("CategoryName"),
                        PageUrl = r.Field<string?>("PageUrl")
                    })
                    .GroupBy(m => m.Id)
                    .Select(g => g.First())
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();

            }

            // 5. Populate PageUrls for session
            user.PageUrls = user.MenuItems
                .Where(m => !string.IsNullOrWhiteSpace(m.PageUrl))
                .Select(m => m.PageUrl!.StartsWith("/") ? m.PageUrl : "/" + m.PageUrl)
                .Distinct()
                .ToList();

            return user;
        }



        //get user pages
        public async Task<UserModel> GetPagesByUserId(int id)
        {
            try
            {
                UserModel userModel = new UserModel()
                {
                    PageUrls = new List<string>()
                };

                // Get RoleId for the user
                string roleQuery = $"SELECT RoleId FROM UserRoles WHERE UserId = {id}";
                DataTable roleData = await _connectionService.SingleQueryReturn(roleQuery, id);

                if (roleData != null && roleData.Rows.Count > 0)
                {
                    int roleId = Convert.ToInt32(roleData.Rows[0]["RoleId"]);
                    userModel.RoleId = roleId;

                    // Get PageUrl and CanEdit in one query
                    string pageQuery = $@"
                        SELECT p.PageUrl, p.IconClass, r.CanEdit
                        FROM Pages p
                        INNER JOIN RolePagePermissions r ON p.Id = r.PageId
                        WHERE r.RoleId = {roleId} AND p.IsActive = 1
                        ORDER BY 
                            CASE WHEN p.PageLevel IS NULL THEN 1 ELSE 0 END,
                            p.PageLevel ASC";

                    DataTable pageData = await _connectionService.SingleQueryReturn(pageQuery, roleId);

                    if (pageData != null)
                    {
                        foreach (DataRow p in pageData.Rows)
                        {
                            string pageUrl = p["PageUrl"].ToString();
                            string iconClass = p["IconClass"].ToString();
                            bool canEdit = p["CanEdit"] != DBNull.Value && Convert.ToBoolean(p["CanEdit"]);

                            // Store as "PageUrl|CanEdit" string, or you can create a tuple/class if needed
                            userModel.PageUrls.Add($"{pageUrl}|{iconClass}|{canEdit}");
                        }
                    }
                }

                return userModel;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve user with ID {id}.", ex);
            }
        }


        public async Task<bool> CheckUserNameExists(IFormCollection collection)
        {
            try
            {
                var userName = collection["UserName"].ToString();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE UserName = @UserName
                ";

                var parameters = new DynamicParameters();
                parameters.Add("UserName", userName, DbType.String);

                // Synchronous call wrapped in Task.FromResult
                var result = _connectionService.ExecuteScalar(sql, parameters);

                int count = Convert.ToInt32(result);

                return await Task.FromResult(count > 0);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }


    }
}
