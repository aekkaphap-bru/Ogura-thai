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
    public class QRAP_ApproveController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_qrap_answers = ConfigurationManager.AppSettings["path_qrap_answers"];
        private string path_qrap_inform = ConfigurationManager.AppSettings["path_qrap_inform"];
        //
        // GET: /QRAP_Approve/List
        [CustomAuthorize(7)]//4 Approve
        public ActionResult List(ApproveListModel model)
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
                    date_str = s.PRB_PlanDate.ToString("yyyy-MM-dd"),
                }).OrderByDescending(o => o.id).ToList();
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
                model.problemList = pr_list;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //QRAP_Approve/List {0}", ex.ToString());
                return View("Error");
            } 
        }
        //
        // POST: /QRAP_Approve/List
        [HttpPost]
        [CustomAuthorize(7)]//4 Approve
        public ActionResult List(FormCollection form, ApproveListModel model)
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

                IEnumerable<CM_Problem> query = db.CM_Problem;
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
                    date_str = s.PRB_PlanDate.ToString("yyyy-MM-dd"),
                }).OrderByDescending(o => o.id).ToList();
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
                model.problemList = pr_list;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //QRAP_Approve/List {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /QRAP_Approve/Solutions
        [CustomAuthorize(7)]//4 Approve
        public ActionResult Solutions(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                ProblemModel model = db.CM_Problem.Where(x => x.PRB_Id == id)
                    .Select(s => new ProblemModel
                    {
                        id = s.PRB_Id,
                        detail = s.PRB_Description,
                        file = s.PRB_FileDetail,
                        reason = s.PRB_Reason,
                        date = s.PRB_PlanDate,
                        owner = s.PRB_Owner,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                        problem_no = s.PRB_Number,
                    }).FirstOrDefault();

                List<SolutionModel> solulist = db.CM_TodoList.Where(x => x.PRB_Id == id)
                    .Select(s => new SolutionModel
                    {
                        solution_id = s.TDO_Id,
                        description = s.TDO_Description,
                        response_owner = s.TDO_Owner,
                        define_by = s.TDO_DefineBy,
                        date = s.TDO_PlanDate.HasValue ? s.TDO_PlanDate.Value : DateTime.Today,
                        need_evidence = s.TDO_NeedEvidence.HasValue ? s.TDO_NeedEvidence.Value > 0 ? true : false : false,
                        problem_id = s.PRB_Id,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                        status = (from tde in db.CM_TodoEvidence
                                  where tde.TDO_Id == s.TDO_Id
                                  select tde.TDE_Status).FirstOrDefault().Trim(),

                    }).ToList();

                model.SolutionList = solulist;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_Approve/Solutions {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /QRAP_Approve/Create
        [CustomAuthorize(7)]//4 Approve
        public ActionResult Create(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                ApproveModel model = new ApproveModel();
                var ans = db.CM_TodoEvidence.Where(x => x.TDO_Id == id).FirstOrDefault();
                if (ans != null)
                {
                    model.answer_id = ans.TDE_Id;
                    model.answer = ans.TDE_Answer;
                    model.file_evidence = ans.TDE_Evidence;
                    model.answer_date = ans.TDE_AnswerDate;
                    model.comment = ans.TDE_Comment;
                    model.status = ans.TDE_Status;
                    model.solution_id = ans.TDO_Id;

                    model.approver_date = ans.TDE_ApprovedDate.HasValue ? ans.TDE_ApprovedDate.Value : DateTime.Now;
                    model.approver = !String.IsNullOrEmpty(ans.TDE_Approver) ? ans.TDE_Approver 
                        : Session["NickName"] != null ? Session["NickName"].ToString() : null;
                }

                SolutionModel so_model = db.CM_TodoList.Where(x => x.TDO_Id == id)
                    .Select(s => new SolutionModel
                    {
                        solution_id = s.TDO_Id,
                        description = s.TDO_Description,
                        response_owner = s.TDO_Owner,
                        define_by = s.TDO_DefineBy,
                        date = s.TDO_PlanDate.HasValue ? s.TDO_PlanDate.Value : DateTime.Today,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                        need_evidence = s.TDO_NeedEvidence.HasValue ? s.TDO_NeedEvidence.Value > 0 ? true : false : false,
                        problem_id = s.PRB_Id,

                    }).FirstOrDefault();

                model.solution_id = so_model.solution_id;
                model.solution_detail = so_model.description;
                model.solution_department = so_model.department;
                model.solution_response_name = so_model.response_owner;
                model.solution_date = so_model.date;
                model.need_evidence = so_model.need_evidence;

                ProblemModel pr_model = db.CM_Problem.Where(x => x.PRB_Id == so_model.problem_id)
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

                model.problem_id = pr_model.id;
                model.problem_no = pr_model.problem_no;
                model.problem_detail = pr_model.detail;
                model.problem_reason = pr_model.reason;
                model.problem_department = pr_model.department;
                model.problem_date = pr_model.date;
                model.problem_file = pr_model.file;

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_Approve/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /QRAP_Approve/Create
        [HttpPost]
        [CustomAuthorize(7)]//4 Approve
        public ActionResult Create(HttpPostedFileBase file, ApproveModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CM_TodoEvidence ev = new CM_TodoEvidence();

                    if (model.answer_id > 0)
                    {
                        ev = db.CM_TodoEvidence.Where(x => x.TDE_Id == model.answer_id).FirstOrDefault();
                        string status = ev.TDE_Status;
                        if (status.Trim() == "2")
                        {
                            ev.TDE_Status = "1";
                            //Save Problem Status
                            var pr_model = db.CM_Problem.Where(x => x.PRB_Id == model.problem_id).FirstOrDefault();
                            pr_model.PRB_Status = 1;
                            db.Entry(pr_model).State = System.Data.Entity.EntityState.Modified;
                            TempData["shortMessage"] = String.Format("Successfully un-approved, {0}", model.answer);
                        }
                        else
                        {
                            ev.TDE_Status = "2";
                            ev.TDE_ApprovedDate = DateTime.Now;
                            ev.TDE_Approver = Session["NickName"] != null ? Session["NickName"].ToString() : null;
                            TempData["shortMessage"] = String.Format("Successfully approved, {0}", model.answer);
                        }
                        if(!String.IsNullOrEmpty(model.comment))
                        {
                            ev.TDE_Comment = model.comment;  
                        }    
                       
                        //Save edit
                        db.Entry(ev).State = System.Data.Entity.EntityState.Modified;
                    }
                    
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("id:", ev.TDE_Id
                                        , "/Approve", ev.TDE_Approver
                                        , "/Comment", ev.TDE_Comment);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Approve",
                        Log_System = "QRAP",
                        Log_Detail = log_detail,
                        Log_Action_Id = ev.TDE_Id,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save
                    db.SaveChanges();

                    //Check Problem Status
                    var so_list = db.CM_TodoList.Where(x => x.PRB_Id == model.problem_id).Select(s => s.TDO_Id).ToList();
                    var an_list = db.CM_TodoEvidence.Where(x => so_list.Contains(x.TDO_Id)).Select(s => s.TDE_Status).ToList();
                    if (so_list.Count() == an_list.Count())
                    {
                        if (an_list.All(a => a.Trim() == "2"))
                        {  //Save Change Status
                            var pr_model = db.CM_Problem.Where(x => x.PRB_Id == model.problem_id).FirstOrDefault();
                            pr_model.PRB_Status = 2;
                            db.Entry(pr_model).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                  
                    return RedirectToAction("Solutions", new { id = model.problem_id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_Approved/Create {0}", ex.ToString());
                return View("Error");
            }
        }


        //DownLoad file
        [CustomAuthorize(7)]//4 Approve
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
        //DownLoad file
        [CustomAuthorize(7)]//4 Approve
        public ActionResult DownloadFileEvidence(string fileName)
        {
            string path = path_qrap_answers + fileName;
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


        // POST: /QRAP_Approve/GetProblemNo
        [HttpPost]
        public JsonResult GetProblemNo(string Prefix)
        {
            var problem = db.CM_Problem
                    .Where(x => x.PRB_Number.StartsWith(Prefix))
                    .Take(10)
                    .Select(s => new { label = s.PRB_Number, val = s.PRB_Number }).ToList();
            return Json(problem);
        }
        // POST: /QRAP_Approve/GetNickName
        [HttpPost]
        public JsonResult GetNickName(string Prefix)
        {
            var nn = db.EmpLists
                .Where(x => x.NName_EN.StartsWith(Prefix))
                .Take(10)
                .Select(s => new { label = s.NName_EN, val = s.NName_EN }).ToList();
            return Json(nn);
        }

        //GET: /QRAP_Approve/GetDatail
        public JsonResult GetDatail(int id)
        {
            //Select Solution List
            IEnumerable<CM_TodoList> query_so = db.CM_TodoList.Where(x => x.PRB_Id == id);
            List<int> so_id = query_so.Select(s => s.TDO_Id).ToList();
            //Select Evidence List
            IEnumerable<CM_TodoEvidence> query_ev = db.CM_TodoEvidence.Where(x => so_id.Contains(x.TDO_Id));
            //Join model
            List<DataTableDetailModel> result = query_so.Join(query_ev, so => so.TDO_Id, ev => ev.TDO_Id, (so, ev) => new {so=so,ev=ev })
                .Select(s => new DataTableDetailModel
                {
                    solution_id = s.so.TDO_Id,
                    solution_detail = s.so.TDO_Description,
                    plan_date = s.so.TDO_PlanDate.HasValue ? s.so.TDO_PlanDate.Value.ToString("yyyy-MM-dd") : "",
                    department = (from dept in db.Departments where dept.Dep_Id == s.so.Dep_Id select dept.Dep_Name).FirstOrDefault(),
                    approver = s.ev.TDE_Approver,
                    approve_date = s.ev.TDE_ApprovedDate.HasValue ? s.ev.TDE_ApprovedDate.Value.ToString("yyyy-MM-dd") : "",
                    answer = s.ev.TDE_Answer,
                    evidence_file = s.ev.TDE_Evidence,
                    rev = s.ev.TDE_Revision,
                    comment = s.ev.TDE_Comment,
                    status = s.ev.TDE_Status.Trim(),
                }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}