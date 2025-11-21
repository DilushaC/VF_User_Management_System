using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
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

        public UserController(IUserService userService, IBranchService branchService,IDepartmentService departmentService,IDesignationService designationService)
        {
            _userService = userService;
            _branchService = branchService;
            _departmentService = departmentService;
            _designationService = designationService;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public IActionResult UsersManagement()
        {
            var users = _userService.GetAllUsersList();
            if (users != null)
            {
                ViewBag.UserList = users.ToList();
            }
            else
            {
                ViewBag.UserList = new List<UserModel>();
            }
            return View();
        }

        public async Task<IActionResult> LoadEditModal(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditUserPartial", user);
        }

        public ActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            UserModel user = await _userService.ValidateUserAsync(username, password);

            if (user != null)
            {

                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserId", Convert.ToString(user.Id));
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            else
            {
                return Json(new { success = false });
            }

        }



        //load user register page view
        [HttpGet]
        public ActionResult Register()
        {
            var branches = _branchService.GetAllBranchList();
            var departments = _departmentService.GetAllDepartmentList();
            var designations = _designationService.GetAllDesignationList();

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

            return View();
        }

        //create user API
        [HttpPost]
        public async Task<IActionResult> Register(IFormCollection collection)
        {
            try
            {
                bool created = await _userService.CreateUserAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "User created successfully",
                        redirectUrl = Url.Action("Index", "Home")
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

    }
}
