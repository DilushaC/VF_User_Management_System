using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Web.Controllers
{
    public class DesignationController : Controller
    {
        // GET: DesignationController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DesignationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DesignationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DesignationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: DesignationController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
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
