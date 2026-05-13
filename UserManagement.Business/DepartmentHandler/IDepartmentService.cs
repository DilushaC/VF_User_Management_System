using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.DepartmentHandler
{
    public interface IDepartmentService
    {
        public List<DepartmentModel> GetAllDepartmentList();
        public List<DepartmentModel> GetAllActiveDepartmentList();
        Task<bool> CreateDepartmentAsync(IFormCollection collection);
        Task<DepartmentModel> GetDepartmentByIdAsync(int id);
        Task<bool> UpdateDepartmentAsync(IFormCollection collection);
        Task<bool> DeleteDepartmentAsync(IFormCollection collection);
        Task<bool> CheckDepartmentNameExists(IFormCollection collection);
    }
}
