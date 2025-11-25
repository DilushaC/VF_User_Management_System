using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.RoleHandler;
using UserManagement.Business.UserHandler;
using UserManagement.Business.UserRoleHandler;

namespace UserManagement.Web.Controllers
{
    public class UserRoleController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IDataTableService _dataTableService;

        public UserRoleController(IUserService userService,IDataTableService dataTableService,IRoleService roleService,IUserRoleService userRoleService)
        {
            _userService = userService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _dataTableService = dataTableService;
        }

        public IActionResult Index()
        {
            var users = _userService.GetAllActiveUsersList();
            var roles = _roleService.GetAllRolesList();

            //viewbag for branches
            ViewBag.Users = users
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.UserName
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

        public ActionResult UserRolesManagement()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _userRoleService.CreateUserRoleAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "User Role created successfully",
                        redirectUrl = Url.Action("UserRolesManagement", "UserRole")
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
        public IActionResult GetUserRolesPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _userRoleService.GetAllUserRolesList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.RoleName.ToLower().Contains(s) ||
                    u.UserName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }


        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var roles = _roleService.GetAllRolesList();

            //viewbag for branches
            ViewBag.UserRoles = roles
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.RoleName
            })
            .ToList();

            var user = await _userRoleService.GetUserRoleByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditUserRolePartial", user);
        }


        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _userRoleService.UpdateUserRoleAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update User Role." });
                }

                return Ok(new { success = true, message = "User Role updated successfully.", redirectUrl = Url.Action("UserRolesManagement", "UserRole") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
