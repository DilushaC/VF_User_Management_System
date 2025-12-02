using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.PageHandler;
using UserManagement.Business.RoleHandler;
using UserManagement.Business.RolePagePermission;

namespace UserManagement.Web.Controllers
{
    public class RolePagePermissionController : Controller
    {
        private readonly IDataTableService _dataTableService;
        private readonly IRoleService _roleService;
        private readonly IPageService _pageService;
        private readonly IRolePagePermissionService _rolePagePermissionService;

        public RolePagePermissionController(IDataTableService dataTableService, IRoleService roleService,IPageService pageService,IRolePagePermissionService rolePagePermissionService)
        {
            _dataTableService = dataTableService;
            _roleService = roleService;
            _pageService = pageService;
            _rolePagePermissionService = rolePagePermissionService;
        }
        [HttpGet]
        public IActionResult Create()
        {
            var roles = _roleService.GetAllRolesList();
            var pages = _pageService.GetAllPagesList();

            //viewbag for branches
            ViewBag.Pages = pages
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.PageName
            })
            .ToList();

            //viewbag for departments
            ViewBag.Roles = roles
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.RoleName
            })
            .ToList();

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
                bool created = await _rolePagePermissionService.CreateRolePagePermissionAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Page Permission created successfully",
                        redirectUrl = Url.Action("Management", "RolePagePermission")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create Page Permission"
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
        public IActionResult GetRolePagePermissionPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _rolePagePermissionService.GetAllRolePagePermissionList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.RoleName.ToLower().Contains(s) ||
                    u.PageName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var pages = _pageService.GetAllPagesList();

            //viewbag for branches
            ViewBag.Pages = pages
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.PageName
            })
            .ToList();

            var rolePage = await _rolePagePermissionService.GetRolePagePermissionByIdAsync(id);
            if (rolePage == null)
            {
                return NotFound();
            }
            return PartialView("_EditRolePagePartial", rolePage);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _rolePagePermissionService.UpdateRolePagePermissionAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Page Permission." });
                }

                return Ok(new { success = true, message = "Page Permission updated successfully.", redirectUrl = Url.Action("Management", "RolePagePermission") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
