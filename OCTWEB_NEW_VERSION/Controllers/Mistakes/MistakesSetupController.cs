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
    public class MistakesSetupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_pic = ConfigurationManager.AppSettings["path_pic"];

        //
        //GET : /MistakesSetup/Formerlist
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Formerlist(SearchMistModel model)
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
                List<EmpMistakesModel> emp_mist_list = query_mist.Join(db.Former_EmpList, em => em.EmpId, e => e.EmpID, (em, e) => new { query_mist = em, EmpLists = e })
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

                model.FormerEmpMistakesPagedList = emp_mist_pagedList;
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
                ViewBag.errorMessage = String.Format("Error: Get /MistakesSetup/Emplist ", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: /MistakesSetup/Formerlist
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Formerlist(FormCollection form, SearchMistModel model)
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
                List<EmpMistakesModel> emp_mist_list = query_mist.Join(db.Former_EmpList, em => em.EmpId, e => e.EmpID, (em, e) => new { query_mist = em, EmpLists = e })
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

                model.FormerEmpMistakesPagedList = emp_mist_pagedList;
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
                ViewBag.errorMessage = String.Format("Error: Post /MistakesSetup/Mistakeslist ", ex.ToString());
                return View("Error");
            }
        }

        //
        //Get : Mistakes
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Formerdetail(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                SearchMistModel model = new SearchMistModel();

                var fmp_model = db.Former_EmpList.Where(x => x.EmpID == id).FirstOrDefault();

                if (fmp_model != null)
                {
                    model.emp_id = fmp_model.EmpID;
                    model.fname = fmp_model.FName_EN;
                    model.lname = fmp_model.LName_EN;
                    model.nickname = fmp_model.NName_EN;
                    model.department = fmp_model.DeptDesc;
                    model.position = fmp_model.Position;
                    model.costcenter = fmp_model.CostCenterName;

                    //Get image path
                    string imgPath = String.Concat(path_pic, fmp_model.EmpID.ToString(), ".png");
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
                List<EmpMistakesModel> mist_list = db.Emp_offense.Where(x => x.EmpId == fmp_model.EmpID)
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
        // GET: /MistakesSetup/Emplist
        [CustomAuthorize(34)]//Mistakes setup
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
                string reason_str  = model.reason_str;
                int? levelwarning_id = model.levelwarning_id;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;

                IEnumerable<Emp_offense> query_mist = model.Page.HasValue ? db.Emp_offense : db.Emp_offense.Take(0);
                if(start_date.HasValue){
                    query_mist = query_mist.Where(x => x.date_Stmistake >= start_date);
                }
                if (end_date.HasValue)
                {
                    query_mist = query_mist.Where(x => x.date_Stmistake <= end_date);
                }
                if(!String.IsNullOrEmpty(reason_str))
                {
                    query_mist = query_mist.Where(x => !String.IsNullOrEmpty(x.Warning_Reason)
                        && x.Warning_Reason.ToLowerInvariant().Contains(reason_str.ToLowerInvariant()));
                }
                if(levelwarning_id > 0)
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
                        Text =  s.DeptName,
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
            catch(Exception ex){
                ViewBag.errorMessage = String.Format("Error: Get /MistakesSetup/Emplist ", ex.ToString());
                return View("Error");
            }
           
        }

        //
        // POST: /MistakesSetup/Emplist
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Emplist(FormCollection form,SearchMistModel model)
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
                List<EmpMistakesModel> emp_mist_list = query_mist.Join(db.EmpLists, em => em.EmpId, e => e.EmpID, (em, e) => new {query_mist=em,EmpLists = e })
                    .Select(s => new EmpMistakesModel
                    {
                        emp_id          = s.query_mist.EmpId,
                        reason          = s.query_mist.Warning_Reason,
                        levelwarning_id = s.query_mist.Level_warning_Id,
                        levelwarning    = (from lw in db.Emp_offense_Levelwarning
                                            where lw.Id == s.query_mist.Level_warning_Id
                                            select lw.level_warning).FirstOrDefault(),
                        shift           = s.query_mist.day_shift,
                        mistake_date    = s.query_mist.date_Stmistake,
                        expire_date     = s.query_mist.Exprie_mt_date,
                        stopwork_start_date = s.query_mist.Stop_wkStart,
                        stopwork_end_date   = s.query_mist.Stop_wkEnd,
                        create_date         = s.query_mist.date_timenow,
                        layoff_date         = s.query_mist.dateLayoff_emp,
                        detail_reason       = s.query_mist.Detail_Reason,
                        nickname            = s.EmpLists.NName_EN,
                        fname               = s.EmpLists.FName_EN,
                        lname               = s.EmpLists.LName_EN,
                        department          = s.EmpLists.DeptDesc,
                        department_code     = s.EmpLists.DeptCode,
                        position            = s.EmpLists.Position,
                        costcenter          = s.EmpLists.CostCenterName,
                        costcenter_code     = s.EmpLists.CostCenter,
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
                ViewBag.errorMessage = String.Format("Error: Post /MistakesSetup/Mistakeslist ", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /MistakesSetup/Setting
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Setting()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
               
                MistaksSettingModel model = new MistaksSettingModel();
                //Select Level warning
                List<SelectListItem> selectLevelwarning = db.Emp_offense_Levelwarning
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.level_warning }).ToList();
                //Select Reason
                List<SelectListItem> selectReason = db.Mstk_Reason
                    .Select(s => new SelectListItem { Value = s.ReasonID.ToString(), Text = s.Mstk_Reas_EN }).ToList();

                model.SelectLevelwarning = selectLevelwarning;
                model.SelectReason = selectReason;
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: GET /MistakesSetup/Setting ", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /MistakesSetup/Setting
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Setting(MistaksSettingModel model)
        {
            try
            {
                //Select Level warning
                List<SelectListItem> selectLevelwarning = db.Emp_offense_Levelwarning
                  .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.level_warning }).ToList();
                //Select Reason
                List<SelectListItem> selectReason = db.Mstk_Reason
                    .Select(s => new SelectListItem { Value = s.ReasonID.ToString(), Text = s.Mstk_Reas_EN }).ToList();
                model.SelectLevelwarning = selectLevelwarning;
                model.SelectReason = selectReason;

                string check = model.check;
                //Add Level Warning
                if (check == "level_add_check")
                {
                    if(!String.IsNullOrEmpty(model.levelwarning))
                    {
                        if(ModelState.IsValid)
                        {
                            Emp_offense_Levelwarning lw = new Emp_offense_Levelwarning();
                            lw.level_warning = model.levelwarning;
                            db.Emp_offense_Levelwarning.Add(lw);
                            db.SaveChanges();
                            TempData["shortMessage"] = String.Format("Created successfully, {0} . ", model.levelwarning);
                            return RedirectToAction("Setting");
                        }                     
                    }
                    else
                    {
                        ViewBag.LevelMessage = "Level of warning field is required.";
                        return View(model);
                    }
                }
                // Delete Level Warning
                if (check == "level_delete_check")
                {
                    List<int> level_list = model.levelwarning_id;

                    if (level_list.Any())
                    {
                        HashSet<int> notDeletable = new HashSet<int> { 74, 75, 76 };

                        List<int> deletableLevels = db.Emp_offense_Levelwarning
                            .Where(x => level_list.Contains(x.Id) && !notDeletable.Contains(x.Id))
                            .Select(x => x.Id)
                            .ToList(); // 

                        HashSet<int> inUseLevels = new HashSet<int>(db.Emp_offense
                            .Where(e => deletableLevels.Contains(e.Level_warning_Id))
                            .Select(e => e.Level_warning_Id)
                            .ToList()); //

                        deletableLevels.RemoveAll(id => inUseLevels.Contains(id));

                        if (!deletableLevels.Any())
                        {
                            ViewBag.ReasonMessage = "Cannot delete level warnings because they are in use or restricted.";
                            return View(model);
                        }

                        List<Emp_offense_Levelwarning> lw_list = db.Emp_offense_Levelwarning
                            .Where(x => deletableLevels.Contains(x.Id))
                            .ToList();

                        if (lw_list.Any())
                        {
                            db.Emp_offense_Levelwarning.RemoveRange(lw_list);
                            db.SaveChanges();
                            TempData["shortMessage"] = "Deleted successfully.";
                            return RedirectToAction("Setting");
                        }
                    }
                }


                //Add Reason 
                if (check == "reason_add_check")
                {
                    if (!String.IsNullOrEmpty(model.reason))
                    { 
                        if(ModelState.IsValid)
                        {
                            Mstk_Reason rs = new Mstk_Reason();
                            rs.Mstk_Reas_EN = model.reason;
                            db.Mstk_Reason.Add(rs);
                            db.SaveChanges();
                            TempData["shortMessage"] = String.Format("Created successfully, {0} ", model.reason);
                            return RedirectToAction("Setting");
                        } 
                    }
                    else
                    {
                        ViewBag.ReasonMessage = "Type for mistake field is required.";
                        return View(model);
                    }
                }
                //Delete Reason
                if (check == "reason_delete_check")
                {
                    List<int> reason_list = model.reason_id;

                    if (reason_list.Any())
                    {
                        // ดึงค่า Reason ที่ต้องการลบ
                        List<string> deleteReasonNames = db.Mstk_Reason
                            .Where(x => reason_list.Contains(x.ReasonID))
                            .Select(x => x.Mstk_Reas_EN)
                            .ToList();

                        // เช็คว่าค่า Reason ถูกใช้งานอยู่หรือไม่ใน Warning_Reason
                        List<string> inUseReasons = db.Emp_offense
                            .Where(e => deleteReasonNames.Contains(e.Warning_Reason))
                            .Select(e => e.Warning_Reason)
                            .Distinct()
                            .ToList();

                        // ลบรายการ Reason ที่ถูกใช้งานออกจาก List
                        deleteReasonNames.RemoveAll(r => inUseReasons.Contains(r));

                        if (deleteReasonNames.Count == 0)
                        {
                            ViewBag.LevelMessage = "Cannot delete reason because they are in use.";
                            return View(model);
                        }

                        // ดึงรายการ Reason ที่สามารถลบได้
                        List<Mstk_Reason> rs_list = db.Mstk_Reason
                            .Where(x => deleteReasonNames.Contains(x.Mstk_Reas_EN))
                            .ToList();

                        if (rs_list.Any())
                        {
                            db.Mstk_Reason.RemoveRange(rs_list);
                            db.SaveChanges();
                            TempData["shortMessage"] = "Deleted successfully";
                            return RedirectToAction("Setting");
                        }
                    }
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /MistakesSetup/Setting ", ex.ToString());
                return View("Error");
            }
        }
 
        //
        // GET: /MistakesSetup/Create
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Create()
        {
            try
            {
                EmpMistakesModel model = new EmpMistakesModel();
                //Get default image path
                string imgPath = String.Concat(path_pic, "NoImg.png");
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;

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
        // POST: /MistakesSetup/Create
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Create(EmpMistakesModel model)
        {
            try 
            {
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;

                if(ModelState.IsValid)
                {
                    int emp_id = db.EmpLists.Where(x => x.EmpID == model.emp_id.Value).Select(s=>s.EmpID).FirstOrDefault();
                    if(emp_id > 0)
                    {
                        DateTime mistake_date = model.mistake_date.Value;
                        DateTime date_now = DateTime.Now;
                        Emp_offense offense_model = new Emp_offense();
                        offense_model.EmpId = emp_id;
                        offense_model.Warning_Reason = model.reason;
                        offense_model.Level_warning_Id = model.levelwarning_id;
                        offense_model.date_Stmistake = mistake_date;
                        offense_model.Exprie_mt_date = mistake_date.AddYears(1);
                        offense_model.day_shift = model.shift;
                        offense_model.Detail_Reason = model.detail_reason;
                        offense_model.date_timenow = date_now;
                        if(model.stopwork_start_date.HasValue && model.stopwork_end_date.HasValue)
                        {
                            var start_date = model.stopwork_start_date.Value;
                            var end_date = model.stopwork_end_date.Value;
                            TimeSpan diff = end_date.Subtract(start_date);

                            offense_model.Stop_wkStart = start_date;
                            offense_model.Stop_wkEnd = end_date;
                            offense_model.Date_mistake = (diff.Days + 1 ).ToString();

                        }
                        db.Emp_offense.Add(offense_model);
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Created successfully, Mistake of Emp Id {0} . ", emp_id);
                        return RedirectToAction("Emplist", new SearchMistModel {emp_id = emp_id,Page=1 });
                    }
                }

                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        // GET: /MistakesSetup/DetailCreate
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult DetailCreate(int id)
        {
            try
            {
                EmpMistakesModel model = new EmpMistakesModel();
                var emp = db.EmpLists.Where(x => x.EmpID == id).FirstOrDefault();
                if (emp != null)
                {
                    model.emp_id = emp.EmpID;
                    model.fname = emp.FName_EN;
                    model.lname = emp.LName_EN;
                    model.nickname = emp.NName_EN;
                    model.department = emp.DeptDesc;
                    model.position = emp.Position;
                    model.costcenter = emp.CostCenterName;
                    //Get image path
                    string imgPath = String.Concat(path_pic, emp.EmpID.ToString(), ".png");
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
                else
                {
                    //Get default image path
                    string imgPath = String.Concat(path_pic, "NoImg.png");
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;

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
        // POST: /MistakesSetup/DetailCreate
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult DetailCreate(EmpMistakesModel model)
        {
            try
            {
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;

                if (ModelState.IsValid)
                {
                    int emp_id = db.EmpLists.Where(x => x.EmpID == model.emp_id.Value).Select(s => s.EmpID).FirstOrDefault();
                    if (emp_id > 0)
                    {
                        DateTime mistake_date = model.mistake_date.Value;
                        DateTime date_now = DateTime.Now;
                        Emp_offense offense_model = new Emp_offense();
                        offense_model.EmpId = emp_id;
                        offense_model.Warning_Reason = model.reason;
                        offense_model.Level_warning_Id = model.levelwarning_id;
                        offense_model.date_Stmistake = mistake_date;
                        offense_model.Exprie_mt_date = mistake_date.AddYears(1);
                        offense_model.day_shift = model.shift;
                        offense_model.Detail_Reason = model.detail_reason;
                        offense_model.date_timenow = date_now;
                        if (model.stopwork_start_date.HasValue && model.stopwork_end_date.HasValue)
                        {
                            var start_date = model.stopwork_start_date.Value;
                            var end_date = model.stopwork_end_date.Value;
                            TimeSpan diff = end_date.Subtract(start_date);

                            offense_model.Stop_wkStart = start_date;
                            offense_model.Stop_wkEnd = end_date;
                            offense_model.Date_mistake = (diff.Days + 1).ToString();

                        }
                        var result = db.Emp_offense.Add(offense_model);
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Created successfully, {0} . ", result.Warning_Reason);
                        return RedirectToAction("Detail", new { id = emp_id });
                    }
                }
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
        // GET: /MistakesSetup/Detail
        [CustomAuthorize(34)]//Mistakes setup
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

                if(emp_model != null)
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
                IEnumerable<Mstk_Reason> query_reason = reason_list.Any() ? db.Mstk_Reason.Where(x=>reason_list.Contains(x.Mstk_Reas_EN)) : db.Mstk_Reason;
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
        // POST: /MistakesSetup/Detail
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
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
                if(!String.IsNullOrEmpty(reason)){
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

        //
        // GET: /MistakesSetup/Edit
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Edit(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                EmpMistakesModel model = new EmpMistakesModel();

                var mist = db.Emp_offense.Where(x => x.Id == id).FirstOrDefault();
                model.mistake_id = mist.Id;
                model.mistake_date = mist.date_Stmistake;
                model.shift = mist.day_shift;
                model.reason = mist.Warning_Reason;
                model.levelwarning_id = mist.Level_warning_Id;
                model.detail_reason = mist.Detail_Reason;
                model.expire_date_str = mist.Exprie_mt_date.HasValue ? mist.Exprie_mt_date.Value.ToString("dd MMM yyyy") : null;
                model.stopwork_start_date = mist.Stop_wkStart;
                model.stopwork_end_date = mist.Stop_wkEnd;
                model.amount_stopwork_date = mist.Date_mistake;
                //Cal amount expire date
                DateTime today = DateTime.Today;
                DateTime expire_date = mist.Exprie_mt_date.HasValue ? mist.Exprie_mt_date.Value: DateTime.Today;
                TimeSpan diff = expire_date - today;
                model.amount_mistake_date = diff.Days > 0 ? diff.Days.ToString() : "0";
                //Set employee detail
                var emp = db.EmpLists.Where(x => x.EmpID == mist.EmpId).FirstOrDefault();
                if (emp != null)
                {
                    model.emp_id = emp.EmpID;
                    model.fname = emp.FName_EN;
                    model.lname = emp.LName_EN;
                    model.nickname = emp.NName_EN;
                    model.department = emp.DeptDesc;
                    model.position = emp.Position;
                    model.costcenter = emp.CostCenterName;
                    //Get image path
                    string imgPath = String.Concat(path_pic, emp.EmpID.ToString(), ".png");
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;
           
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
        // POST: /MistakesSetup/Edit
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Edit(EmpMistakesModel model)
        {
            try
            {
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
                //Select chift
                List<SelectListItem> selectShift = new List<SelectListItem>();
                selectShift.Add(new SelectListItem { Value = "Day (กะเช้า)", Text = "Day (กะเช้า)" });
                selectShift.Add(new SelectListItem { Value = "Night (กะดึก)", Text = "Night (กะดึก)" });

                model.SelectReason = selectReason;
                model.SelectLevalWarning = selectLevelWarning;
                model.SelectShift = selectShift;

                if (ModelState.IsValid)
                {
                    Emp_offense offense_model = db.Emp_offense.Where(x => x.Id == model.mistake_id).FirstOrDefault();
                    if (offense_model != null)
                    {
                        DateTime mistake_date = model.mistake_date.Value;
                        DateTime date_now = DateTime.Now;
                        offense_model.EmpId = model.emp_id.HasValue ? model.emp_id.Value : 0;
                        offense_model.Warning_Reason = model.reason;
                        offense_model.Level_warning_Id = model.levelwarning_id;
                        offense_model.date_Stmistake = mistake_date;
                        offense_model.Exprie_mt_date = mistake_date.AddYears(1);
                        offense_model.day_shift = model.shift;
                        offense_model.Detail_Reason = model.detail_reason;
                        offense_model.date_timenow = date_now;
                        if (model.stopwork_start_date.HasValue && model.stopwork_end_date.HasValue)
                        {
                            var start_date = model.stopwork_start_date.Value;
                            var end_date = model.stopwork_end_date.Value;
                            TimeSpan diff = end_date.Subtract(start_date);

                            offense_model.Stop_wkStart = start_date;
                            offense_model.Stop_wkEnd = end_date;
                            offense_model.Date_mistake = (diff.Days + 1).ToString();

                        }
                        db.Entry(offense_model).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Edited successfully, {0} . ", model.reason);
                        return RedirectToAction("Detail", new { id = model.emp_id });
                    }
                }
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
        //MistakesSetup/Delete
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> emp_id_list = new List<int>();
                var selectedItem = form["selectedItem"];
              
                if (selectedItem != null)
                {
                    emp_id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var mistake_list = db.Emp_offense.Where(x => emp_id_list.Contains(x.EmpId)).ToList();
                    if (mistake_list.Any())
                    {
                        //Delete Mistakes
                        db.Emp_offense.RemoveRange(mistake_list);
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Mistakes Setup",
                            Log_System = "HR",
                            Log_Detail = String.Concat("Emp ID:"
                                        , selectedItem),
                            //Log_Action_Id = ,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Save
                        db.SaveChanges();
                        TempData["shortMessage"] = String.Format("Deleted successfully, Mistakes of Emp ID : {0} . ", selectedItem);
                    }
                }
                return RedirectToAction("Emplist");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //MistakesSetup/DetailDelete
        [HttpPost]
        [CustomAuthorize(34)]//Mistakes setup
        public ActionResult DetailDelete(FormCollection form)
        {
            try
            {

                List<int> mistake_id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                var emp_id_str = form["emp_id"];
                int emp_id = Convert.ToInt32(emp_id_str);
                if(selectedItem != null)
                {
                    mistake_id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var mistake_list = db.Emp_offense.Where(x => mistake_id_list.Contains(x.Id)).ToList();
                    if(mistake_list.Any())
                    {
                        //Delete Mistakes
                        db.Emp_offense.RemoveRange(mistake_list);
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Mistakes Setup",
                            Log_System = "HR",
                            Log_Detail = String.Concat("Mistakes ID:"
                                        , selectedItem),
                            //Log_Action_Id = ,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Save
                        db.SaveChanges();
                        TempData["shortMessage"] = String.Format("Deleted successfully, {0} Items . ", mistake_list.Count());
                    }
                }
                return RedirectToAction("Detail", new {id = emp_id });
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //MistakesSetup/ExportToCsv      
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
                     levelwarning_id  = v.levelwarning_id,
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
                response.AddHeader("content-disposition", "attachment;filename=Employee_Mistake.CSV ");
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
        // GET: /MistakesSetup/GetEmpDetail/
        public JsonResult GetEmpDetail(int emp_id)
        {

            var emp_detail = new Dictionary<string, string>();
            if(emp_id > 1000)
            {
                var emp = db.EmpLists.Where(x => x.EmpID == emp_id).FirstOrDefault();

                if (emp != null)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, emp.EmpID.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        emp_detail.Add("pic", imgDataURL);
                    }
                    else 
                    {
                        emp_detail.Add("pic","");
                    }
                    emp_detail.Add("empid", emp.EmpID.ToString());
                    emp_detail.Add("fname", emp.FName_EN);
                    emp_detail.Add("lname", emp.LName_EN);
                    emp_detail.Add("nickname", emp.NName_EN);
                    emp_detail.Add("department", emp.DeptDesc);
                    emp_detail.Add("position", emp.Position);
                    emp_detail.Add("costcenter", emp.CostCenterName);
                }

            }
           
            return Json(emp_detail, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /MistakesSetup/GetEmpId/
        [HttpPost]
        public JsonResult GetEmpId(int Prefix)
        {
            var empid = (from el in db.EmpLists
                         where el.EmpID.ToString().StartsWith(Prefix.ToString())
                         select new { label = el.EmpID, val = el.EmpID }).Take(10).ToList();
            return Json(empid);
        }
        //
        // POST: /MistakesSetup/GetFName/
        [HttpPost]
        public JsonResult GetFName(string Prefix)
        {
            var fname = (from el in db.EmpLists
                         where el.FName_EN.StartsWith(Prefix)
                         select new { label = el.FName_EN, val = el.FName_EN }).Take(10).ToList();
            return Json(fname);
        }
        //
        // POST: /MistakesSetup/GetLName/
        [HttpPost]
        public JsonResult GetLName(string Prefix)
        {
            var lname = (from el in db.EmpLists
                         where el.LName_EN.StartsWith(Prefix)
                         select new { label = el.LName_EN, val = el.LName_EN }).Take(10).ToList();
            return Json(lname);
        }
        //
        // POST: /MistakesSetup/GetNName/
        [HttpPost]
        public JsonResult GetNName(string Prefix)
        {
            var nname = (from el in db.EmpLists
                         where el.NName_EN.StartsWith(Prefix)
                         select new { label = el.NName_EN, val = el.NName_EN }).Take(10).ToList();
            return Json(nname);
        }


        //---------------------------------------------------------------------------------------------//

        [HttpPost]
        public JsonResult GetFEmpId(int Prefix)
        {
            var fempid = (from eof in db.Emp_offense
                          join fe in db.Former_EmpList on eof.EmpId equals fe.EmpID
                          where fe.EmpID.ToString().StartsWith(Prefix.ToString())
                          select new { label = fe.EmpID, val = fe.EmpID })
                         .Distinct() // ใช้ DISTINCT เพื่อตัดข้อมูลซ้ำ
                         .Take(10)
                         .ToList();

            return Json(fempid);
        }

        [HttpPost]
        public JsonResult GetFFName (string Prefix)
        {
            var ffname = (from eof in db.Emp_offense
                          join fe in db.Former_EmpList on eof.EmpId equals fe.EmpID
                          where fe.FName_EN.StartsWith(Prefix)
                          select new { label = fe.FName_EN ,val = fe.FName_EN})
                          .Distinct()
                          .Take(10)
                          .ToList();
            return Json(ffname);
        }

        [HttpPost]
        public JsonResult GetFLName (string Prefix)
        {
            var flname = (from eof in db.Emp_offense
                          join fe in db.Former_EmpList on eof.EmpId equals fe.EmpID
                          where fe.LName_EN.StartsWith(Prefix)
                          select new { label = fe.LName_EN, val = fe.LName_EN})
                          .Distinct()
                          .Take (10)
                          .ToList();
            return Json(flname);
        }

        [HttpPost]
        public JsonResult GetFNName(string Prefix)
        {
            var fnname = (from eof in db.Emp_offense
                          join fe in db.Former_EmpList on eof.EmpId equals fe.EmpID
                          where fe.NName_EN.StartsWith(Prefix)
                          select new {label = fe.NName_EN, val = fe.NName_EN})
                          .Distinct()
                          .Take(10)
                          .ToList(); 
            return Json(fnname);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
       
    }
}
