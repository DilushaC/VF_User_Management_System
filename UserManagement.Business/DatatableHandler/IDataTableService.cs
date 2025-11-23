using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.DatatableHandler
{
    public interface IDataTableService
    {
        DataTableRequest BuildRequest(HttpRequest request);
        DataTableResponseModel<T> ApplyDataTable<T>(
            IQueryable<T> query,
            DataTableRequest request);
    }

}
