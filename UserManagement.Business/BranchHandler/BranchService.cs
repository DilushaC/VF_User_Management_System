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

namespace UserManagement.Business.BranchHandler
{
    public class BranchService : IBranchService
    {
        private readonly _ConnectionService _connectionService;

        public BranchService(_ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<bool> CreateBranchAsync(IFormCollection collection)
        {
            try
            {
                var branchName = collection["BranchName"].ToString();
                var isActive = collection["IsActive"].ToString();

                string sql = @"
                INSERT INTO Branch
                (BranchName, IsActive, CreatedDate)
                VALUES
                (@BranchName, @IsActive, @CreatedDate);
            ";

                var parameters = new DynamicParameters();
                parameters.Add("BranchName", branchName, DbType.String);
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


        public List<BranchModel> GetAllBranchList()
        {

            try
            {
                string Query = $"SELECT * FROM Branch";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<BranchModel> branchList = new List<BranchModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    BranchModel bModel = new BranchModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        BranchName = BRow["BranchName"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    branchList.Add(bModel);
                }
                return branchList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<BranchModel> GetAllActiveBranchList()
        {

            try
            {
                string Query = $"SELECT * FROM Branch WHERE IsActive = 1";
                var Data = _connectionService.Return(Query);
                var Row = Data.Rows[0];

                List<BranchModel> branchList = new List<BranchModel>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    var BRow = Data.Rows[i];
                    BranchModel bModel = new BranchModel()
                    {
                        Id = Convert.ToInt32(BRow["Id"]),
                        BranchName = BRow["BranchName"].ToString(),
                        IsActive = Convert.ToBoolean(BRow["IsActive"]),
                        CreatedDate = Convert.ToDateTime(BRow["CreatedDate"]),
                    };
                    branchList.Add(bModel);
                }
                return branchList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<BranchModel> GetBranchByIdAsync(int id)
        {
            try
            {
                string query = @"
                                SELECT 
                                    Id,
                                    BranchName,
                                    IsActive,
                                    CreatedDate
                                FROM 
                                    Branch
                                WHERE 
                                    Id = @Id";

                DataTable data = await _connectionService.SingleQueryReturn(query, id);

                if (data == null || data.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = data.Rows[0];

                BranchModel model = new BranchModel()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    BranchName = row["BranchName"] == DBNull.Value ? string.Empty : row["BranchName"].ToString(),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = row["CreatedDate"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(row["CreatedDate"])
                };

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve BranchName with ID {id}.", ex);
            }
        }

        public async Task<bool> UpdateBranchAsync(IFormCollection collection)
        {
            try
            {
                var Id = Convert.ToInt32(collection["Id"]);
                var branchName = collection["BranchName"].ToString();
                var isActive = Convert.ToBoolean(collection["IsActive"]);

                string sql = @"
                    UPDATE Branch
                    SET 
                        BranchName = @BranchName,
                        IsActive = @IsActive,
                        CreatedDate = @CreatedDate
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", Id, DbType.Int32);
                parameters.Add("BranchName", branchName, DbType.String);
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

        public async Task<bool> CheckBranchNameExists(IFormCollection collection)
        {
            try
            {
                var branch = collection["BranchName"].ToString();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Branch
                    WHERE BranchName = @Branch
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Branch", branch, DbType.String);

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
