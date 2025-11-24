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
                string Query = $"SELECT * FROM Department";
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

        public List<DepartmentModel> GetAllActiveDepartmentList()
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


        public async Task<DepartmentModel> GetDepartmentByIdAsync(int id)
        {
            try
            {
                string query = @"
                                SELECT 
                                    Id,
                                    DepartmentName,
                                    IsActive,
                                    CreatedDate
                                FROM 
                                    Department
                                WHERE 
                                    Id = @Id";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                DepartmentModel model = new DepartmentModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    DepartmentName = row["DepartmentName"] == DBNull.Value ? string.Empty : row["DepartmentName"].ToString(),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(row["CreatedDate"])
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve designation with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateDepartmentAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var departmentName = collection["DepartmentName"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Department
                    SET 
                        DepartmentName = @DepartmentName,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("DepartmentName", departmentName, DbType.String);
                parameters.Add("IsActive", isActive, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

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
