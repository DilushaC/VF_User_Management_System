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
        //get all user list
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

        //load single user record
        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
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
                // Send the entire form collection to service layer
                var result = await _userService.UpdateUserAsync(form);

                if (!result)
                {
                    return StatusCode(500, "Failed to update user.");
                }

                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
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

        //create user with method overloading 
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
