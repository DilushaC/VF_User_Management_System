using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.BranchHandler;
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

        public UserController(IUserService userService, IBranchService branchService)
        {
            _userService = userService;
            _branchService = branchService;
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

        public ActionResult Register()
        {
            var branches = _branchService.GetAllBranchList();

            ViewBag.Branches = branches
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.BranchName
            })
            .ToList();

            return View();
        }
    }
}
