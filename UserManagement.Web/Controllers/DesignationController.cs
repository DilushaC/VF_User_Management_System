using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security;
using UserManagement.Business.BranchHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.UserHandler;

namespace UserManagement.Web.Controllers
{
    public class DesignationController : Controller
    {
        private readonly IDesignationService _designationService;
        private readonly IDataTableService _dataTableService;

        public DesignationController(IDesignationService designationService,IDataTableService dataTableService)
        {
            _designationService = designationService;
            _dataTableService = dataTableService;
        }

        [HttpGet]
        public IActionResult Create(string permission)
        {
            bool canEdit = permission?.ToLower() == "true";

            ViewBag.CanEdit = canEdit;

            return View();
        }

        // GET: DesignationController
        public ActionResult Management()
        {
            return View();
        }

        // GET: DesignationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                bool created = await _designationService.CreateDesignationAsync(collection);

                if (created)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Designation created successfully",
                        redirectUrl = Url.Action("Management", "Designation")
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

        // GET: DesignationController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //load data table
        [HttpPost]
        public IActionResult GetDesignationsPaged()
        {
            var dtRequest = _dataTableService.BuildRequest(Request);

            // Build query
            var query = _designationService.GetAllDesignationList().AsQueryable();

            // Custom search (your logic)
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                string s = dtRequest.SearchValue;
                query = query.Where(u =>
                    u.DesignationName.ToLower().Contains(s));
            }

            // Execute paging using common handler
            var response = _dataTableService.ApplyDataTable(query, dtRequest);

            return Json(response);
        }

        //load single user record
        [HttpGet]
        public async Task<IActionResult> LoadEditModal(int id)
        {
            var designation = await _designationService.GetDesignationByIdAsync(id);
            if (designation == null)
            {
                return NotFound();
            }
            return PartialView("_EditDesignationPartial", designation);
        }

        //update single designation record
        [HttpPost]
        public async Task<IActionResult> LoadEditModal(IFormCollection form)
        {
            try
            {
                var result = await _designationService.UpdateDesignationAsync(form);

                if (!result)
                {
                    return Ok(new { success = false, message = "Failed to update Designation." });
                }

                return Ok(new { success = true, message = "Designation updated successfully.", redirectUrl = Url.Action("Management", "Designation") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDesignation(IFormCollection form)
        {
            try
            {
                var result = await _designationService.DeleteDesignationAsync(form);

                if (!result)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Designation is assigned to a User"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Designation deleted successfully.",
                    redirectUrl = Url.Action("Management", "Designation")
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
        public async Task<IActionResult> CheckDesignationName(IFormCollection collection)
        {
            // Pass the form collection directly to the service
            bool exists = await _designationService.CheckDesignationNameExists(collection);

            return Json(new { exists = exists });
        }
    }
}
