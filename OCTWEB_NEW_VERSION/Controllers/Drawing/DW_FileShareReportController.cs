using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Drawing
{
    [Authorize]
    public class DW_FileShareReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_dw_fileshared = ConfigurationManager.AppSettings["path_dw_fileshared"];

        //
        // GET: /DW_FileShareReport/FileList
        [CustomAuthorize(24)]//File Share
        public ActionResult FileList(FileShareListModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                int? file_share_id = model.file_name_id;

                IEnumerable<DW_Fileshare> query = model.Page.HasValue ? db.DW_Fileshare : db.DW_Fileshare; //.Take(0)
                if (file_share_id.HasValue)
                {
                    query = query.Where(x => x.SF_Id == file_share_id);
                }

                var fs_list = query.Select(s => new FileShareModel
                {
                    id = s.SF_Id,
                    name = s.SF_name,
                    file = s.SF_file,
                    rev = s.SF_rev,
                    date = s.SF_date,
                    note = s.SF_note,
                    date_str = s.SF_date.ToString("dd/MM/yyyy")

                }).OrderBy(o => o.name).ToList();

                //IPagedList<FileShareModel> fs_pagedList = fs_list.ToPagedList(pageIndex, pageSize);
                //Select Filename
                List<SelectListItem> selectFilename = db.DW_Fileshare.OrderBy(o => o.SF_name)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SF_Id.ToString(),
                        Text = s.SF_name
                    }).ToList();

                model.fileshareList = fs_list;
                model.select_name = selectFilename;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_FileShareReport/FileList {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /DW_FileShareReport/FileList
        [HttpPost]
        [CustomAuthorize(24)]//File Share
        public ActionResult FileList(FormCollection form, FileShareListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? file_share_id = model.file_name_id;

                IEnumerable<DW_Fileshare> query = db.DW_Fileshare;
                if (file_share_id.HasValue)
                {
                    query = query.Where(x => x.SF_Id == file_share_id);
                }

                var fs_list = query.Select(s => new FileShareModel
                {
                    id = s.SF_Id,
                    name = s.SF_name,
                    file = s.SF_file,
                    rev = s.SF_rev,
                    date = s.SF_date,
                    note = s.SF_note,
                    date_str = s.SF_date.ToString("dd/MM/yyyy")

                }).OrderBy(o => o.name).ToList();

                //IPagedList<FileShareModel> fs_pagedList = fs_list.ToPagedList(pageIndex, pageSize);
                //Select Filename
                List<SelectListItem> selectFilename = db.DW_Fileshare.OrderBy(o => o.SF_name)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SF_Id.ToString(),
                        Text = s.SF_name
                    }).ToList();

                model.fileshareList = fs_list;
                model.select_name = selectFilename;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //DW_FileShareReport/FileList {0}", ex.ToString());
                return View("Error");
            }
        }

        [CustomAuthorize(24)]//File Share
        public ActionResult DownloadFile(string fileName)
        {
            string path = path_dw_fileshared + fileName;
            try
            {
                //Read the File data into Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(path);

                //Send the File to Download.
                return File(bytes, "application/octet-stream", fileName);
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                return View("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}