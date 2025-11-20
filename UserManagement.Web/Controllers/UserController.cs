using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpPost]
        public ActionResult Register(IFormCollection collection)
        {
            return View();
        }
    }
}
