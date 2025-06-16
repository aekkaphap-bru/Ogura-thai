using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System.Linq;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.DocumentControll
{
    [Authorize]
    [CustomAuthorize(73)]
    public class DocumentController : Controller
    {


        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        public ActionResult List()
        {
            return View();
        }

        public ActionResult Create()
        {
            var model = new DocumentFormViewModel();

            var areas = db.WS_Training_Section.Select(s => new AreaItemViewModel
            {
                Id = s.Id,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                IsSelected = false
            }).ToList();

            model.AvailableAreas = areas;

            return View(model);
        }

    }
}
