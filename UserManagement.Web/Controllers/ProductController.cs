using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.ProductHandler;
using UserManagement.Business.UserHandler;

namespace UserManagement.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IDataTableService _dataTableService;

        public ProductController(IProductService productService,IDataTableService dataTableService)
        {
            _productService = productService;
            _dataTableService = dataTableService;
        }

        [HttpGet]
        public IActionResult Create(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            return View();
        }

        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _productService.CreateProductAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Product created successfully",
                        redirectUrl = Url.Action("Management", "Product")
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
        public IActionResult GetProductsPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _productService.GetAllProductList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.ProductName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return PartialView("_EditProductPartial", product);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Product." });
                }

                return Ok(new { success = true, message = "Product updated successfully.", redirectUrl = Url.Action("Management", "Product") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(IFormCollection form)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(form);

                if (!result)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Product is assigned to a User"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Product deleted successfully.",
                    redirectUrl = Url.Action("Management", "Product")
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
        public async Task<IActionResult> CheckProductName(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _productService.CheckProductNameExists(collection);

            return Json(new { exists = exists });
        }


    }
}
