using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.UserHandler;
using UserManagement.Data.Models;
using UserManagement.Presentation.Filters;

namespace UserManagement.Web.Controllers
{
    [SessionCheck]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            var branches = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Email" },
                new SelectListItem { Value = "2", Text = "Phone" },
                new SelectListItem { Value = "3", Text = "Walk-in" },
                new SelectListItem { Value = "4", Text = "Website Form" }
            };

            ViewBag.Branches = branches;
            return View();
        }
    }
}
