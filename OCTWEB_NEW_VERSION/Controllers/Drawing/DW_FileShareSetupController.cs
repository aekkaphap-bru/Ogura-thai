using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Drawing
{
    [Authorize]
    public class DW_FileShareSetupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_dw_fileshared = ConfigurationManager.AppSettings["path_dw_fileshared"];

        // GET: /DW_FileShareSetup/FileList
        [CustomAuthorize(25)]//Upload File Share
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
                if(file_share_id.HasValue)
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
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_FileShareSetup/FileList {0}", ex.ToString());
                return View("Error");
            }

        }
        //
        // POST: /DW_FileShareSetup/FileList
        [HttpPost]
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult FileList(FormCollection form,FileShareListModel model)
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
                ViewBag.errorMessage = String.Format("Error: Post //DW_FileShareSetup/FileList {0}", ex.ToString());
                return View("Error");
            }

        }

        //
        // GET: DW_FileShareSetup/Create
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult Create()
        {
            try
            {
                FileShareModel model = new FileShareModel();
                model.date = DateTime.Today;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("Error: Get //DW_FileShareSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: DW_FileShareSetup/Create
        [HttpPost]
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult Create(HttpPostedFileBase file, FormCollection form, FileShareModel model)
        {
            try
            {
                var supportedTypes = new[] { "txt", "doc", "docx", "xls", "xlsx" };

                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        string name_str = model.name.Replace(" ", "");
                        string _FileName = String.Concat(name_str, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                        string _path = Path.Combine(path_dw_fileshared, _FileName);
                        var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                       
                        // Get a list of invalid file characters.
                       /* string pattern = "^[a-zA-Z0-9_.\\s-]*$"; 
                        Match match = Regex.Match(_FileName, pattern, RegexOptions.IgnoreCase);
                        
                        if (!match.Success)
                        {
                            ViewBag.Message = String.Format("The following characters are invalid in a filename", "a-z,0-9");
                            return View(model);
                        }
                        */ 
                        if (!supportedTypes.Contains(fileExt))
                        {
                            ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                            return View(model);
                        }
                        if (System.IO.File.Exists(_path))
                        {
                            ViewBag.Message = String.Format("The {0} file already exists.", _FileName);
                            return View(model);
                        }
                        DW_Fileshare fs = new DW_Fileshare();
                        fs.SF_name = model.name;
                        fs.SF_rev = model.rev;
                        fs.SF_file = _FileName;
                        fs.SF_note = model.note;
                        fs.SF_date = model.date;

                        var result = db.DW_Fileshare.Add(fs);
                        //Save Logs
                        if(result != null)
                        {
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            Log logmodel = new Log()
                            {
                                Log_Action = "add",
                                Log_Type = "Upload File Share",
                                Log_System = "Drawing",
                                Log_Detail = String.Concat(result.SF_name).Length <= 240 ? 
                                    String.Concat(result.SF_name) : String.Concat(result.SF_name).Substring(0, 240),
                                Log_Action_Id = result.SF_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }
                        //Save Changes
                        db.SaveChanges();
                        file.SaveAs(_path);

                        TempData["shortMessage"] = String.Format("Successfully created, {0}", model.name);
                        return RedirectToAction("FileList");
                    }
                    else
                    {
                        ViewBag.Message = String.Format("Upload file is required.");
                        return View(model);
                    }

                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("Error: Post //DW_FileShareSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET : /DW_FileShareSetup/FileList
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult Edit(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                var model = db.DW_Fileshare.Where(x => x.SF_Id == id)
                    .Select(s => new FileShareModel
                    {
                        id = s.SF_Id,
                        name = s.SF_name,
                        file = s.SF_file,
                        date = s.SF_date,
                        rev = s.SF_rev,
                        note = s.SF_note,
                    }).FirstOrDefault();

                return View(model);
                
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("Error: Get //DW_FileShareSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_FileShareSetup/Edit
        [HttpPost]
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult Edit(HttpPostedFileBase file, FormCollection form, FileShareModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    DW_Fileshare fs = db.DW_Fileshare.Where(x => x.SF_Id == model.id).FirstOrDefault();
                    if (fs != null)
                    {
                        string file_old = fs.SF_file;

                        fs.SF_name = model.name;
                        fs.SF_rev = model.rev;
                        fs.SF_date = model.date;
                        fs.SF_note = model.note;
                        if(file != null)
                        {
                            var supportedTypes = new[] { "txt", "doc", "docx", "xls", "xlsx" };
                            string name_str = model.name.Replace(" ", "");
                            string _FileName = String.Concat(name_str, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                            string _path = Path.Combine(path_dw_fileshared, _FileName);
                            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                                return View(model);
                            }
                            //Delete old file
                            if(file_old != _FileName)
                            {
                                string _path_old = Path.Combine(path_dw_fileshared, file_old);
                                if (System.IO.File.Exists(_path_old))
                                {  
                                    System.IO.File.Delete(_path_old); 
                                }
                            }
                           
                            fs.SF_file = _FileName;
                            file.SaveAs(_path);
                        }
                        //Save Edit
                        db.Entry(fs).State = System.Data.Entity.EntityState.Modified;

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "edit",
                            Log_Type = "Upload File Share",
                            Log_System = "Drawing",
                            Log_Detail = String.Concat(fs.SF_name),
                            Log_Action_Id = fs.SF_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                       
                        //Save
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.name);
                        return RedirectToAction("FileList");

                    }
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("Error: Post //DW_FileShareSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: DW_FileShareSetup/Delete
        [CustomAuthorize(25)]//Upload File Share
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var fs_list = db.DW_Fileshare.Where(x => id_list.Contains(x.SF_Id)).ToList();

                    foreach (var fs in fs_list)
                    {                        
                        //Remove
                        db.DW_Fileshare.Remove(fs);

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("filename:", fs.SF_name, "/rev:", fs.SF_rev, "/date", fs.SF_date.ToString("yyyy-MM-dd"));
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Upload File Share",
                            Log_System = "Drawing",
                            Log_Detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240),
                            Log_Action_Id = fs.SF_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Delete file
                        string _FileName = fs.SF_file;
                        if(!String.IsNullOrEmpty(_FileName))
                        {
                            string _path = Path.Combine(path_dw_fileshared, _FileName);
                            if (System.IO.File.Exists(_path))
                            {
                                System.IO.File.Delete(_path);
                            }
                        }                       
                    }
                    if (fs_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }

                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return RedirectToAction("FileList");
                }
                return RedirectToAction("FileList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        [CustomAuthorize(25)]//Upload File Share
        public ActionResult DownloadFile(string fileName)
        {
            //Build the File Path.
            //string path = Server.MapPath("~/File/") + fileName;  --visual path
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