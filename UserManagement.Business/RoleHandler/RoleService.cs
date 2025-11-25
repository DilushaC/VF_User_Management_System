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
                var isAdminAccount = collection["IsAdminAccount"].ToString();

                string sql = @"
                INSERT INTO Roles
                (RoleName, Description, IsAdminAccount)
                VALUES
                (@RoleName, @Description, @IsAdminAccount);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("RoleName", roleName, DbType.String);
                parameters.Add("Description", description, DbType.String);
                parameters.Add("IsAdminAccount", isAdminAccount, DbType.Boolean);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
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
                        IsAdminAccount = Convert.ToBoolean(BRow["IsAdminAccount"]),
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
    }
}
