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

namespace UserManagement.Business.DepartmentHandler
{
    public class DepartmentService: IDepartmentService
    {
        private readonly _ConnectionService _connectionService;

        public DepartmentService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<bool> CreateDepartmentAsync(IFormCollection collection)
        {
            try
            {
                var departmentName = collection["DepartmentName"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Department
                (DepartmentName, IsActive, CreatedDate)
                VALUES
                (@DepartmentName, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();
                parameters.Add("DepartmentName", departmentName, DbType.String);
                parameters.Add("IsActive", isActive, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
            }
        }


        public List<DepartmentModel> GetAllDepartmentList()
        {

            try
            {
                string Query = $"SELECT * FROM Department WHERE IsActive = 1";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<DepartmentModel> departmentList = new List<DepartmentModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    DepartmentModel depModel = new DepartmentModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        DepartmentName = BRow["DepartmentName"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    departmentList.Add(depModel);
                }
                return departmentList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
