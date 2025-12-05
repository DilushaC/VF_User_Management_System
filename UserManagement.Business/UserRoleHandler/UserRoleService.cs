using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Data.Models;

namespace UserManagement.Business.UserRoleHandler
{
    public class UserRoleService : IUserRoleService
    {
        private readonly _ConnectionService _connectionService;

        public UserRoleService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }
        public async Task<bool> CreateUserRoleAsync(IFormCollection collection)
        {
            try
            {
                int userId = int.Parse(collection["UserId"]);

                // multiple role IDs (List<string>)
                var roleIds = collection["RoleIds"].ToList();

                string sql = @"
                    INSERT INTO UserRoles
                    (UserId, RoleId)
                    VALUES
                    (@UserId, @RoleId);
                ";

                int insertedCount = 0;

                foreach (var rid in roleIds)
                {
                    int roleId = int.Parse(rid);

                    var parameters = new DynamicParameters();
                    parameters.Add("UserId", userId, DbType.Int32);
                    parameters.Add("RoleId", roleId, DbType.Int32);

                    insertedCount += _connectionService.ExecuteWithPara(sql, parameters);
                }

                return insertedCount == roleIds.Count; // all inserted
            }
            catch
            {
                return false;
            }
        }


        public List<UserRoleModel> GetAllUserRolesList()
        {
            try
            {
                string Query = @"
                    SELECT DISTINCT
                        U.Id AS UserId,
                        U.UserName
                    FROM Users U
                    INNER JOIN UserRoles UR ON UR.UserId = U.Id;
                ";

                var Data = _connectionService.Return(Query);

                List<UserRoleModel> userRoles = new List<UserRoleModel>();

                foreach (DataRow row in Data.Rows)
                {
                    userRoles.Add(new UserRoleModel
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        UserName = row["UserName"].ToString()
                    });
                }

                return userRoles;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<List<UserRoleModel>> GetUserRolesByUserIdAsync(int userId)
        {
            try
            {
                string query = @"
                    SELECT 
                        UR.Id,
                        UR.UserId,
                        UR.RoleId,
                        U.UserName,
                        R.RoleName
                    FROM UserRoles UR
                    INNER JOIN Users U ON UR.UserId = U.Id
                    INNER JOIN Roles R ON UR.RoleId = R.Id
                    WHERE UR.UserId = (SELECT Id FROM Users WHERE Id = @Id);
                ";

                // Pass userId directly as the single scalar parameter
                DataTable data = await _connectionService.SingleQueryReturn(query, userId);

                List<UserRoleModel> rolesList = new List<UserRoleModel>();

                if (data == null || data.Rows.Count == 0)
                    return rolesList;

                foreach (DataRow row in data.Rows)
                {
                    rolesList.Add(new UserRoleModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        UserId = Convert.ToInt32(row["UserId"]),
                        UserName = row["UserName"].ToString(),
                        RoleId = Convert.ToInt32(row["RoleId"]),
                        RoleName = row["RoleName"].ToString()
                    });
                }

                return rolesList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading user roles", ex);
            }
        }





        public async Task<bool> UpdateUserRoleAsync(IFormCollection collection)
        {
            try
            {
                int userId = Convert.ToInt32(collection["UserId"]);
                string[] roleIds = collection["RoleIds"];

                // 1. Delete existing permissions for the role
                string deleteSql = "DELETE FROM UserRoles WHERE UserId = @UserId";
                var deleteParams = new DynamicParameters();
                deleteParams.Add("@UserId", userId, DbType.Int32);
                _connectionService.ExecuteWithPara(deleteSql, deleteParams);

                // 2. Insert new permissions for each selected page
                string insertSql = @"
                    INSERT INTO UserRoles (UserId, RoleId)
                    VALUES (@UserId, @RoleId);
                ";

                foreach (var rid in roleIds)
                {
                    var insertParams = new DynamicParameters();
                    insertParams.Add("@UserId", userId, DbType.Int32);
                    insertParams.Add("@RoleId", Convert.ToInt32(rid), DbType.Int32);

                    _connectionService.ExecuteWithPara(insertSql, insertParams);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> UpdateRolePagePermissionAsync(IFormCollection collection)
        {
            try
            {
                int roleId = Convert.ToInt32(collection["RoleId"]);
                string[] pageIds = collection["PageIds"];
                bool canEdit = collection["CanEdit"] == "true";

                // 1. Delete existing permissions for the role
                string deleteSql = "DELETE FROM RolePagePermissions WHERE RoleId = @RoleId";
                var deleteParams = new DynamicParameters();
                deleteParams.Add("@RoleId", roleId, DbType.Int32);
                _connectionService.ExecuteWithPara(deleteSql, deleteParams);

                // 2. Insert new permissions for each selected page
                string insertSql = @"
                    INSERT INTO RolePagePermissions (RoleId, PageId, CanEdit)
                    VALUES (@RoleId, @PageId, @CanEdit);
                ";

                foreach (var pid in pageIds)
                {
                    var insertParams = new DynamicParameters();
                    insertParams.Add("@RoleId", roleId, DbType.Int32);
                    insertParams.Add("@PageId", Convert.ToInt32(pid), DbType.Int32);
                    insertParams.Add("@CanEdit", canEdit, DbType.Boolean);

                    _connectionService.ExecuteWithPara(insertSql, insertParams);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }




    }
}
