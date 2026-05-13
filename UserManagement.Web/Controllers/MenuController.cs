using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.PageHandler;
using UserManagement.Business.ProductHandler;
using UserManagement.Business.UserHandler;

namespace UserManagement.Web.Controllers
{
    public class MenuController : Controller
    {
        private readonly IProductService _productService;
        private readonly IPageService _pageService;
        private readonly IDataTableService _dataTableService;
        private readonly IMenuItemService _menuItemService;
        private readonly IConfiguration _configuration;

        public MenuController(IProductService productService, IDataTableService dataTableService, IPageService pageService, IMenuItemService menuItemService, IConfiguration configuration)
        {
            _productService = productService;
            _pageService = pageService;
            _dataTableService = dataTableService;
            _menuItemService = menuItemService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Create(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            var products = _productService.GetAllActiveProductList();
            var pages = _pageService.GetAllPagesList();
            var parentMenus = _menuItemService.GetAllMenuItemsList();
            var categories = _menuItemService.GetAllMenuCategoryList();

            //viewbag for branches
            ViewBag.Products = products
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProductName
            })
            .ToList();

            ViewBag.Pages = pages
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.PageName
            })
            .ToList();

            ViewBag.ParentMenus = parentMenus
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.MenuTitle
            })
            .ToList();

            ViewBag.Categories = categories
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.CategoryName
            })
            .ToList();

            return View();
        }

        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetPagesPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);
            var query = _menuItemService.GetAllMenuList().AsQueryable();

            // ── Product dropdown filter ───────────────────────────────
            var productIdStr = Request.Form["productId"].ToString();
            if (!string.IsNullOrWhiteSpace(productIdStr) &&
                int.TryParse(productIdStr, out int productId))
            {
                query = query.Where(u => u.ProductId == productId);
            }

            // ── DataTable global search ───────────────────────────────
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue.ToLower();
                query = query.Where(u =>
                    (u.MenuTitle != null && u.MenuTitle.ToLower().Contains(s)) ||
                    (u.ProductName != null && u.ProductName.ToLower().Contains(s))
                );
            }

            var response = _dataTableService.ApplyDataTable(query, dtRequest);
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _menuItemService.CreateMenuItemAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Menu created successfully",
                        redirectUrl = Url.Action("Management", "Menu")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create Page"
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

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var products = _productService.GetAllActiveProductList();
            var pages = _pageService.GetAllPagesList();
            var parentMenus = _menuItemService.GetAllMenuItemsList();
            var categories = _menuItemService.GetAllMenuCategoryList();

            ViewBag.Products = products
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProductName
            })
            .ToList();

            ViewBag.Pages = pages
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.PageName
            })
            .ToList();

            ViewBag.ParentMenus = parentMenus
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.MenuTitle
            })
            .ToList();

            ViewBag.Categories = categories
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.CategoryName
            })
            .ToList();

            var user = await _menuItemService.GetMenuByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditMenuPartial", user);
        }


        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _menuItemService.UpdateMenuAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Menu." });
                }

                return Ok(new { success = true, message = "Menu updated successfully.", redirectUrl = Url.Action("Management", "Menu") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMenu(IFormCollection form)
        {
            try
            {
                var result = await _menuItemService.DeleteMenuAsync(form);

                if (!result)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Failed to delete menu."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Menu deleted successfully.",
                    redirectUrl = Url.Action("Management", "Menu")
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
        public async Task<IActionResult> CheckMenuTitle(IFormCollection collection)
        {
            bool exists = await _menuItemService.CheckMenuTitle(collection);

            return Json(new { exists = exists });
        }

        [HttpPost]
        public async Task<IActionResult> CheckPageLevel(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _pageService.CheckPageLevelExists(collection);

            return Json(new { exists = exists });
        }
    }
}
