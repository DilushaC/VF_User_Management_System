using System;
using System.Collections.Generic;
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
        public List<BranchModel> GetAllBranchList()
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
    }
}
