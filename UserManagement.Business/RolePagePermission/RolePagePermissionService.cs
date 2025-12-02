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
                    SELECT 
                        UR.Id,
                        UR.RoleId,
                        UR.PageId,
                        UR.CanEdit,
                        R.RoleName,
                        P.PageName
                    FROM RolePagePermissions UR
                    INNER JOIN Roles R ON UR.RoleId = R.Id
                    INNER JOIN Pages P ON UR.PageId = P.Id;
                ";

                var Data = _connectionService.Return(Query);

                List<RolePagePermissionModel> pagePermissions = new List<RolePagePermissionModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var row = Data.Rows[i];

                    RolePagePermissionModel model = new RolePagePermissionModel()
                    {
                        Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]),
                        RoleId = row["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleId"]),
                        PageId = row["PageId"] == DBNull.Value ? 0 : Convert.ToInt32(row["PageId"]),
                        CanEdit = Convert.ToBoolean(row["CanEdit"]),

                        RoleName = row["RoleName"] == DBNull.Value ? string.Empty : row["RoleName"].ToString(),
                        PageName = row["PageName"] == DBNull.Value ? string.Empty : row["PageName"].ToString()
                    };

                    pagePermissions.Add(model);
                }

                return pagePermissions;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RolePagePermissionModel> GetRolePagePermissionByIdAsync(int id)
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
            WHERE UR.RoleId = (SELECT RoleId FROM RolePagePermissions WHERE Id = @Id);
        ";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                    return null;

                RolePagePermissionModel model = new RolePagePermissionModel();
                model.PageIds = new List<int>();

                // first row for base details
                DataRow first = data.Rows[0];

                model.RoleId = Convert.ToInt32(first["RoleId"]);
                model.RoleName = first["RoleName"].ToString();
                model.CanEdit = Convert.ToBoolean(first["CanEdit"]);

                // add all PageIds
                foreach (DataRow row in data.Rows)
                {
                    model.PageIds.Add(Convert.ToInt32(row["PageId"]));
                }

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading permission", ex);
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
