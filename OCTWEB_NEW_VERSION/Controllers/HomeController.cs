using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers
{
    public class HomeController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        public ActionResult Index()
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

       


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult UnAuthorized()
        {
            ViewBag.errorMessage = "Access Denied!!";
            return View("Error");
        }
    }
}