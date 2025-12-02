using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public ActionResult Create()
        {
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
                        redirectUrl = Url.Action("DesignationsManagement", "Designation")
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

        //// POST: DesignationController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

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

                return Ok(new { success = true, message = "Designation updated successfully.", redirectUrl = Url.Action("DesignationsManagement", "Designation") });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: DesignationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DesignationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DesignationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
