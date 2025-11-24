using Microsoft.AspNetCore.Mvc;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;

namespace UserManagement.Web.Controllers
{
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly IDataTableService _dataTableService;

        public BranchController(IBranchService branchService, IDataTableService dataTableService)
        {
            _branchService = branchService;
            _dataTableService = dataTableService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BranchesManagement()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _branchService.CreateBranchAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Branch created successfully",
                        redirectUrl = Url.Action("BranchesManagement", "Branch")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create Branch"
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
        public IActionResult GetBranchesPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _branchService.GetAllBranchList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.BranchName.Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null)
            {
                return NotFound();
            }
            return PartialView("_EditBranchPartial", branch);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _branchService.UpdateBranchAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Branch." });
                }

                return Ok(new { success = true, message = "Branch updated successfully.", redirectUrl = Url.Action("BranchesManagement", "Branch") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

    }
}
