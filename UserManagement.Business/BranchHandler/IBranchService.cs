using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.BranchHandler
{
    public interface IBranchService
    {
        public List<BranchModel> GetAllBranchList();
        public List<BranchModel> GetAllActiveBranchList();
        Task<bool> CreateBranchAsync(IFormCollection collection);
        Task<BranchModel> GetBranchByIdAsync(int id);
        Task<bool> UpdateBranchAsync(IFormCollection collection);
    }
}
