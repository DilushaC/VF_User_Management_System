using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.RoleHandler;

namespace UserManagement.Web.Controllers
{
    public class RoleController : Controller
    {
        private readonly IDataTableService _dataTableService;
        private readonly IRoleService _roleService;

        public RoleController(IDataTableService dataTableService,IRoleService roleService)
        {
            _dataTableService = dataTableService;
            _roleService = roleService;
        }

        [HttpGet]
        public IActionResult Create(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            return View();
        }

        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _roleService.CreateRoleAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Role created successfully",
                        redirectUrl = Url.Action("Management", "Role")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create Role"
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
        public IActionResult GetRolesPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _roleService.GetAllRolesList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.RoleName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return PartialView("_EditRolePartial", role);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _roleService.UpdateRoleAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Role." });
                }

                return Ok(new { success = true, message = "Role updated successfully.", redirectUrl = Url.Action("Management", "Role") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(IFormCollection form)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(form);

                if (!result)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Role is linked to User"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Role deleted successfully.",
                    redirectUrl = Url.Action("Management", "Role")
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CheckRoleName(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _roleService.CheckRoleNameExists(collection);

            return Json(new { exists = exists });
        }
    }
}

