using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.QRAP
{
    [Authorize]
    public class QRAP_DefineSolutionController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        //
        // GET: /QRAP_DefineSolution/List
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult List(DefineSolutionListModel model)
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
                ViewBag.errorMessage = String.Format("Error: Get //QRAP_DefineSolution/List {0}", ex.ToString());
                return View("Error");
            } 
        }

        //
        // POST: /QRAP_DefineSolution/List
        [HttpPost]
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult List(FormCollection form, DefineSolutionListModel model)
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
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //QRAP_DefineSolution/List {0}", ex.ToString());
                return View("Error");
            }
        }

        //GET: /QRAP_DefineSolution/DefineSolution
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult DefineSolution(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                ProblemModel model = db.CM_Problem.Where(x=>x.PRB_Id == id)
                    .Select(s=> new ProblemModel
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

                List<SolutionModel> solulist = db.CM_TodoList.Where(x=>x.PRB_Id == id)
                    .Select(s=>new SolutionModel
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
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_DefineSolution/DefineSolution {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /QRAP_DefineSolution/Create
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult Create(int id)
        {
            try
            {
                SolutionModel model = new SolutionModel();
                model.problem_id = id;

                //Select Response Name
                List<SelectListItem> selectName = db.UserDetails
                    .Select(s => new SelectListItem
                    {
                        Value = s.USE_Usercode.ToString(),
                        Text = String.Concat(s.USE_Usercode.ToString()
                        , " : ", s.USE_NickName, " ", s.USE_FName, " ", s.USE_LName, " : "
                        , (from dept in db.Departments where dept.Dep_Id == s.Dep_Id select dept.Dep_Name).FirstOrDefault())
                    }).ToList();
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s=> new SelectListItem{Value = s.Dep_Id.ToString(), Text = s.Dep_Name}).ToList();

                model.SelectResponseOwner = selectName;
                model.SelectDepartmet = selectDept;

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_DefineSolution/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /QRAP_DefineSolution/Create
        [HttpPost]
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult Create(FormCollection form, SolutionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Update problem status
                    var problem_model = db.CM_Problem.Where(x => x.PRB_Id == model.problem_id).FirstOrDefault();
                    problem_model.PRB_Status = 1;
                    db.Entry(problem_model).State = System.Data.Entity.EntityState.Modified;
                    
                    CM_TodoList pr = new CM_TodoList();
                    pr.TDO_Description = model.description;
                    pr.TDO_Owner = model.response_owner;
                    pr.TDO_DefineBy = Session["NickName"] != null ? Session["NickName"].ToString() : null;
                    pr.TDO_PlanDate = model.date;
                    pr.TDO_NeedEvidence = model.need_evidence ? 1 : 0;
                    pr.Dep_Id = model.department_id;
                    pr.PRB_Id = model.problem_id;
                    //Save
                    var result = db.CM_TodoList.Add(pr);
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("id:", result.PRB_Id
                        , "/Date:", result.TDO_PlanDate.HasValue ? result.TDO_PlanDate.Value.ToString("yyy-MM-dd"): ""
                        , "/Dept:", result.Dep_Id
                        , "/Solution", result.TDO_Description);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Define Solution",
                        Log_System = "QRAP",
                        Log_Detail = log_detail,
                        Log_Action_Id = result.TDO_Id,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    //Save
                    db.SaveChanges();

                    //Add New Evidence
                    CM_TodoEvidence evi = new CM_TodoEvidence();
                    evi.TDO_Id = result.TDO_Id;
                    evi.TDE_Status = "0";
                    evi.TDE_Parent = result.TDO_Id;
                    db.CM_TodoEvidence.Add(evi);
                    db.SaveChanges();

                    //Sent Mail
                    model.solution_id = result.TDO_Id;
                    SendEmail(model);

                    TempData["shortMessage"] = String.Format("Successfully created, {0}", model.description);
                    return RedirectToAction("DefineSolution", new { id = model.problem_id});
                }
                //Select Response Name
                List<SelectListItem> selectName = db.UserDetails
                    .Select(s => new SelectListItem
                    {
                        Value = s.USE_Usercode.ToString(),
                        Text = String.Concat(s.USE_Usercode.ToString()
                        , " : ", s.USE_NickName, " ", s.USE_FName, " ", s.USE_LName, " : "
                        , (from dept in db.Departments where dept.Dep_Id == s.Dep_Id select dept.Dep_Name).FirstOrDefault())
                    }).ToList();
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectResponseOwner = selectName;
                model.SelectDepartmet = selectDept;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_DefineSolution/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /QRAP_DefineSolution/Edit
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult Edit(int id)
        {
            try
            {
                SolutionModel model = db.CM_TodoList.Where(x=>x.TDO_Id == id)
                    .Select(s => new SolutionModel
                    {
                        solution_id = s.TDO_Id,
                        description = s.TDO_Description,
                        response_owner = s.TDO_Owner,
                        define_by = s.TDO_DefineBy,
                        date = s.TDO_PlanDate.HasValue ? s.TDO_PlanDate.Value : DateTime.Today,
                        department_id = s.Dep_Id,
                        need_evidence = s.TDO_NeedEvidence.HasValue ? s.TDO_NeedEvidence > 0 ? true : false : false,
                        problem_id = s.PRB_Id,
                    }).FirstOrDefault();

                //Select Response Name
                List<SelectListItem> selectName = db.UserDetails
                    .Select(s => new SelectListItem
                    {
                        Value = s.USE_Usercode.ToString(),
                        Text = String.Concat(s.USE_Usercode.ToString()
                        , " : ", s.USE_NickName, " ", s.USE_FName, " ", s.USE_LName, " : "
                        , (from dept in db.Departments where dept.Dep_Id == s.Dep_Id select dept.Dep_Name).FirstOrDefault())
                    }).ToList();
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s=> new SelectListItem{Value = s.Dep_Id.ToString(), Text = s.Dep_Name}).ToList();

                model.SelectResponseOwner = selectName;
                model.SelectDepartmet = selectDept;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /QRAP_DefineSolution/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /QRAP_DefineSolution/Edit
        [HttpPost]
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult Edit(SolutionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CM_TodoList pr = db.CM_TodoList.Where(x => x.TDO_Id == model.solution_id).FirstOrDefault();
                    pr.TDO_Description = model.description;
                    pr.TDO_Owner = model.response_owner;
                    pr.TDO_DefineBy = Session["NickName"] != null ? Session["NickName"].ToString() : null;
                    pr.TDO_PlanDate = model.date;
                    pr.TDO_NeedEvidence = model.need_evidence ? 1 : 0;
                    pr.Dep_Id = model.department_id;   
                    //Save Edit
                    db.Entry(pr).State = System.Data.Entity.EntityState.Modified;
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("id:", pr.PRB_Id
                        , "/Date:", pr.TDO_PlanDate.HasValue ? pr.TDO_PlanDate.Value.ToString("yyy-MM-dd") : ""
                        , "/Dept:", pr.Dep_Id
                        , "/Solution", pr.TDO_Description);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "edit",
                        Log_Type = "Define Solution",
                        Log_System = "QRAP",
                        Log_Detail = log_detail,
                        Log_Action_Id = pr.TDO_Id,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    //Save
                    db.SaveChanges();

                    //Sent Mail
                    SendEmail(model);

                    TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.description);
                    return RedirectToAction("DefineSolution", new { id = model.problem_id });
                }
                //Select Response Name
                List<SelectListItem> selectName = db.UserDetails
                    .Select(s => new SelectListItem
                    {
                        Value = s.USE_Usercode.ToString(),
                        Text = String.Concat(s.USE_Usercode.ToString()
                        , " : ", s.USE_NickName, " ", s.USE_FName, " ", s.USE_LName, " : "
                        , (from dept in db.Departments where dept.Dep_Id == s.Dep_Id select dept.Dep_Name).FirstOrDefault())
                    }).ToList();
                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectResponseOwner = selectName;
                model.SelectDepartmet = selectDept;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_DefineSolution/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //  /QRAP_DefineSolution/SendEmail/
        public ActionResult SendEmail(SolutionModel solution_model)
        {
            try
            {
                int empid = !String.IsNullOrEmpty(solution_model.response_owner) ? Convert.ToInt32(solution_model.response_owner) : 0;
                SendMailCenterModel model = new SendMailCenterModel();
                //Send mail 
                var tomail = db.UserDetails.Where(x => x.USE_Usercode == empid)
                    .Select(s => new { email = s.USE_Email, fname = s.USE_FName, lname = s.USE_LName }).FirstOrDefault();
                if (tomail == null)
                {
                    TempData["shortMessage"] = String.Format("Send email failed, email of {0} is null. ",empid);
                    return RedirectToAction("DefineSolution", new { id = solution_model.problem_id });
                }
                //Send mail CC
                int idgroup_cc = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_QRAP_CC").Select(s => s.USG_Id).FirstOrDefault();
                var userlist_cc = db.GroupMembers.Where(x => x.USG_Id == idgroup_cc).Select(x => x.USE_Id).ToList();
                var tomail_cc = db.UserDetails.Where(x => userlist_cc.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();
                //Problem model
                var pr_model = db.CM_Problem.Where(x => x.PRB_Id == solution_model.problem_id)
                    .Select(s => new ProblemModel
                    {
                        id = s.PRB_Id,
                        detail = s.PRB_Description,
                        problem_no = s.PRB_Number,
                        reason = s.PRB_Reason,
                        owner = s.PRB_Owner,
                        date = s.PRB_PlanDate,
                        department_id = s.Dep_Id,
                        department = (from dept in db.Departments
                                      where dept.Dep_Id == s.Dep_Id
                                      select dept.Dep_Name).FirstOrDefault(),
                    }).FirstOrDefault();

                string plan_date = solution_model.date.ToString("yyyy-MM-dd");
                string need_evidence = solution_model.need_evidence ? "Yes" : "No";
                var callbackUrl = Url.Action("Create", "QRAP_Answer", new {id= solution_model.solution_id }, protocol: Request.Url.Scheme);

                string html = "<strong style='font-size: 20px'> Dear K. "+tomail.fname+" "+tomail.lname +", </strong>"
                             + "<p style='font-size: 20px'>คุณได้รับมอบหมายภาระกิจเพื่อแก้ปัญหาที่เกิดขึ้น โดยมีรายละเอียดดังต่อไปนี้ </p>"
                             + "<table style='font-size: 20px'>"
                             + "<tr><td>ปัญหาหมายเลข : </td><td>"+pr_model.problem_no +"</td></tr>"
                             + "<tr><td>รายละเอียดปัญหา : </td><td>"+ pr_model.detail +"</td></tr>"
                             + "<tr><td>เกิดขึ้นที่แผนก : </td><td>"+ pr_model.department +"</td></tr>"
                             + "<tr><td>วิธีการแก้ปัญหา :</td><td>"+ solution_model.description +"</td></tr>"
                             + "<tr><td>วันครบกำหนด(ตามแผน) :</td><td>" + plan_date + "</td></tr>"
                             + "<tr><td>อัพโหลดหลักฐาน :</td><td>" + need_evidence + "</td></tr>"
                             + "</table>"
                             + "<p style='font-size: 20px'>สามารถตรวจสอบรายละเอียดปัญหาและวิธีการแก้ปัญหาเพิ่มเติมได้ที่ระบบ "
                             + "<a href=\"" + callbackUrl + "\"> QRAP </a></p><br/>"
                             + "<p><h3></h3></p><br/><br/>"
                             + "<p style='font-size: 18px'>Best Regards,<br/>"
                             + "QRAP System</p>";

                List<string> tomail_list = new List<string>();
                tomail_list.Add(tomail.email);

                model.To = tomail_list;
                model.Tocc = tomail_cc;
                model.Subject = String.Format("อีเมลอัตโนมัติจากระบบ QRAP สำหรับแก้ปัญหาหมายเลข: {0}",pr_model.problem_no);
                model.Body = html;

                SendMailCenterController.SendMail(model);

                TempData["shortMessage"] = String.Format("Successfully send mail, To {0} ", String.Join(", ", tomail));
                return RedirectToAction("DefineSolution", new { id = solution_model.problem_id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //POST: /QRAP_DefineSolution/Delete
        [CustomAuthorize(2)]//2 Define Solution
        public ActionResult Delete(FormCollection form, ProblemModel model)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var check = db.CM_TodoEvidence.Where(x => id_list.Contains(x.TDO_Id) && x.TDE_Status.Trim() != "0" ).Select(s=>s.TDE_Id).ToList();
                    if (check.Any())
                    {
                        TempData["errorMessage"] = String.Format("The solution has an answer, Cannot be deleted. ");
                        return RedirectToAction("DefineSolution", new { id = model.id });
                    }
                    var so_list = db.CM_TodoList.Where(x => id_list.Contains(x.TDO_Id)).ToList();
                    foreach (var so in so_list)
                    {
                        //Remove
                        db.CM_TodoList.Remove(so);
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("id:", so.TDO_Id
                            , "/Date:", so.TDO_PlanDate.HasValue ? so.TDO_PlanDate.Value.ToString("yyy-MM-dd"): ""
                            , "/Dept:", so.Dep_Id
                            , "/SolutionDetail", so.TDO_Description);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Define Solution",
                            Log_System = "QRAP",
                            Log_Detail = log_detail,
                            Log_Action_Id = so.TDO_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Save
                        db.SaveChanges();
                    }
                    //Change Problem Status
                    if (so_list.Any())
                    {
                        var solu_list = db.CM_TodoList.Where(x => x.PRB_Id == model.id).Select(s => s.TDO_Id).ToList();
                        var an_list = db.CM_TodoEvidence.Where(x => solu_list.Contains(x.TDO_Id)).Select(s => s.TDE_Status).ToList();
                        if (solu_list.Count() == an_list.Count())
                        {
                            if (an_list.All(a => a.Trim() == "2"))
                            {  //Save Change Status
                                var pr_model = db.CM_Problem.Where(x => x.PRB_Id == model.id).FirstOrDefault();
                                pr_model.PRB_Status = 2;
                                db.Entry(pr_model).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return RedirectToAction("DefineSolution", new { id = model.id });
                }
                return RedirectToAction("DefineSolution", new { id = model.id });
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /QRAP_DefineSolution/Delete {0}", ex.ToString());
                return View("Error");
            }
        }

        // POST: /QRAP_DefineSolution/GetProblemNo
        [HttpPost]
        public JsonResult GetProblemNo(string Prefix)
        {
            var problem = db.CM_Problem
                    .Where(x => x.PRB_Number.StartsWith(Prefix))
                    .Take(10)
                    .Select(s => new { label = s.PRB_Number, val = s.PRB_Number }).ToList();
            return Json(problem);
        }
        // POST: /QRAP_DefineSolution/GetNickName
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