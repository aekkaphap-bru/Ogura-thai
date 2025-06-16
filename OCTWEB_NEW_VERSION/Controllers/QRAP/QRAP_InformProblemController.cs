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

namespace OCTWEB_NET45.Controllers.QRAP
{
    [Authorize]
    public class QRAP_InformProblemController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_qrap_inform = ConfigurationManager.AppSettings["path_qrap_inform"];

        //
        // GET: /QRAP_InformProblem/List
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult List(InformListModel model)
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

                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                int? department_id = model.department_id;
                string problem_no = model.problem_no;
                string detail = model.detail;
                string owner = model.owner;
                int? status = model.status;

                IEnumerable<CM_Problem> query = model.Page.HasValue ? db.CM_Problem : db.CM_Problem.Take(0);
                if(start_date.HasValue)
                {
                    query = query.Where(x => x.PRB_PlanDate >= start_date.Value);
                }
                if (end_date.HasValue)
                {
                    query = query.Where(x => x.PRB_PlanDate <= end_date.Value);
                }
                if (department_id.HasValue)
                {
                    query = query.Where(x => x.Dep_Id == department_id);
                }
                if (!String.IsNullOrEmpty(problem_no))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PRB_Number) 
                        && x.PRB_Number.ToLowerInvariant().Contains(problem_no.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(detail))
                {
                    query = query.Where(x => x.PRB_Description.ToLowerInvariant().Contains(detail.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(owner))
                {
                    query = query.Where(x => x.PRB_Owner.ToLowerInvariant().Contains(owner.ToLowerInvariant()));
                }
                if (status.HasValue)
                {
                    query = query.Where(x => x.PRB_Status == status.Value);
                }
                var pr_list = query.Select(s => new ProblemModel
                    {
                        id = s.PRB_Id,
                        detail = s.PRB_Description,
                        file = s.PRB_FileDetail,
                        reason = s.PRB_Reason,
                        date = s.PRB_PlanDate,
                        status = s.PRB_Status,
                        owner = s.PRB_Owner,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                        problem_no = s.PRB_Number,
                    }).OrderByDescending(o=>o.id).ToList();
                IPagedList<ProblemModel> problemModelPagedList = pr_list.ToPagedList(pageIndex, pageSize);

                //Select Status
                List<SelectListItem> selectStatus = new List<SelectListItem>();
                selectStatus.Add(new SelectListItem { Value = "0", Text = "None operate" });
                selectStatus.Add(new SelectListItem { Value = "1", Text = "Operating" });
                selectStatus.Add(new SelectListItem { Value = "2", Text = "Completed" });

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectStatus = selectStatus;
                model.SelectDepartmet = selectDept;
                model.problemPagedListModel = problemModelPagedList;
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //QRAP_InformProblem/List {0}", ex.ToString());
                return View("Error");
            }   
        }
        //
        // POST: /QRAP_InformProblem/List
        [HttpPost]
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult List(FormCollection form,InformListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                int? department_id = model.department_id;
                string problem_no = model.problem_no;
                string detail = model.detail;
                string owner = model.owner;
                int? status = model.status;

                IEnumerable<CM_Problem> query = db.CM_Problem ;
                if (start_date.HasValue)
                {
                    query = query.Where(x => x.PRB_PlanDate >= start_date.Value);
                }
                if (end_date.HasValue)
                {
                    query = query.Where(x => x.PRB_PlanDate <= end_date.Value);
                }
                if (department_id.HasValue)
                {
                    query = query.Where(x => x.Dep_Id == department_id);
                }
                if (!String.IsNullOrEmpty(problem_no))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PRB_Number)
                        && x.PRB_Number.ToLowerInvariant().Contains(problem_no.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(detail))
                {
                    query = query.Where(x => x.PRB_Description.ToLowerInvariant().Contains(detail.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(owner))
                {
                    query = query.Where(x => x.PRB_Owner.ToLowerInvariant().Contains(owner.ToLowerInvariant()));
                }
                if (status.HasValue)
                {
                    query = query.Where(x => x.PRB_Status == status.Value);
                }
                var pr_list = query.Select(s => new ProblemModel
                {
                    id = s.PRB_Id,
                    detail = s.PRB_Description,
                    file = s.PRB_FileDetail,
                    reason = s.PRB_Reason,
                    date = s.PRB_PlanDate,
                    status = s.PRB_Status,
                    owner = s.PRB_Owner,
                    department_id = s.Dep_Id,
                    department = (from dept in db.Departments
                                  where dept.Dep_Id == s.Dep_Id
                                  select dept.Dep_Name).FirstOrDefault(),
                    problem_no = s.PRB_Number,
                }).OrderByDescending(o=>o.id).ToList();
                IPagedList<ProblemModel> problemModelPagedList = pr_list.ToPagedList(pageIndex, pageSize);

                //Select Status
                List<SelectListItem> selectStatus = new List<SelectListItem>();
                selectStatus.Add(new SelectListItem { Value = "0", Text = "None operate" });
                selectStatus.Add(new SelectListItem { Value = "1", Text = "Operating" });
                selectStatus.Add(new SelectListItem { Value = "2", Text = "Completed" });

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectStatus = selectStatus;
                model.SelectDepartmet = selectDept;
                model.problemPagedListModel = problemModelPagedList;
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //QRAP_InformProblem/List {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: QRAP_InformProblem/Create
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult Create()
        {
            try
            {
                ProblemModel model = new ProblemModel();
               
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmet = selectDept;

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_InformProblem/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: QRAP_InformProblem/Create
        [HttpPost]
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult Create(HttpPostedFileBase file, FormCollection form, ProblemModel model)
        {
            try
            {
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmet = selectDept;

                if (ModelState.IsValid)
                {
                    CM_Problem pr = new CM_Problem();
                    pr.PRB_Description = model.detail;
                    pr.PRB_Reason = model.reason;
                    pr.PRB_PlanDate = model.date;
                    pr.PRB_Owner = model.owner;
                    pr.Dep_Id = model.department_id;
                    pr.PRB_Status = 0;
                    pr.PRB_Owner = Session["NickName"] != null ? Session["NickName"].ToString() : null;

                    if (file != null)
                    {
                        var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };
                        //Concat filename
                        DateTime dateNow = DateTime.Now;
                        string date_str = dateNow.ToString("yyMMddHHMMss");
                        string _FileName = String.Concat("problem_",date_str, Path.GetExtension(file.FileName));
                        string _path = Path.Combine(path_qrap_inform, _FileName);

                        var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                        if (!supportedTypes.Contains(fileExt))
                        {
                            ViewBag.Message = "Invalid file extension, Only word, PDF, excel, text and image (.jpg, .jpeg, .png) files.";
                            return View(model);
                        }
                        //Set path file
                        pr.PRB_FileDetail = _FileName;
                        file.SaveAs(_path);
                    }
                    else
                    {
                        ViewBag.Message = String.Format("Upload file is required.");
                        return View(model);
                    }
                    //Save
                    var result = db.CM_Problem.Add(pr);
                    db.SaveChanges();

                    var num = result.PRB_Id;
                    if(num > 0){
                        string num_str = num.ToString();
                        string problem_number = num_str.PadLeft(7, '0');
                        result.PRB_Number = problem_number;
                        db.Entry(result).State = System.Data.Entity.EntityState.Modified;
                    }

                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("id:", result.PRB_Id
                                        , "/Date:", result.PRB_PlanDate.ToString("yyy-MM-dd")
                                        , "/Dept:", result.Dep_Id
                                        , "/Problem", result.PRB_Description);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Inform Problem",
                        Log_System = "QRAP",
                        Log_Detail = log_detail,
                        Log_Action_Id = result.PRB_Id,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully created, {0}", model.detail);
                    return RedirectToAction("List"
                        , new InformListModel { problem_no = result.PRB_Number, Page = 1 });
                }
             
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_InformProblem/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /QRAP_InformProblem/Edit
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult Edit(int id)
        {
            try
            {
                ProblemModel model = db.CM_Problem.Where(x => x.PRB_Id == id && x.PRB_Status == 0)
                    .Select(s => new ProblemModel
                    {
                        id = s.PRB_Id,
                        detail = s.PRB_Description,
                        file = s.PRB_FileDetail,
                        reason = s.PRB_Reason,
                        date = s.PRB_PlanDate,
                        status = s.PRB_Status,
                        owner = s.PRB_Owner,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                        problem_no = s.PRB_Number,
                    }).FirstOrDefault();

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmet = selectDept;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_InformProblem/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /QRAP_InformProblem/Edit
        [HttpPost]
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult Edit(HttpPostedFileBase file, FormCollection form, ProblemModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CM_Problem pr = db.CM_Problem.Where(x => x.PRB_Id == model.id ).FirstOrDefault();
                    pr.PRB_Description = model.detail;
                    pr.PRB_Reason = model.reason;
                    pr.PRB_PlanDate = model.date;
                    pr.PRB_Owner = Session["NickName"] != null ? Session["NickName"].ToString() : null;
                    pr.Dep_Id = model.department_id;

                    string old_file = pr.PRB_FileDetail;

                    if (file != null)
                    {
                        var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };
                        //Concat filename
                        DateTime dateNow = DateTime.Now;
                        string date_str = dateNow.ToString("yyMMddHHMMss");
                        string _FileName = String.Concat("problem_", date_str, Path.GetExtension(file.FileName));
                        string _path = Path.Combine(path_qrap_inform, _FileName);

                        var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                        if (!supportedTypes.Contains(fileExt))
                        {
                            ViewBag.Message = "Invalid file extension, Only word, PDF, excel, text and image (.jpg, .jpeg, .png) files.";
                            return View(model);
                        }
                        //Set path file
                        pr.PRB_FileDetail = _FileName;
                        file.SaveAs(_path);

                        //Delete Old file
                        string _path_old = Path.Combine(path_qrap_inform, old_file);
                        if (System.IO.File.Exists(_path_old))
                        {
                            System.IO.File.Delete(_path_old);
                        }
                    }
                    //Save edit
                    db.Entry(pr).State = System.Data.Entity.EntityState.Modified;

                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("id:", pr.PRB_Id
                                        , "/Date:", pr.PRB_PlanDate.ToString("yyy-MM-dd")
                                        , "/Dept:", pr.Dep_Id
                                        , "/Problem", pr.PRB_Description);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "edit",
                        Log_Type = "Inform Problem",
                        Log_System = "QRAP",
                        Log_Detail = log_detail,
                        Log_Action_Id = pr.PRB_Id,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.detail);
                    return RedirectToAction("List"
                        , new InformListModel { problem_no = pr.PRB_Number, Page = 1 });
                }

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmet = selectDept;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_InformProblem/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //POST: /QRAP_InformProblem/Delete
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var pr_list = db.CM_Problem.Where(x => id_list.Contains(x.PRB_Id) && x.PRB_Status == 0).ToList();
                    foreach(var pr in pr_list)
                    {
                        //Remove
                        db.CM_Problem.Remove(pr);
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("id:", pr.PRB_Id
                                            , "/Date:", pr.PRB_PlanDate.ToString("yyy-MM-dd")
                                            , "/Dept:", pr.Dep_Id
                                            , "/Problem", pr.PRB_Description);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Inform Problem",
                            Log_System = "QRAP",
                            Log_Detail = log_detail,
                            Log_Action_Id = pr.PRB_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Delete file
                        string _path = Path.Combine(path_qrap_inform, pr.PRB_FileDetail);
                        if (System.IO.File.Exists(_path))
                        {
                            System.IO.File.Delete(_path);
                        }
                    }
                    if (pr_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return RedirectToAction("List");
                }
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_InformProblem/Delete {0}", ex.ToString());
                return View("Error");
            }
        }


        //Load file
        [CustomAuthorize(5)]//1 Inform Problem
        public ActionResult DownloadFile(string fileName)
        {
            string path = path_qrap_inform + fileName;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    //Read the File data into Byte Array.
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    //Send the File to Download.
                    return File(bytes, "application/octet-stream", fileName);
                }
                ViewBag.errorMessage = String.Format("Could not find file.");
                return View("Error");
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                return View("Error");
            }
        }

        // POST: /QRAP_InformProblem/GetProblemNo
        [HttpPost]
        public JsonResult GetProblemNo(string Prefix)
        {
            var problem = db.CM_Problem
                    .Where(x => x.PRB_Number.StartsWith(Prefix))
                    .Take(10)
                    .Select(s => new { label = s.PRB_Number, val = s.PRB_Number }).ToList();
            return Json(problem);
        }
        // POST: /QRAP_InformProblem/GetNickName
        [HttpPost]
        public JsonResult GetNickName(string Prefix)
        {
            var nn = db.EmpLists
                .Where(x => x.NName_EN.StartsWith(Prefix))
                .Take(10)
                .Select(s => new { label = s.NName_EN, val = s.NName_EN }).ToList();
            return Json(nn);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}