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
    }
}
