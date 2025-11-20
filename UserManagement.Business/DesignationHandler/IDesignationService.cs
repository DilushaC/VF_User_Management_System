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
        public List<DesignationModel> GetAllDesignationList();
    }
}
