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
                var userId = collection["UserId"].ToString();
                var roleId = collection["RoleId"].ToString();

                string sql = @"
                INSERT INTO UserRoles
                (UserId, RoleId)
                VALUES
                (@UserId, @RoleId);
            ";

                var parameters = new DynamicParameters();

                parameters.Add("UserId", Convert.ToInt32(userId), DbType.Int32);
                parameters.Add("RoleId", Convert.ToInt32(roleId), DbType.Int32);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

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
                    SELECT 
                        UR.Id,
                        UR.UserId,
                        UR.RoleId,
                        U.UserName,
                        R.RoleName
                    FROM UserRoles UR
                    INNER JOIN Users U ON UR.UserId = U.Id
                    INNER JOIN Roles R ON UR.RoleId = R.Id;
                ";

                var Data = _connectionService.Return(Query);

                List<UserRoleModel> userRoles = new List<UserRoleModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var row = Data.Rows[i];

                    UserRoleModel model = new UserRoleModel()
                    {
                        Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]),
                        UserId = row["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserId"]),
                        RoleId = row["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleId"]),

                        UserName = row["UserName"] == DBNull.Value ? string.Empty : row["UserName"].ToString(),
                        RoleName = row["RoleName"] == DBNull.Value ? string.Empty : row["RoleName"].ToString()
                    };

                    userRoles.Add(model);
                }

                return userRoles;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
