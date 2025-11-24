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
    public class PageController : Controller
    {
        private readonly IProductService _productService;
        private readonly IPageService _pageService;
        private readonly IDataTableService _dataTableService;

        public PageController(IProductService productService,IDataTableService dataTableService,IPageService pageService)
        {
            _productService = productService;
            _pageService = pageService;
            _dataTableService = dataTableService;
        }
        public IActionResult Index()
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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _pageService.CreatePageAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Page created successfully",
                        redirectUrl = Url.Action("ProductsManagement", "Product")
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
    }
}
