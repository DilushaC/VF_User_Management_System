using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.ProductHandler;
using UserManagement.Business.UserHandler;
using UserManagement.Data.Models;
using UserManagement.Presentation.Filters;

namespace UserManagement.Web.Controllers
{
    [SessionCheck]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBranchService _branchService;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly IProductService _productService;
        private readonly IDataTableService _dataTableService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IBranchService branchService,IDepartmentService departmentService,IDesignationService designationService,IProductService productService,IDataTableService dataTableService, IConfiguration configuration)
        {
            _userService = userService;
            _branchService = branchService;
            _departmentService = departmentService;
            _designationService = designationService;
            _productService = productService;
            _dataTableService = dataTableService;
            _configuration = configuration;
        }

        //get all user list draw the initial data table
        public IActionResult Management()
        {
            return View();
        }

        //data table APi call to get records
        [HttpPost]
        public IActionResult GetUsersPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _userService.GetAllUsersList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(s) ||
                    u.FirstName.ToLower().Contains(s) ||
                    u.LastName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }



        //load single user record
        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var branches = _branchService.GetAllActiveBranchList();
            var departments = _departmentService.GetAllActiveDepartmentList();
            var designations = _designationService.GetAllActiveDesignationList();
            var products = _productService.GetAllActiveProductList();

            ViewBag.Products = products
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProductName
            })
            .ToList();

            //viewbag for branches
            ViewBag.Branches = branches
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.BranchName
            })
            .ToList();

            //viewbag for departments
            ViewBag.Departments = departments
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DepartmentName
            })
            .ToList();

            //viewbag for desingations
            ViewBag.Designations = designations
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DesignationName
            })
            .ToList();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditUserPartial", user);
        }

        //update single user data
        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update user."});
                }

                return Ok(new { success = true, message = "User updated successfully.", redirectUrl = Url.Action("Management", "User") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }



        public ActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            int allowedProductId = _configuration.GetValue<int>("AllowedProducts:ProductId");

            var user = await _userService.ValidateUserAsync(username, password, allowedProductId);
            if (user == null)
                return Json(new { success = false, message = "Invalid login" });

            if (user.ProductIds == null || !user.ProductIds.Contains(allowedProductId))
                return Json(new { success = false, message = "Unauthorized product access" });

            // Session storage
            HttpContext.Session.SetString("UserName", user.DisplayName);
            HttpContext.Session.SetString("Designation", user.DisplayDesignation);
            HttpContext.Session.SetString("Department", user.DisplayDepartment);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            // Store PageUrls
            var pageUrlsJson = JsonSerializer.Serialize(user.PageUrls ?? new List<string>());
            HttpContext.Session.SetString("PageUrls", pageUrlsJson);

            // Store MenuItems
            var menuJson = JsonSerializer.Serialize(user.MenuItems ?? new List<MenuItem>());
            HttpContext.Session.SetString("MenuItems", menuJson);

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Home"),
                loggedUser = user.DisplayName
            });
        }

        [HttpGet]
        public ActionResult Register(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            var branches = _branchService.GetAllActiveBranchList();
            var departments = _departmentService.GetAllActiveDepartmentList();
            var designations = _designationService.GetAllActiveDesignationList();
            var products = _productService.GetAllActiveProductList();

            ViewBag.Products = products
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProductName
            })
            .ToList();

            ViewBag.Branches = branches
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.BranchName
            })
            .ToList();

            ViewBag.Departments = departments
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DepartmentName
            })
            .ToList();

            ViewBag.Designations = designations
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DesignationName
            })
            .ToList();

            return View();
        }

        //create user with method overloading 
        [HttpPost]
        public async Task<IActionResult> Register(IFormCollection collection)
        {
            try
            {
                var (created, userId) = await _userService.CreateUserAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "User created successfully",
                        redirectUrl = Url.Action("Management", "User")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create user"
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
        public async Task<IActionResult> CheckUsername(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _userService.CheckUserNameExists(collection);

            return Json(new { exists = exists });
        }



    }
}
