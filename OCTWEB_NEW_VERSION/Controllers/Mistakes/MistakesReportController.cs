using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Mistakes
{
    [Authorize]
    public class MistakesReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_pic = ConfigurationManager.AppSettings["path_pic"];

        //
        // GET: /MistakesReport/Emplist
        [CustomAuthorize(55)]//Mistakes report
        public ActionResult Emplist(SearchMistModel model)
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

                int? emp_id = model.emp_id;
                string nickname = model.nickname;
                string fname = model.fname;
                string lname = model.lname;
                string deptcode = model.deptcode;
                string position = model.position;
                string costcenter_code = model.costcenter_code;
                string reason_str = model.reason_str;
                int? levelwarning_id = model.levelwarning_id;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;

                //Show by department login
                string user_dep_id = null;
                if (Session["UserCode"] != null)
                {
                    int usercode = Convert.ToInt32(Session["UserCode"].ToString());
                    user_dep_id = db.EmpLists.Where(x => x.EmpID == usercode).Select(s => s.DeptCode).FirstOrDefault();
                    model.deptcode = user_dep_id;
                    deptcode = user_dep_id.ToString();
                }

                IEnumerable<Emp_offense> query_mist = model.Page.HasValue ? db.Emp_offense : db.Emp_offense.Take(0);
                if (start_date.HasValue)
                {
                    query_mist = query_mist.Where(x => x.date_Stmistake >= start_date);
                }
                if (end_date.HasValue)
                {
                    query_mist = query_mist.Where(x => x.date_Stmistake <= end_date);
                }
                if (!String.IsNullOrEmpty(reason_str))
                {
                    query_mist = query_mist.Where(x => !String.IsNullOrEmpty(x.Warning_Reason)
                        && x.Warning_Reason.ToLowerInvariant().Contains(reason_str.ToLowerInvariant()));
                }
                if (levelwarning_id > 0)
                {
                    query_mist = query_mist.Where(x => x.Level_warning_Id == levelwarning_id);
                }
                //Set EmpMistakesModel
                List<EmpMistakesModel> emp_mist_list = query_mist.Join(db.EmpLists, em => em.EmpId, e => e.EmpID, (em, e) => new { query_mist = em, EmpLists = e })
                   .Select(s => new EmpMistakesModel
                   {
                       emp_id = s.query_mist.EmpId,
                       reason = s.query_mist.Warning_Reason,
                       levelwarning_id = s.query_mist.Level_warning_Id,
                       levelwarning = (from lw in db.Emp_offense_Levelwarning
                                       where lw.Id == s.query_mist.Level_warning_Id
                                       select lw.level_warning).FirstOrDefault(),
                       shift = s.query_mist.day_shift,
                       mistake_date = s.query_mist.date_Stmistake,
                       expire_date = s.query_mist.Exprie_mt_date,
                       stopwork_start_date = s.query_mist.Stop_wkStart,
                       stopwork_end_date = s.query_mist.Stop_wkEnd,
                       create_date = s.query_mist.date_timenow,
                       detail_reason = s.query_mist.Detail_Reason,
                       layoff_date = s.query_mist.dateLayoff_emp,
                       nickname = s.EmpLists.NName_EN,
                       fname = s.EmpLists.FName_EN,
                       lname = s.EmpLists.LName_EN,
                       department = s.EmpLists.DeptDesc,
                       department_code = s.EmpLists.DeptCode,
                       position = s.EmpLists.Position,
                       costcenter = s.EmpLists.CostCenterName,
                       costcenter_code = s.EmpLists.CostCenter,
                   }).ToList();

                if (emp_id > 0)
                {
                    emp_mist_list = emp_mist_list.Where(x => x.emp_id == emp_id).ToList();
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.nickname)
                       && x.nickname.ToLowerInvariant().Contains(nickname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.fname)
                        && x.fname.ToLowerInvariant().Contains(fname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.lname)
                        && x.lname.ToLowerInvariant().Contains(lname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(deptcode))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.department_code == deptcode).ToList();
                }
                if (!String.IsNullOrEmpty(position))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.position == position).ToList();
                }
                if (!String.IsNullOrEmpty(costcenter_code))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.costcenter_code == costcenter_code).ToList();
                }
                emp_mist_list = emp_mist_list.OrderBy(o => o.emp_id).ToList();

                List<EmpMistakesModel> emp_mist_list_distinct = emp_mist_list.GroupBy(g => new
                {
                    emp_id = g.emp_id,
                    nickname = g.nickname,
                    fname = g.fname,
                    lname = g.lname,
                    department = g.department,
                    position = g.position,
                    costcenter = g.costcenter,
                })
                   .Select(s => new EmpMistakesModel
                   {
                       emp_id = s.Key.emp_id,
                       nickname = s.Key.nickname,
                       fname = s.Key.fname,
                       lname = s.Key.lname,
                       department = s.Key.department,
                       position = s.Key.position,
                       costcenter = s.Key.costcenter,
                   }).ToList();

                //Add Employee Picture
                foreach (var i in emp_mist_list_distinct)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, i.emp_id.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        i.emp_profile = imgDataURL;
                    }
                }
                IPagedList<EmpMistakesModel> emp_mist_pagedList = emp_mist_list_distinct.ToPagedList(pageIndex, pageSize);
                //Select Department
                List<SelectListItem> selectDeptCode = db.emp_department.OrderBy(o => o.DeptCode)
                    .Select(s => new SelectListItem
                    {
                        Value = s.DeptCode,
                        Text = s.DeptName,
                    }).ToList();
                //Select Position
                List<SelectListItem> selectPosition = db.emp_position.OrderBy(o => o.PositionName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.PositionName,
                        Text = s.PositionName,
                    }).ToList();
                //Select Cost Center
                List<SelectListItem> selectCostCenter = db.Emp_CostCenter
                    .Select(s => new SelectListItem
                    {
                        Value = s.CostCenter,
                        Text = s.CostCenterName,
                    }).ToList();
                //Select Reason
                List<SelectListItem> selectReason = db.Mstk_Reason
                    .Select(s => new SelectListItem
                    {
                        Value = s.Mstk_Reas_EN,
                        Text = s.Mstk_Reas_EN
                    }).ToList();
                //Select Level Warning
                List<SelectListItem> selectLevelWarning = db.Emp_offense_Levelwarning
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.level_warning
                    }).ToList();

                model.EmpMistakesPagedList = emp_mist_pagedList;
                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.total = emp_mist_list_distinct.Count();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /MistakesReport/Emplist ", ex.ToString());
                return View("Error");
            }

        }

        //
        // POST: /MistakesReport/Emplist
        [HttpPost]
        [CustomAuthorize(55)]//Mistakes report
        public ActionResult Emplist(FormCollection form, SearchMistModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? emp_id = model.emp_id;
                string nickname = model.nickname;
                string fname = model.fname;
                string lname = model.lname;
                string deptcode = model.deptcode;
                string position = model.position;
                string costcenter_code = model.costcenter_code;
                string reason_str = model.reason_str;
                int? levelwarning_id = model.levelwarning_id;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                //Show by department login
                string user_dep_id = null;
                if (Session["UserCode"] != null)
                {
                    int usercode = Convert.ToInt32(Session["UserCode"].ToString());
                    user_dep_id = db.EmpLists.Where(x => x.EmpID == usercode).Select(s => s.DeptCode).FirstOrDefault();
                    model.deptcode = user_dep_id;
                    deptcode = user_dep_id.ToString();
                }

                IEnumerable<Emp_offense> query_mist = db.Emp_offense;
                if (start_date.HasValue)
                {
                    query_mist = query_mist.Where(x => x.date_Stmistake >= start_date);
                }
                if (end_date.HasValue)
                {
                    query_mist = query_mist.Where(x => x.date_Stmistake <= end_date);
                }
                if (!String.IsNullOrEmpty(reason_str))
                {
                    query_mist = query_mist.Where(x => !String.IsNullOrEmpty(x.Warning_Reason)
                        && x.Warning_Reason.ToLowerInvariant().Contains(reason_str.ToLowerInvariant()));
                }
                if (levelwarning_id > 0)
                {
                    query_mist = query_mist.Where(x => x.Level_warning_Id == levelwarning_id);
                }

                //Set EmpMistakesModel
                List<EmpMistakesModel> emp_mist_list = query_mist.Join(db.EmpLists, em => em.EmpId, e => e.EmpID, (em, e) => new { query_mist = em, EmpLists = e })
                    .Select(s => new EmpMistakesModel
                    {
                        emp_id = s.query_mist.EmpId,
                        reason = s.query_mist.Warning_Reason,
                        levelwarning_id = s.query_mist.Level_warning_Id,
                        levelwarning = (from lw in db.Emp_offense_Levelwarning
                                        where lw.Id == s.query_mist.Level_warning_Id
                                        select lw.level_warning).FirstOrDefault(),
                        shift = s.query_mist.day_shift,
                        mistake_date = s.query_mist.date_Stmistake,
                        expire_date = s.query_mist.Exprie_mt_date,
                        stopwork_start_date = s.query_mist.Stop_wkStart,
                        stopwork_end_date = s.query_mist.Stop_wkEnd,
                        create_date = s.query_mist.date_timenow,
                        layoff_date = s.query_mist.dateLayoff_emp,
                        detail_reason = s.query_mist.Detail_Reason,
                        nickname = s.EmpLists.NName_EN,
                        fname = s.EmpLists.FName_EN,
                        lname = s.EmpLists.LName_EN,
                        department = s.EmpLists.DeptDesc,
                        department_code = s.EmpLists.DeptCode,
                        position = s.EmpLists.Position,
                        costcenter = s.EmpLists.CostCenterName,
                        costcenter_code = s.EmpLists.CostCenter,
                    }).ToList();

                if (emp_id > 0)
                {
                    emp_mist_list = emp_mist_list.Where(x => x.emp_id == emp_id).ToList();
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.nickname)
                       && x.nickname.ToLowerInvariant().Contains(nickname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.fname)
                        && x.fname.ToLowerInvariant().Contains(fname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    emp_mist_list = emp_mist_list.Where(x => !String.IsNullOrEmpty(x.lname)
                        && x.lname.ToLowerInvariant().Contains(lname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(deptcode))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.department_code == deptcode).ToList();
                }
                if (!String.IsNullOrEmpty(position))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.position == position).ToList();
                }
                if (!String.IsNullOrEmpty(costcenter_code))
                {
                    emp_mist_list = emp_mist_list.Where(x => x.costcenter_code == costcenter_code).ToList();
                }
                emp_mist_list = emp_mist_list.OrderBy(o => o.emp_id).ToList();

                //Export Csv
                var exp = form["ExportToCsv"];
                if (exp == "ExportToCsv")
                {
                    ExportToCsv(emp_mist_list);
                }

                List<EmpMistakesModel> emp_mist_list_distinct = emp_mist_list.GroupBy(g => new
                {
                    emp_id = g.emp_id,
                    nickname = g.nickname,
                    fname = g.fname,
                    lname = g.lname,
                    department = g.department,
                    position = g.position,
                    costcenter = g.costcenter,
                })
                    .Select(s => new EmpMistakesModel
                    {
                        emp_id = s.Key.emp_id,
                        nickname = s.Key.nickname,
                        fname = s.Key.fname,
                        lname = s.Key.lname,
                        department = s.Key.department,
                        position = s.Key.position,
                        costcenter = s.Key.costcenter,
                    }).ToList();

                //Add Employee Picture
                foreach (var i in emp_mist_list_distinct)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, i.emp_id.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        i.emp_profile = imgDataURL;
                    }
                }

                IPagedList<EmpMistakesModel> emp_mist_pagedList = emp_mist_list_distinct.ToPagedList(pageIndex, pageSize);
                //Select Department
                List<SelectListItem> selectDeptCode = db.emp_department.OrderBy(o => o.DeptCode)
                    .Select(s => new SelectListItem
                    {
                        Value = s.DeptCode,
                        Text = s.DeptName,
                    }).ToList();
                //Select Position
                List<SelectListItem> selectPosition = db.emp_position.OrderBy(o => o.PositionName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.PositionName,
                        Text = s.PositionName,
                    }).ToList();
                //Select Cost Center
                List<SelectListItem> selectCostCenter = db.Emp_CostCenter
                    .Select(s => new SelectListItem
                    {
                        Value = s.CostCenter,
                        Text = s.CostCenterName,
                    }).ToList();
                //Select Reason
                List<SelectListItem> selectReason = db.Mstk_Reason
                    .Select(s => new SelectListItem
                    {
                        Value = s.Mstk_Reas_EN,
                        Text = s.Mstk_Reas_EN
                    }).ToList();
                //Select Level Warning
                List<SelectListItem> selectLevelWarning = db.Emp_offense_Levelwarning
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.level_warning
                    }).ToList();

                model.EmpMistakesPagedList = emp_mist_pagedList;
                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.total = emp_mist_list_distinct.Count();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /MistakesReport/Emplist ", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /MistakesReport/Detail
        [CustomAuthorize(55)]//Mistakes report
        public ActionResult Detail(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                SearchMistModel model = new SearchMistModel();

                var emp_model = db.EmpLists.Where(x => x.EmpID == id).FirstOrDefault();

                if (emp_model != null)
                {
                    model.emp_id = emp_model.EmpID;
                    model.fname = emp_model.FName_EN;
                    model.lname = emp_model.LName_EN;
                    model.nickname = emp_model.NName_EN;
                    model.department = emp_model.DeptDesc;
                    model.position = emp_model.Position;
                    model.costcenter = emp_model.CostCenterName;

                    //Get image path
                    string imgPath = String.Concat(path_pic, emp_model.EmpID.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        model.emp_profile = imgDataURL;
                    }
                }

                List<EmpMistakesModel> mist_list = db.Emp_offense.Where(x => x.EmpId == emp_model.EmpID)
                        .Select(s => new EmpMistakesModel
                        {
                            levelwarning = (from lw in db.Emp_offense_Levelwarning
                                            where lw.Id == s.Level_warning_Id
                                            select lw.level_warning).FirstOrDefault(),
                            levelwarning_id = s.Level_warning_Id,
                            reason = s.Warning_Reason,
                            detail_reason = s.Detail_Reason,
                            shift = s.day_shift,
                            mistake_date = s.date_Stmistake,
                            expire_date = s.Exprie_mt_date,
                            stopwork_start_date = s.Stop_wkStart,
                            stopwork_end_date = s.Stop_wkEnd,
                            amount_stopwork_date = s.Date_mistake,
                            mistake_id = s.Id,
                        }).ToList();
                //Set days expire
                foreach (var i in mist_list)
                {
                    DateTime datenow = DateTime.Today;
                    DateTime expire_date = i.expire_date.HasValue ? i.expire_date.Value : DateTime.Today;
                    TimeSpan diff = expire_date - datenow;
                    int days = diff.Days;
                    i.amount_expire_date = days > 0 ? days : 0;
                }

                List<string> reason_list = mist_list.Select(s => s.reason).ToList();
                List<int> levelwarning_id_list = mist_list.Select(s => s.levelwarning_id).ToList();
                /*
                    74	ตักเตือนสติ (สีขาว) - Exhort (White card)
                    75	ตักเตือนวาจา (สีฟ้า) - Exhort (Blue card)
                    76	ตักเตือนลายลักษณ์อักษร (สีเหลือง) - exhort written form (Yellow card) 	
                    77	พักงาน - You be suspended from job 
                    78	เลิกจ้าง - lay off employee
                */
                model.count_card_white = levelwarning_id_list.Where(x => x == 74).Count();
                model.count_card_blue = levelwarning_id_list.Where(x => x == 75).Count();
                model.count_card_yellow = levelwarning_id_list.Where(x => x == 76).Count();
                model.count_card_orange = levelwarning_id_list.Where(x => x == 77).Count();
                model.count_card_red = levelwarning_id_list.Where(x => x == 78).Count();
                model.count_total = levelwarning_id_list.Count();

                //Select Reason
                IEnumerable<Mstk_Reason> query_reason = reason_list.Any() ? db.Mstk_Reason.Where(x => reason_list.Contains(x.Mstk_Reas_EN)) : db.Mstk_Reason;
                List<SelectListItem> selectReason = query_reason
                    .Select(s => new SelectListItem
                    {
                        Value = s.Mstk_Reas_EN,
                        Text = s.Mstk_Reas_EN
                    }).ToList();
                //Select Level Warning
                IEnumerable<Emp_offense_Levelwarning> query_levelwarning = levelwarning_id_list.Any() ?
                    db.Emp_offense_Levelwarning.Where(x => levelwarning_id_list.Contains(x.Id)) : db.Emp_offense_Levelwarning;
                List<SelectListItem> selectLevelWarning = query_levelwarning
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.level_warning
                    }).ToList();

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.EmpMistakesList = mist_list;


                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        // POST: /MistakesReport/Detail
        [HttpPost]
        [CustomAuthorize(55)]//Mistakes report
        public ActionResult Detail(SearchMistModel model)
        {
            try
            {
                int? levelwarning_id = model.levelwarning_id;
                string reason = model.reason_str;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;

                IEnumerable<Emp_offense> query_offense = db.Emp_offense.Where(x => x.EmpId == model.emp_id);
                if (levelwarning_id.HasValue)
                {
                    query_offense = query_offense.Where(x => x.Level_warning_Id == levelwarning_id);
                }
                if (!String.IsNullOrEmpty(reason))
                {
                    query_offense = query_offense.Where(x => x.Warning_Reason == reason);
                }
                if (start_date.HasValue)
                {
                    query_offense = query_offense.Where(x => x.date_Stmistake >= start_date);
                }
                if (end_date.HasValue)
                {
                    query_offense = query_offense.Where(x => x.date_Stmistake <= end_date);
                }
                List<EmpMistakesModel> mist_list = query_offense
                        .Select(s => new EmpMistakesModel
                        {
                            levelwarning = (from lw in db.Emp_offense_Levelwarning
                                            where lw.Id == s.Level_warning_Id
                                            select lw.level_warning).FirstOrDefault(),
                            levelwarning_id = s.Level_warning_Id,
                            reason = s.Warning_Reason,
                            detail_reason = s.Detail_Reason,
                            shift = s.day_shift,
                            mistake_date = s.date_Stmistake,
                            expire_date = s.Exprie_mt_date,
                            stopwork_start_date = s.Stop_wkStart,
                            stopwork_end_date = s.Stop_wkEnd,
                            amount_stopwork_date = s.Date_mistake,
                            mistake_id = s.Id,
                        }).ToList();
                //Set days expire
                foreach (var i in mist_list)
                {
                    DateTime datenow = DateTime.Today;
                    DateTime expire_date = i.expire_date.HasValue ? i.expire_date.Value : DateTime.Today;
                    TimeSpan diff = expire_date - datenow;
                    int days = diff.Days;
                    i.amount_expire_date = days > 0 ? days : 0;
                }

                List<Emp_offense> emp_offense = db.Emp_offense.Where(x => x.EmpId == model.emp_id).ToList();
                List<string> reason_list = emp_offense.Select(s => s.Warning_Reason).ToList();
                List<int> levelwarning_id_list = emp_offense.Select(s => s.Level_warning_Id).ToList();

                //Select Reason
                IEnumerable<Mstk_Reason> query_reason = reason_list.Any() ? db.Mstk_Reason.Where(x => reason_list.Contains(x.Mstk_Reas_EN)) : db.Mstk_Reason;
                List<SelectListItem> selectReason = query_reason
                    .Select(s => new SelectListItem
                    {
                        Value = s.Mstk_Reas_EN,
                        Text = s.Mstk_Reas_EN
                    }).ToList();
                //Select Level Warning
                IEnumerable<Emp_offense_Levelwarning> query_levelwarning = levelwarning_id_list.Any() ?
                    db.Emp_offense_Levelwarning.Where(x => levelwarning_id_list.Contains(x.Id)) : db.Emp_offense_Levelwarning;
                List<SelectListItem> selectLevelWarning = query_levelwarning
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.level_warning
                    }).ToList();

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.EmpMistakesList = mist_list;


                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }


        //MistakesReport/ExportToCsv      
        public void ExportToCsv(List<EmpMistakesModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    emp_id = v.emp_id,
                    fname = v.fname,
                    lname = v.lname,
                    nickname = v.nickname,
                    department = "\"" + v.department + "\"",
                    department_code = v.department_code,
                    position = "\"" + v.position + "\"",
                    costcenter = "\"" + v.costcenter + "\"",
                    costcenter_code = v.costcenter_code,
                    reason = "\"" + v.reason + "\"",
                    levelwarning_id = v.levelwarning_id,
                    levelwarning = v.levelwarning,
                    shift = v.shift,
                    mistake_date = v.mistake_date,
                    expire_date = v.expire_date,
                    stopwork_start_date = v.stopwork_start_date,
                    stopwork_end_date = v.stopwork_end_date,
                    create_date = v.create_date,
                    layoff_date = v.layoff_date,
                    detail_reason = "\"" + v.detail_reason + "\"",

                });

                sb.AppendFormat(
                    "{0},{1},{2},{3},{4}"
                    + ",{5},{6},{7},{8},{9}"
                    + ",{10},{11},{12},{13},{14}"
                    + ",{15},{16},{17},{18}"
                    , "No.", "Emp ID", "First Name EN", "Last Name EN", "Nick Name EN"
                    , "Department", "Position", "Cost Center Name", "Reason Warning", "Reason Warning Detail"
                    , "Level Warning", "Mistake Date", "Expire", "Stop Working [Start Date]", "Stop Working [End Date]"
                    , "Layoff Date", "Shift", "Create Date"
                    , Environment.NewLine);

                string stopwork_s_date_str = null;
                string stopwork_e_date_str = null;
                string mistake_date_str = null;
                string expire_date_str = null;
                string layoff_date_str = null;
                string create_date_str = null;

                foreach (var i in forexport)
                {
                    stopwork_s_date_str = i.stopwork_start_date.HasValue ? i.stopwork_start_date.Value.ToString("yyyy-MM-dd") : null;
                    stopwork_e_date_str = i.stopwork_end_date.HasValue ? i.stopwork_end_date.Value.ToString("yyyy-MM-dd") : null;
                    mistake_date_str = i.mistake_date.HasValue ? i.mistake_date.Value.ToString("yyyy-MM-dd") : null;
                    expire_date_str = i.expire_date.HasValue ? i.expire_date.Value.ToString("yyyy-MM-dd") : null;
                    layoff_date_str = i.layoff_date.HasValue ? i.layoff_date.Value.ToString("yyyy-MM-dd") : null;
                    create_date_str = i.create_date.HasValue ? i.create_date.Value.ToString("yyyy-MM-dd") : null;

                    sb.AppendFormat(
                       "{0},{1},{2},{3},{4}"
                       + ",{5},{6},{7},{8},{9}"
                       + ",{10},{11},{12},{13},{14}"
                       + ",{15},{16},{17},{18}"
                       , i.item, i.emp_id, i.fname, i.lname, i.nickname
                       , i.department, i.position, i.costcenter, i.reason, i.detail_reason
                       , i.levelwarning, mistake_date_str, expire_date_str, stopwork_s_date_str, stopwork_e_date_str
                       , layoff_date_str, i.shift, create_date_str
                       , Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=Employee_Mistake_Report.CSV ");
                response.ContentType = "text/plain";
                response.Write(sb.ToString());
                response.End();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        //
        // POST: /MistakesReport/GetEmpId/
        [HttpPost]
        public JsonResult GetEmpId(int Prefix)
        {
            var empid = (from el in db.EmpLists
                         where el.EmpID.ToString().StartsWith(Prefix.ToString())
                         select new { label = el.EmpID, val = el.EmpID }).Take(10).ToList();
            return Json(empid);
        }
        //
        // POST: /MistakesReport/GetFName/
        [HttpPost]
        public JsonResult GetFName(string Prefix)
        {
            var fname = (from el in db.EmpLists
                         where el.FName_EN.StartsWith(Prefix)
                         select new { label = el.FName_EN, val = el.FName_EN }).Take(10).ToList();
            return Json(fname);
        }
        //
        // POST: /MistakesReport/GetLName/
        [HttpPost]
        public JsonResult GetLName(string Prefix)
        {
            var lname = (from el in db.EmpLists
                         where el.LName_EN.StartsWith(Prefix)
                         select new { label = el.LName_EN, val = el.LName_EN }).Take(10).ToList();
            return Json(lname);
        }
        //
        // POST: /MistakesReport/GetNName/
        [HttpPost]
        public JsonResult GetNName(string Prefix)
        {
            var nname = (from el in db.EmpLists
                         where el.NName_EN.StartsWith(Prefix)
                         select new { label = el.NName_EN, val = el.NName_EN }).Take(10).ToList();
            return Json(nname);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

	}
}