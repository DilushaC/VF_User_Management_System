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

        //get all user list draw the initial data table
        public IActionResult UsersManagement()
        {
            return View();
        }

        //dat table APi call to get records
        [HttpPost]
        public IActionResult GetUsersPaged()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var users = _userService.GetAllUsersList();

            // Data table searching
            if (!string.IsNullOrEmpty(searchValue))
            {
                users = users.Where(u =>
                    u.UserName.Contains(searchValue) ||
                    u.FirstName.Contains(searchValue) ||
                    u.LastName.Contains(searchValue) ||
                    u.Email.Contains(searchValue)
                ).ToList();
            }

            int recordsTotal = users.Count();

            // Pagination
            var data = users.Skip(skip).Take(pageSize).Select(u => new
            {
                u.Id,
                u.UserName,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Email,
                u.Phone,
                u.PrimaryBranchName,
                u.PrimaryDepartmentName,
                u.DesignationName,
                IsActive = u.IsActive ? "Active" : "Inactive"
            }).ToList();

            return Json(new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data
            });
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
                var result = await _userService.UpdateUserAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update user."});
                }

                return Ok(new { success = true, message = "User updated successfully.", redirectUrl = Url.Action("UsersManagement", "User") });
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
