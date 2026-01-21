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

namespace UserManagement.Business.RoleHandler
{
    public class RoleService : IRoleService
    {
        private readonly _ConnectionService _connectionService;

        public RoleService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }
        public async Task<bool> CreateRoleAsync(IFormCollection collection)
        {
            try
            {
                var roleName = collection["RoleName"].ToString();
                var description = collection["Description"].ToString();
                var IsActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Roles
                (RoleName, Description, IsActive)
                VALUES
                (@RoleName, @Description, @IsActive);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("RoleName", roleName, DbType.String);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("IsActive", IsActive, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
            }
        }

        public List<RoleModel> GetRolesWithoutPages()
        {
            try
            {
                string Query = $"SELECT R.Id AS RoleId, R.RoleName FROM Roles R LEFT JOIN RolePagePermissions UR ON UR.RoleId = R.Id WHERE UR.RoleId IS NULL;";


                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<RoleModel> rolesList = new List<RoleModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    RoleModel bModel = new RoleModel()
                    {
                        Id = Convert.ToInt32(BRow["RoleId"]),
                        RoleName = BRow["RoleName"].ToString(),
                    };
                    rolesList.Add(bModel);
                }
                return rolesList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RoleModel> GetAllRolesList()
        {
            try
            {
                string Query = $"SELECT * FROM Roles";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<RoleModel> rolesList = new List<RoleModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    RoleModel bModel = new RoleModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        RoleName = BRow["RoleName"].ToString(),
                        Description = BRow["Description"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                    };
                    rolesList.Add(bModel);
                }
                return rolesList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RoleModel> GetAllActiveRolesList()
        {
            try
            {
                string Query = $"SELECT * FROM Roles WHERE IsActive = 1";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<RoleModel> rolesList = new List<RoleModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    RoleModel bModel = new RoleModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        RoleName = BRow["RoleName"].ToString(),
                        Description = BRow["Description"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                    };
                    rolesList.Add(bModel);
                }
                return rolesList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<RoleModel> GetRoleByIdAsync(int id)
        {
            try
            {
                string query = @"
                                SELECT 
                                    Id,
                                    RoleName,
                                    Description,
                                    IsActive
                                FROM 
                                    Roles
                                WHERE 
                                    Id = @Id";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                RoleModel model = new RoleModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    RoleName = row["RoleName"] == DBNull.Value ? string.Empty : row["RoleName"].ToString(),
                    Description = row["Description"] == DBNull.Value ? string.Empty : row["Description"].ToString(),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve role with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateRoleAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var roleName = collection["RoleName"].ToString();
                var description = collection["Description"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Roles
                    SET 
                        RoleName = @RoleName,
                        Description = @Description,
                        IsActive = @IsActive
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("RoleName", roleName, DbType.String);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("IsActive", isActive, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CheckRoleNameExists(IFormCollection collection)
        {
            try
            {
                var role = collection["RoleName"].ToString();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Roles
                    WHERE RoleName = @RoleName
                ";

                var parameters = new DynamicParameters();
                parameters.Add("RoleName", role, DbType.String);

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
