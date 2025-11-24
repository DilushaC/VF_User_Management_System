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
    }
}
