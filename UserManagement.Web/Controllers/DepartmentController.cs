using Microsoft.AspNetCore.Mvc;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.ProductHandler;

namespace UserManagement.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IDataTableService _dataTableService;

        public DepartmentController(IDepartmentService departmentService, IDataTableService dataTableService)
        {
            _departmentService = departmentService;
            _dataTableService = dataTableService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult DepartmentsManagement()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _departmentService.CreateDepartmentAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Department created successfully",
                        redirectUrl = Url.Action("DepartmentsManagement", "Department")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create Department"
                    });
                }
            }
            catch (Exception ex)
            {
                // Return error response
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public IActionResult GetDepartmentsPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _departmentService.GetAllDepartmentList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.DepartmentName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return PartialView("_EditDepartmentPartial", department);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _departmentService.UpdateDepartmentAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Department." });
                }

                return Ok(new { success = true, message = "Department updated successfully.", redirectUrl = Url.Action("DepartmentsManagement", "Department") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
