using Dapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Business.Helpers;
using UserManagement.Data.Models;

namespace UserManagement.Business.DesignationHandler
{
    public class DesignationService : IDesignationService
    {
        private readonly _ConnectionService _connectionService;

        public DesignationService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<bool> CreateDesignationAsync(IFormCollection collection)
        {
            try
            {
                // Extract values from collection
                var designationName = collection["DesignationName"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Designation
                (DesignationName, IsActive, CreatedDate)
                VALUES
                (@DesignationName, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();
                parameters.Add("DesignationName", designationName, DbType.String);
                parameters.Add("IsActive", true, DbType.Boolean);
                parameters.Add("CreatedDate", DateTime.Now, DbType.DateTime);

                int rows = _connectionService.ExecuteWithPara(sql, parameters);

                return rows > 0;

            }
            catch
            {
                return false;
            }
        }

        public List<DesignationModel> GetAllDesignationList()
        {
            try
            {
                string Query = $"SELECT * FROM Designation";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<DesignationModel> designationList = new List<DesignationModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    DesignationModel bModel = new DesignationModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        DesignationName = BRow["DesignationName"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    designationList.Add(bModel);
                }
                return designationList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DesignationModel> GetAllUSerDesignationList()
        {
            try
            {
                string Query = $"SELECT * FROM Designation WHERE IsActive = 1";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<DesignationModel> designationList = new List<DesignationModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    DesignationModel bModel = new DesignationModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        DesignationName = BRow["DesignationName"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    designationList.Add(bModel);
                }
                return designationList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DesignationModel> GetDesignationByIdAsync(int id)
        {
            try
            {
                string query = @"
                                SELECT 
                                    Id,
                                    DesignationName,
                                    IsActive,
                                    CreatedDate
                                FROM 
                                    Designation
                                WHERE 
                                    Id = @Id";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                DesignationModel model = new DesignationModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    DesignationName = row["DesignationName"] == DBNull.Value ? string.Empty : row["DesignationName"].ToString(),
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

        public async Task<bool> UpdateDesignationAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var designationName = collection["DesignationName"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Designation
                    SET 
                        DesignationName = @DesignationName,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("DesignationName", designationName, DbType.String);
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
