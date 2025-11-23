using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Business.DatatableHandler
{
    public class DataTableService : IDataTableService
    {
        public DataTableRequest BuildRequest(HttpRequest request)
        {
            return new DataTableRequest
            {
                Draw = Convert.ToInt32(request.Form["draw"]),
                Start = Convert.ToInt32(request.Form["start"]),
                Length = Convert.ToInt32(request.Form["length"]),
                SearchValue = request.Form["search[value]"].FirstOrDefault()
            };
        }

        public DataTableResponseModel<T> ApplyDataTable<T>(
            IQueryable<T> query,
            DataTableRequest request)
        {
            int totalRecords = query.Count();

            // Search filter
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                // This requires custom filtering per controller OR expression trees
                // For now developer filters before calling this method
            }

            int recordsFiltered = query.Count();

            // Paging
            var data = query
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            return new DataTableResponseModel<T>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = recordsFiltered,
                data = data
            };
        }
    }

}
