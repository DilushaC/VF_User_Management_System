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
                var roleId = collection["RoleId"].ToString();
                var pageId = collection["PageId"].ToString();
                var canEdit = collection["CanEdit"].ToString();

                string sql = @"
                INSERT INTO RolePagePermissions
                (RoleId, PageId, CanEdit)
                VALUES
                (@RoleId, @PageId, @CanEdit);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("RoleId", Convert.ToInt32(roleId), DbType.Int32);
                parameters.Add("PageId", Convert.ToInt32(pageId), DbType.Int32);
                parameters.Add("CanEdit", canEdit, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

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
                    WHERE UR.Id = @Id;
                ";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                    return null;

                DataRow row = data.Rows[0];

                RolePagePermissionModel model = new RolePagePermissionModel()
                {
                    Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]),
                    RoleId = row["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleId"]),
                    PageId = row["PageId"] == DBNull.Value ? 0 : Convert.ToInt32(row["PageId"]),
                    CanEdit = row["CanEdit"] == DBNull.Value ? false : Convert.ToBoolean(row["CanEdit"]),

                    RoleName = row["RoleName"]?.ToString() ?? "",
                    PageName = row["PageName"]?.ToString() ?? ""
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve role page permission with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateRolePagePermissionAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var pageId = collection["PageId"].ToString();
                var canEdit = Convert.ToBoolean(collection["CanEdit"]);

                string sql = @"
                    UPDATE RolePagePermissions
                    SET 
                        PageId = @PageId,
                        CanEdit = @CanEdit
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("PageId", Convert.ToInt32(pageId), DbType.Int32);
                parameters.Add("CanEdit", canEdit, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
