using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public MenuController(IProductService productService, IDataTableService dataTableService, IPageService pageService, IMenuItemService menuItemService)
        {
            _productService = productService;
            _pageService = pageService;
            _dataTableService = dataTableService;
            _menuItemService = menuItemService;
        }

        [HttpGet]
        public IActionResult Create(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            var products = _productService.GetAllActiveProductList();
            var pages = _pageService.GetAllPagesList();
            var parentMenus = _menuItemService.GetAllMenuItemsList();

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

            // Build query
            var query = _pageService.GetAllPagesList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.PageName.ToLower().Contains(s));
            }

            // Execute paging using common handler
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
                        message = "Page created successfully",
                        redirectUrl = Url.Action("Management", "Page")
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

            //viewbag for branches
            ViewBag.Products = products
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProductName
            })
            .ToList();

            var user = await _pageService.GetPageByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditPagePartial", user);
        }


        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _pageService.UpdatePageAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Page." });
                }

                return Ok(new { success = true, message = "Page updated successfully.", redirectUrl = Url.Action("Management", "Page") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckPageName(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _pageService.CheckPageNameExists(collection);

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
