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

namespace UserManagement.Business.RolePagePermission
{
    public class RolePagePermissionService : IRolePagePermissionService
    {
        private readonly _ConnectionService _connectionService;

        public RolePagePermissionService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }
        public async Task<bool> CreateRolePagePermissionAsync(IFormCollection collection)
        {
            try
            {
                int roleId = int.Parse(collection["RoleId"]);
                bool canEdit = bool.Parse(collection["CanEdit"]);

                // Multi-selected Page IDs
                var pageIds = collection["PageIds"].ToList(); // This gives List<string>

                string sql = @"
                    INSERT INTO RolePagePermissions
                    (RoleId, PageId, CanEdit)
                    VALUES
                    (@RoleId, @PageId, @CanEdit);
                ";

                int totalInserted = 0;

                foreach (var pageIdStr in pageIds)
                {
                    int pageId = int.Parse(pageIdStr);

                    var parameters = new DynamicParameters();
                    parameters.Add("RoleId", roleId, DbType.Int32);
                    parameters.Add("PageId", pageId, DbType.Int32);
                    parameters.Add("CanEdit", canEdit, DbType.Boolean);

                    totalInserted += _connectionService.ExecuteWithPara(sql, parameters);
                }

                return totalInserted == pageIds.Count; // all rows inserted successfully
            }
            catch
            {
                return false;
            }
        }


        public List<RolePagePermissionModel> GetAllRolePagePermissionList()
        {
            try
            {
                string Query = @"
                    SELECT DISTINCT
                        R.Id AS RoleId,
                        R.RoleName,
                        CASE 
                            WHEN EXISTS (
                                SELECT 1
                                FROM RolePagePermissions UR2
                                WHERE UR2.RoleId = R.Id AND UR2.CanEdit = 1
                            ) THEN 1
                            ELSE 0
                        END AS CanEdit
                    FROM Roles R
                    INNER JOIN RolePagePermissions UR ON UR.RoleId = R.Id;
                ";

                var Data = _connectionService.Return(Query);

                List<RolePagePermissionModel> pagePermissions = new List<RolePagePermissionModel>();

                foreach (DataRow row in Data.Rows)
                {
                    pagePermissions.Add(new RolePagePermissionModel
                    {
                        RoleId = row["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleId"]),
                        RoleName = row["RoleName"].ToString(),
                        CanEdit = row["CanEdit"] == DBNull.Value ? false : Convert.ToBoolean(row["CanEdit"])
                    });
                }

                return pagePermissions;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<RolePagePermissionModel> GetRolePagePermissionByIdAsync(int roleId)
        {
            try
            {
                string query = @"
                    SELECT 
                        UR.Id,
                        UR.RoleId,
                        UR.PageId,
                        UR.CanEdit,
                        R.RoleName,
                        P.PageName
                    FROM RolePagePermissions UR
                    LEFT JOIN Roles R ON UR.RoleId = R.Id
                    LEFT JOIN Pages P ON UR.PageId = P.Id
                    WHERE UR.RoleId = (SELECT Id FROM Roles WHERE Id = @Id);
                ";

                // Pass roleId directly as the single scalar parameter
                DataTable data = await _connectionService.SingleQueryReturn(query, roleId);

                if (data == null || data.Rows.Count == 0)
                    return null;

                RolePagePermissionModel model = new RolePagePermissionModel
                {
                    PageIds = new List<int>(),
                    RoleId = Convert.ToInt32(data.Rows[0]["RoleId"]),
                    RoleName = data.Rows[0]["RoleName"].ToString(),
                    CanEdit = false
                };

                foreach (DataRow row in data.Rows)
                {
                    if (row["PageId"] != DBNull.Value)
                        model.PageIds.Add(Convert.ToInt32(row["PageId"]));

                    if (row["CanEdit"] != DBNull.Value && Convert.ToBoolean(row["CanEdit"]))
                        model.CanEdit = true;
                }

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading role page permissions", ex);
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
