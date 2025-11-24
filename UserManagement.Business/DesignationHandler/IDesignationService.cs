using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.DesignationHandler
{
    public interface IDesignationService
    {
        Task<bool> CreateDesignationAsync(IFormCollection collection);
        public List<DesignationModel> GetAllDesignationList();
        public List<DesignationModel> GetAllActiveDesignationList();
        Task<DesignationModel> GetDesignationByIdAsync(int id);
        Task<bool> UpdateDesignationAsync(IFormCollection collection);

    }
}
