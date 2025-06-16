using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.EmployeeManagement
{
    [Authorize]
    public class EmployeeMasterReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_pic = ConfigurationManager.AppSettings["path_pic"];
        private string path_job = ConfigurationManager.AppSettings["path_job"];

        //
        // GET: /EmployeeMasterReport/Emplist
        [CustomAuthorize(53)]//Employee list report
        public ActionResult Emplist(SearchEmpModel model)
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
                string deptname = model.deptname;
                string position = model.position;
                string costcenter = model.costcenter;
                string nationality = model.nationality;
                string idnumber = model.idnumber;
                DateTime? dateworking_start = model.dateworking_start;
                DateTime? dateworking_end = model.dateworking_end;
                string emp_status = model.emp_status;
                string education = model.education;

                IEnumerable<EmpList> query = model.Page.HasValue ? db.EmpLists : db.EmpLists.Take(0);
                if (emp_id.HasValue)
                {
                    query = query.Where(x => x.EmpID == emp_id);
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.NName_EN)
                        && x.NName_EN.ToLowerInvariant().Contains(nickname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.FName_EN)
                        && x.FName_EN.ToLowerInvariant().Contains(fname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.LName_EN)
                        && x.LName_EN.ToLowerInvariant().Contains(lname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(deptname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.DeptDesc)
                        && x.DeptDesc == deptname);
                }
                if (!String.IsNullOrEmpty(position))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Position)
                        && x.Position == position);
                }
                if (!String.IsNullOrEmpty(costcenter))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CostCenter)
                        && x.CostCenter == costcenter);
                }
                if (!String.IsNullOrEmpty(nationality))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Nation)
                        && x.Nation == nationality);
                }
                if (!String.IsNullOrEmpty(idnumber))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.IDcode)
                        && x.IDcode.ToLowerInvariant().Contains(idnumber.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(emp_status))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.EmpStatus)
                        && x.EmpStatus == emp_status);
                }
                if (!String.IsNullOrEmpty(education))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Educate)
                        && x.EmpStatus == education);
                }
                if (dateworking_start.HasValue)
                {
                    DateTime sd = new DateTime(dateworking_start.Value.Year, dateworking_start.Value.Month, dateworking_start.Value.Day, 0, 0, 0);
                    query = query.Where(x => x.StartDate.HasValue && x.StartDate >= sd);
                }
                if (dateworking_end.HasValue)
                {
                    DateTime ed = new DateTime(dateworking_end.Value.Year, dateworking_end.Value.Month, dateworking_end.Value.Day, 23, 59, 59);
                    query = query.Where(x => x.StartDate.HasValue && x.StartDate <= ed);
                }
                var emp_list = query.Select(s => new EmployeeModel
                {
                    name_eng = String.Concat(s.Title_EN, s.FName_EN, " ", s.LName_EN),
                    name_th = String.Concat(s.Title_TH, s.FName_TH, " ", s.LName_TH),

                    EmpID = s.EmpID,
                    IDcode = s.IDcode,
                    PPID = s.PPID,
                    TaxID = s.TaxID,
                    SSO = s.SSO,
                    Title_TH = s.Title_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    NName_TH = s.NName_TH,
                    Title_EN = s.Title_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    NName_EN = s.NName_EN,
                    Gender = s.Gender,
                    Nation = s.Nation,
                    BDate = s.BDate,
                    Disabled_stat = s.Disabled_stat.HasValue ? s.Disabled_stat.Value : false,
                    Disabled_By = s.Disabled_By,
                    Addr_Build = s.Addr_Build,
                    Addr_No = s.Addr_No,
                    Addr_Alle = s.Addr_Alle,
                    Addr_Rd = s.Addr_Rd,
                    Addr_Vill = s.Addr_Vill,
                    Addr_Prsh = s.Addr_Prsh,
                    Addr_Dtrct = s.Addr_Dtrct,
                    Addr_Prvnc = s.Addr_Prvnc,
                    Addr_Post = s.Addr_Post,
                    Educate = s.Educate,
                    In_Email = s.In_Email,
                    Ex_Email = s.Ex_Email,
                    Mobile = s.Mobile,
                    Tel = s.Tel,
                    Bank_Acc = s.Bank_Acc,
                    DeptCode = s.DeptCode,
                    DeptDesc = s.DeptDesc,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                    StartDate = s.StartDate,
                    PassDate = s.PassDate,
                    StartPosition = s.StartPosition,
                    Note = s.Note,
                    EmpStatus = s.EmpStatus,
                    ReasonPPS = s.ReasonPPS,
                    DatePasSport = s.DatePasSport,
                    DatePasSportExpire = s.DatePasSportExpire,

                }).OrderBy(o => o.EmpID).ToList();
                //Concat Nick Name
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_EN)).ToList().ForEach(f => f.name_eng = String.Concat(f.name_eng, " (", f.NName_EN, ")"));
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_TH)).ToList().ForEach(f => f.name_th = String.Concat(f.name_th, " (", f.NName_TH, ")"));
                //Add Employee Picture
                foreach (var i in emp_list)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, i.EmpID.ToString(), ".png");
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
                pageSize = emp_list.Count() > 0 ? emp_list.Count() : 30;
                IPagedList<EmployeeModel> emp_pagedList = emp_list.ToPagedList(pageIndex, pageSize);
                //Select Department
                List<SelectListItem> selectDeptCode = db.emp_department.OrderBy(o => o.DeptCode)
                    .Select(s => new SelectListItem
                    {
                        Value = s.DeptName,
                        Text = String.Concat(s.DeptCode, ": ", s.DeptName),
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
                        Text = String.Concat(s.CostCenter, ": ", s.CostCenterName),
                    }).ToList();
                //Select Education
                List<SelectListItem> selectEducation = new List<SelectListItem>();
                selectEducation.Add(new SelectListItem { Value = "Bachelor", Text = "Bachelor" });
                selectEducation.Add(new SelectListItem { Value = "High Vocational Certificate", Text = "High Vocational Certificate" });
                selectEducation.Add(new SelectListItem { Value = "Junior High School", Text = "Junior High School" });
                selectEducation.Add(new SelectListItem { Value = "Master", Text = "Master" });
                selectEducation.Add(new SelectListItem { Value = "Senior High School", Text = "Senior High School" });
                selectEducation.Add(new SelectListItem { Value = "Vocational Certificate", Text = "Vocational Certificate" });
                //Select National
                List<SelectListItem> selectNationality = db.Emp_MasterNationality
                    .Select(s => new SelectListItem { Value = s.Nationality, Text = s.Nationality }).ToList();
                //Select Employee Status
                List<SelectListItem> selectEmpStatus = new List<SelectListItem>();
                selectEmpStatus.Add(new SelectListItem { Value = "Employee Ogura Clutch", Text = "Employee Ogura Clutch" });
                selectEmpStatus.Add(new SelectListItem { Value = "Sub Contractor", Text = "Sub Contractor" });

                //Get permission
                if (Session["USE_Id"] != null)
                {
                    int use_id = Convert.ToInt32(Session["USE_Id"]);
                    //To search with ID code (45)
                    int rights_45 = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 45).Count();
                    //To export data (49)
                    int rights_49 = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 49).Count();

                    model.rights_45 = rights_45 > 0 ? true : false;
                    model.rights_49 = rights_49 > 0 ? true : false;
                }

                model.EmployeePagedList = emp_pagedList;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.SelectEducation = selectEducation;
                model.SelectNational = selectNationality;
                model.SelectEmpStatus = selectEmpStatus;
                //Set Count Employee
                List<EmpList> query_all = db.EmpLists.ToList();
                int count_emp = query_all.Where(x => x.EmpStatus == "Employee Ogura Clutch").ToList().Count();
                int count_contractor = query_all.Where(x => x.EmpStatus == "Sub Contractor").ToList().Count();
                int count_all = query_all.Count();
                model.count_employee = count_emp;
                model.count_contractor = count_contractor;
                model.count_all = count_all;
                var data_last = db.Logs.Where(x => x.Log_System == "Administration" && x.Log_Type == "Employee List")
                    .OrderByDescending(o => o.Log_Date).FirstOrDefault();
                if (data_last != null)
                {
                    DateTime? last_date = data_last.Log_Date;
                    model.date_dataupdate = last_date.HasValue ? last_date.Value.ToString("dd MMMM yyyy hh:mm tt") : "";
                }
                //Set Count Employee Search
                model.count_employee_search = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch").ToList().Count();
                var count_employee_search_male = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch" && x.Gender == "Male").ToList().Count();
                var count_employee_search_female = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch" && x.Gender == "Female").ToList().Count();
                model.count_employee_search_detail = String.Concat("Male: ", count_employee_search_male, ", Female: ", count_employee_search_female);

                model.count_contractor_search = emp_list.Where(x => x.EmpStatus == "Sub Contractor").ToList().Count();
                var count_contractor_search_male = emp_list.Where(x => x.EmpStatus == "Sub Contractor" && x.Gender == "Male").ToList().Count();
                var count_contractor_search_female = emp_list.Where(x => x.EmpStatus == "Sub Contractor" && x.Gender == "Female").ToList().Count();
                model.count_contractor_search_detail = String.Concat("Male: ", count_contractor_search_male, ", Female: ", count_contractor_search_female);

                model.count_all_search = emp_list.Count();
                var count_all_search_male = emp_list.Where(x => x.Gender == "Male").ToList().Count();
                var count_all_search_female = emp_list.Where(x => x.Gender == "Female").ToList().Count();
                model.count_all_search_detail = String.Concat("Male: ", count_all_search_male, ", Female: ", count_all_search_female);
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /EmployeeMasterReport/EmpList {0}", ex.ToString());
                return View("Error");
            }   
        }

        //
        // POST: /EmployeeMasterReport/EmpList
        [HttpPost]
        [CustomAuthorize(53)]//Employee list report
        public ActionResult EmpList(FormCollection form, SearchEmpModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? emp_id = model.emp_id;
                string nickname = model.nickname;
                string fname = model.fname;
                string lname = model.lname;
                string deptname = model.deptname;
                string position = model.position;
                string costcenter = model.costcenter;
                string nationality = model.nationality;
                string idnumber = model.idnumber;
                DateTime? dateworking_start = model.dateworking_start;
                DateTime? dateworking_end = model.dateworking_end;
                string emp_status = model.emp_status;
                string education = model.education;

                IEnumerable<EmpList> query = db.EmpLists;
                if (emp_id.HasValue)
                {
                    query = query.Where(x => x.EmpID == emp_id);
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.NName_EN)
                        && x.NName_EN.ToLowerInvariant().Contains(nickname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.FName_EN)
                        && x.FName_EN.ToLowerInvariant().Contains(fname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.LName_EN)
                        && x.LName_EN.ToLowerInvariant().Contains(lname.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(deptname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.DeptDesc)
                        && x.DeptDesc == deptname);
                }
                if (!String.IsNullOrEmpty(position))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Position)
                        && x.Position == position);
                }
                if (!String.IsNullOrEmpty(costcenter))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CostCenter)
                        && x.CostCenter == costcenter);
                }
                if (!String.IsNullOrEmpty(nationality))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Nation)
                        && x.Nation.ToLowerInvariant().Contains(nationality.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(idnumber))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.IDcode)
                        && x.IDcode.ToLowerInvariant().Contains(idnumber.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(emp_status))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.EmpStatus)
                        && x.EmpStatus == emp_status);
                }
                if (!String.IsNullOrEmpty(education))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Educate)
                        && x.EmpStatus == education);
                }
                if (dateworking_start.HasValue)
                {
                    DateTime sd = new DateTime(dateworking_start.Value.Year, dateworking_start.Value.Month, dateworking_start.Value.Day, 0, 0, 0);
                    query = query.Where(x => x.StartDate.HasValue && x.StartDate >= sd);
                }
                if (dateworking_end.HasValue)
                {
                    DateTime ed = new DateTime(dateworking_end.Value.Year, dateworking_end.Value.Month, dateworking_end.Value.Day, 23, 59, 59);
                    query = query.Where(x => x.StartDate.HasValue && x.StartDate <= ed);
                }
                var emp_list = query.Select(s => new EmployeeModel
                {
                    name_eng = String.Concat(s.Title_EN, s.FName_EN, " ", s.LName_EN),
                    name_th = String.Concat(s.Title_TH, s.FName_TH, " ", s.LName_TH),

                    EmpID = s.EmpID,
                    IDcode = s.IDcode,
                    PPID = s.PPID,
                    TaxID = s.TaxID,
                    SSO = s.SSO,
                    Title_TH = s.Title_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    NName_TH = s.NName_TH,
                    Title_EN = s.Title_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    NName_EN = s.NName_EN,
                    Gender = s.Gender,
                    Nation = s.Nation,
                    BDate = s.BDate,
                    Disabled_stat = s.Disabled_stat.HasValue ? s.Disabled_stat.Value : false,
                    Disabled_By = s.Disabled_By,
                    Addr_Build = s.Addr_Build,
                    Addr_No = s.Addr_No,
                    Addr_Alle = s.Addr_Alle,
                    Addr_Rd = s.Addr_Rd,
                    Addr_Vill = s.Addr_Vill,
                    Addr_Prsh = s.Addr_Prsh,
                    Addr_Dtrct = s.Addr_Dtrct,
                    Addr_Prvnc = s.Addr_Prvnc,
                    Addr_Post = s.Addr_Post,
                    Educate = s.Educate,
                    In_Email = s.In_Email,
                    Ex_Email = s.Ex_Email,
                    Mobile = s.Mobile,
                    Tel = s.Tel,
                    Bank_Acc = s.Bank_Acc,
                    DeptCode = s.DeptCode,
                    DeptDesc = s.DeptDesc,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                    StartDate = s.StartDate,
                    PassDate = s.PassDate,
                    StartPosition = s.StartPosition,
                    Note = s.Note,
                    EmpStatus = s.EmpStatus,
                    ReasonPPS = s.ReasonPPS,
                    DatePasSport = s.DatePasSport,
                    DatePasSportExpire = s.DatePasSportExpire,

                }).OrderBy(o => o.EmpID).ToList();
                //Concat Nick Name
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_EN)).ToList().ForEach(f => f.name_eng = String.Concat(f.name_eng, " (", f.NName_EN, ")"));
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_TH)).ToList().ForEach(f => f.name_th = String.Concat(f.name_th, " (", f.NName_TH, ")"));

                //Add Employee Picture
                foreach (var i in emp_list)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, i.EmpID.ToString(), ".png");
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
                var exp = form["ExportToCsv"];
                //Export Csv
                if (exp == "ExportToCsv")
                {
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat(model.emp_id
                                        , "/", model.fname
                                        , " ", model.lname
                                        , "/", model.idnumber
                                        , "/", model.deptname);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "Export Fil",
                        Log_Type = "Employee List For Export File",
                        Log_System = "Administration",
                        Log_Detail = log_detail,
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();

                    ExportToCsv(emp_list);
                }
                var exe = form["ExportToExcel"];
                //Export Excel
                if (exe == "ExportToExcel")
                {
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat(model.emp_id
                                        , "/", model.fname
                                        , " ", model.lname
                                        , "/", model.idnumber
                                        , "/", model.deptname);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "Export Fil",
                        Log_Type = "Employee List For Export File",
                        Log_System = "Administration",
                        Log_Detail = log_detail,
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();

                    ExportToExcel(emp_list);
                }
                pageSize = emp_list.Count() > 0 ? emp_list.Count() : 30;
                IPagedList<EmployeeModel> emp_pagedList = emp_list.ToPagedList(pageIndex, pageSize);
                //Select Department
                List<SelectListItem> selectDeptCode = db.emp_department.OrderBy(o => o.DeptCode)
                    .Select(s => new SelectListItem
                    {
                        Value = s.DeptName,
                        Text = String.Concat(s.DeptCode, ": ", s.DeptName),
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
                        Text = String.Concat(s.CostCenter, ": ", s.CostCenterName),
                    }).ToList();
                //Select Education
                List<SelectListItem> selectEducation = new List<SelectListItem>();
                selectEducation.Add(new SelectListItem { Value = "Bachelor", Text = "Bachelor" });
                selectEducation.Add(new SelectListItem { Value = "High Vocational Certificate", Text = "High Vocational Certificate" });
                selectEducation.Add(new SelectListItem { Value = "Junior High School", Text = "Junior High School" });
                selectEducation.Add(new SelectListItem { Value = "Master", Text = "Master" });
                selectEducation.Add(new SelectListItem { Value = "Senior High School", Text = "Senior High School" });
                selectEducation.Add(new SelectListItem { Value = "Vocational Certificate", Text = "Vocational Certificate" });
                //Select National
                List<SelectListItem> selectNationality = db.Emp_MasterNationality
                    .Select(s => new SelectListItem { Value = s.Nationality, Text = s.Nationality }).ToList();
                //Select Employee Status
                List<SelectListItem> selectEmpStatus = new List<SelectListItem>();
                selectEmpStatus.Add(new SelectListItem { Value = "Employee Ogura Clutch", Text = "Employee Ogura Clutch" });
                selectEmpStatus.Add(new SelectListItem { Value = "Sub Contractor", Text = "Sub Contractor" });

                model.EmployeePagedList = emp_pagedList;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.SelectEducation = selectEducation;
                model.SelectNational = selectNationality;
                model.SelectEmpStatus = selectEmpStatus;

                //Set Count Employee Search
                model.count_employee_search = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch").ToList().Count();
                var count_employee_search_male = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch" && x.Gender == "Male").ToList().Count();
                var count_employee_search_female = emp_list.Where(x => x.EmpStatus == "Employee Ogura Clutch" && x.Gender == "Female").ToList().Count();
                model.count_employee_search_detail = String.Concat("Male: ", count_employee_search_male, ", Female: ", count_employee_search_female);

                model.count_contractor_search = emp_list.Where(x => x.EmpStatus == "Sub Contractor").ToList().Count();
                var count_contractor_search_male = emp_list.Where(x => x.EmpStatus == "Sub Contractor" && x.Gender == "Male").ToList().Count();
                var count_contractor_search_female = emp_list.Where(x => x.EmpStatus == "Sub Contractor" && x.Gender == "Female").ToList().Count();
                model.count_contractor_search_detail = String.Concat("Male: ", count_contractor_search_male, ", Female: ", count_contractor_search_female);

                model.count_all_search = emp_list.Count();
                var count_all_search_male = emp_list.Where(x => x.Gender == "Male").ToList().Count();
                var count_all_search_female = emp_list.Where(x => x.Gender == "Female").ToList().Count();
                model.count_all_search_detail = String.Concat("Male: ", count_all_search_male, ", Female: ", count_all_search_female);

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /EmployeeMasterReport/EmpList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /EmployeeMasterReport/EmpDetail
        [CustomAuthorize(53)]//Employee list report
        public ActionResult EmpDetail(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                var model = db.EmpLists.Where(x => x.EmpID == id).Select(s => new EmployeeModel
                {
                    id = s.EmpID,
                    EmpID = s.EmpID,
                    Title_TH = s.Title_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    NName_TH = s.NName_TH,
                    Title_EN = s.Title_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    NName_EN = s.NName_EN,
                    Gender = s.Gender,
                    Nation = s.Nation,
                    BDate = s.BDate,
                    Disabled_stat = s.Disabled_stat.HasValue ? s.Disabled_stat.Value : false,
                    Disabled_By = s.Disabled_By,
                    Educate = s.Educate,
                    In_Email = s.In_Email,
                    Ex_Email = s.Ex_Email,
                    Mobile = s.Mobile,
                    Tel = s.Tel,
                    DeptCode = s.DeptCode,
                    DeptDesc = s.DeptDesc,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                    StartDate = s.StartDate,
                    PassDate = s.PassDate,
                    StartPosition = s.StartPosition,
                    Note = s.Note,
                    EmpStatus = s.EmpStatus,
                    pvd1_start_date = s.StartDatePVD1,
                    pvd1_end_date = s.EndDatePVD1,
                    pvd2_start_date = s.StartDatePVD2,
                    pvd2_end_date = s.EndDatePVD2,
                   
                }).FirstOrDefault();

                //Get permission
                if (Session["USE_Id"] != null)
                {
                    List<int> hr_rights_id = new List<int>() { 58, 60 };
                    int use_id = Convert.ToInt32(Session["USE_Id"]);

                    List<int> rights = db.UserRights.Where(x => x.USE_Id == use_id
                        && hr_rights_id.Contains(x.RIH_Id)).Select(s => s.RIH_Id).ToList();

                    //Can see Training Employee (58)
                    model.rights_see_training = rights.Contains(58) ? true : false;
                    //Can see Work Leave All (60)
                    model.rights_see_leave = rights.Contains(60) ? true : false;
                }
                else
                { 
                    model.rights_see_training = false;
                    model.rights_see_leave = false;
                }

                //Get Logo
                string logoPath = Server.MapPath("~/static/img/") + "kihonrogo_A.jpg";
                //Check file exist
                if (System.IO.File.Exists(logoPath))
                {
                    //Convert image to byte array
                    byte[] byteData = System.IO.File.ReadAllBytes(logoPath);
                    //Convert byte array to base64string
                    string imreBase64Data = Convert.ToBase64String(byteData);
                    string imgDataURL = string.Format("data:image/jpg;base64,{0}", imreBase64Data);
                    //Passing image data in model to view
                    model.logo = imgDataURL;
                }

                //Get image path
                string imgPath = String.Concat(path_pic, model.EmpID.ToString(), ".png");
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
                //Get Job description path
                string jobPath = String.Concat(path_job, model.EmpID.ToString(), "_jobdescription.pdf");
                if (System.IO.File.Exists(jobPath))
                {
                    model.jobdes_path = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf"); ;
                }
              
                //Set Working period
                if (model.StartDate.HasValue)
                {
                    DateTime date1 = DateTime.Today;
                    DateTime date2 = model.StartDate.Value;
                    int months = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
                    int diff_days = Math.Abs(date1.Day - date2.Day);
                    int diff_months = 0;
                    int diff_years = 0;
                    if (months > 0)
                    {
                        diff_months = months % 12;
                        diff_years = months / 12;
                    }
                    string diff_years_str = diff_years > 0 ? String.Concat(diff_years.ToString(), " years ") : null;
                    string diff_months_str = diff_months > 0 ? String.Concat(diff_months.ToString(), " months ") : null;
                    string diff_days_str = diff_days > 0 ? String.Concat(diff_days.ToString(), " days") : null;
                    model.working_period = String.Concat(diff_years_str, diff_months_str, diff_days_str);
                }

                //set Position period
                if(model.StartPosition.HasValue) { 
                    DateTime cr_position_date = DateTime.Today;
                    DateTime st_position_date = model.StartPosition.Value;
                    int pt_month = Math.Abs(((cr_position_date.Year - st_position_date.Year) * 12) + cr_position_date.Month - st_position_date.Month);
                    int pt_diff_day = Math.Abs(cr_position_date.Day - st_position_date.Day);
                    int pt_diff_months = 0;
                    int pt_diff_years = 0;

                    if (pt_month > 0)
                    {
                        pt_diff_months = pt_month % 12;
                        pt_diff_years = pt_month / 12;
                    }

                    string pt_diff_years_str = pt_diff_years > 0 ? String.Concat(pt_diff_years.ToString(), " years ") : null;
                    string pt_diff_months_str = pt_diff_months > 0 ? String.Concat(pt_diff_months.ToString(), " months ") : null;
                    string pt_diff_days_str = pt_diff_day > 0 ? String.Concat(pt_diff_day.ToString(), " days") : null;
                    model.position_period = String.Concat(pt_diff_years_str, pt_diff_months_str, pt_diff_days_str);
                
                }
                //Set Leave date
                int year = DateTime.Today.Year;
                if (model.EmpStatus == "Employee Ogura Clutch")
                {
                    model.leave_start_date = new DateTime(year - 1, 11, 21);
                    model.leave_end_date = new DateTime(year, 11, 20);
                }
                else
                {
                    model.leave_start_date = new DateTime(year, 1, 1);
                    model.leave_end_date = new DateTime(year, 12, 31);
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
        // GET: /EmployeeMasterReport/GetEmpDetail
        [CustomAuthorize(53)]//Employee list report
        public ActionResult GetEmpDetail(int id)
        {
            try
            {
                var model = db.EmpLists.Where(x => x.EmpID == id).Select(s => new EmployeeModel
                {
                    id = s.EmpID,
                    EmpID = s.EmpID,
                    Title_TH = s.Title_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    NName_TH = s.NName_TH,
                    Title_EN = s.Title_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    NName_EN = s.NName_EN,
                    Gender = s.Gender,
                    Nation = s.Nation,
                    BDate = s.BDate,
                    Disabled_stat = s.Disabled_stat.HasValue ? s.Disabled_stat.Value : false,
                    Disabled_By = s.Disabled_By,
                    Educate = s.Educate,
                    In_Email = s.In_Email,
                    Ex_Email = s.Ex_Email,
                    Mobile = s.Mobile,
                    Tel = s.Tel,
                    DeptCode = s.DeptCode,
                    DeptDesc = s.DeptDesc,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                    StartDate = s.StartDate,
                    PassDate = s.PassDate,
                    StartPosition = s.StartPosition,
                    Note = s.Note,
                    EmpStatus = s.EmpStatus,
                    pvd1_start_date = s.StartDatePVD1,
                    pvd1_end_date = s.EndDatePVD1,
                    pvd2_start_date = s.StartDatePVD2,
                    pvd2_end_date = s.EndDatePVD2,

                }).FirstOrDefault();

                //Get permission
                if (Session["USE_Id"] != null)
                {
                    List<int> hr_rights_id = new List<int>() { 58, 60 };
                    int use_id = Convert.ToInt32(Session["USE_Id"]);

                    List<int> rights = db.UserRights.Where(x => x.USE_Id == use_id
                        && hr_rights_id.Contains(x.RIH_Id)).Select(s => s.RIH_Id).ToList();

                    //Can see Training Employee (58)
                    model.rights_see_training = rights.Contains(58) ? true : false;
                    //Can see Work Leave All (60)
                    model.rights_see_leave = rights.Contains(60) ? true : false;
                }
                else
                {
                    model.rights_see_training = false;
                    model.rights_see_leave = false;
                }

                //Get Logo
                string logoPath = Server.MapPath("~/static/img/") + "kihonrogo_A.jpg";
                //Check file exist
                if (System.IO.File.Exists(logoPath))
                {
                    //Convert image to byte array
                    byte[] byteData = System.IO.File.ReadAllBytes(logoPath);
                    //Convert byte array to base64string
                    string imreBase64Data = Convert.ToBase64String(byteData);
                    string imgDataURL = string.Format("data:image/jpg;base64,{0}", imreBase64Data);
                    //Passing image data in model to view
                    model.logo = imgDataURL;
                }

                //Get image path
                string imgPath = String.Concat(path_pic, model.EmpID.ToString(), ".png");
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
                //Get Job description path
                string jobPath = String.Concat(path_job, model.EmpID.ToString(), "_jobdescription.pdf");
                if (System.IO.File.Exists(jobPath))
                {
                    model.jobdes_path = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf"); ;
                }

                //Set Working period
                if (model.StartDate.HasValue)
                {
                    DateTime date1 = DateTime.Today;
                    DateTime date2 = model.StartDate.Value;
                    int months = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
                    int diff_days = Math.Abs(date1.Day - date2.Day);
                    int diff_months = 0;
                    int diff_years = 0;
                    if (months > 0)
                    {
                        diff_months = months % 12;
                        diff_years = months / 12;
                    }
                    string diff_years_str = diff_years > 0 ? String.Concat(diff_years.ToString(), " years ") : null;
                    string diff_months_str = diff_months > 0 ? String.Concat(diff_months.ToString(), " months ") : null;
                    string diff_days_str = diff_days > 0 ? String.Concat(diff_days.ToString(), " days") : null;
                    model.working_period = String.Concat(diff_years_str, diff_months_str, diff_days_str);
                }

                //set Position period
                if (model.StartPosition.HasValue)
                {
                    DateTime cr_position_date = DateTime.Today;
                    DateTime st_position_date = model.StartPosition.Value;
                    int pt_month = Math.Abs(((cr_position_date.Year - st_position_date.Year) * 12) + cr_position_date.Month - st_position_date.Month);
                    int pt_diff_day = Math.Abs(cr_position_date.Day - st_position_date.Day);
                    int pt_diff_months = 0;
                    int pt_diff_years = 0;

                    if (pt_month > 0)
                    {
                        pt_diff_months = pt_month % 12;
                        pt_diff_years = pt_month / 12;
                    }

                    string pt_diff_years_str = pt_diff_years > 0 ? String.Concat(pt_diff_years.ToString(), " years ") : null;
                    string pt_diff_months_str = pt_diff_months > 0 ? String.Concat(pt_diff_months.ToString(), " months ") : null;
                    string pt_diff_days_str = pt_diff_day > 0 ? String.Concat(pt_diff_day.ToString(), " days") : null;
                    model.position_period = String.Concat(pt_diff_years_str, pt_diff_months_str, pt_diff_days_str);

                }
                //Set Leave date
                int year = DateTime.Today.Year;
                if (model.EmpStatus == "Employee Ogura Clutch")
                {
                    model.leave_start_date = new DateTime(year - 1, 11, 21);
                    model.leave_end_date = new DateTime(year, 11, 20);
                }
                else
                {
                    model.leave_start_date = new DateTime(year, 1, 1);
                    model.leave_end_date = new DateTime(year, 12, 31);
                }

                return PartialView("GetEmpDetail",model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        // GET: /EmployeeMasterReport/EmpDetail
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult EmpCourse(int id)
        {
            try
            {
                EmployeeModel model = new EmployeeModel();
                model.id = id;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        // GET: /EmployeeMasterReport/GetEmpCourse
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult GetEmpCourse(int id)
        {
            try
            {
                EmployeeModel model = new EmployeeModel();
                model.id = id;

                return PartialView("GetEmpCourse",model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //Download file
        public ActionResult DownloadFile(string fileName)
        {
            //Build the File Path.
            //string path = Server.MapPath("~/File/") + fileName;  --visual path
            string path = path_job + fileName;
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

        //  /EmployeeMasterReport/ExportToCsv
        public void ExportToCsv(List<EmployeeModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    emp_id = v.EmpID,
                    title_th = v.Title_TH,
                    fname_th = v.FName_TH,
                    lname_th = v.LName_TH,
                    nickname_th = v.NName_TH,
                    title_en = v.Title_EN,
                    fname_en = v.FName_EN,
                    lname_en = v.LName_EN,
                    nick_en = v.NName_EN,
                    gender = v.Gender,
                    nationality = v.Nation,
                    disabled_status = v.Disabled_stat.ToString(),
                    disabled_by = v.Disabled_By,
                    dept_code = "\"" + v.DeptCode + "\"",
                    dept_name = "\"" + v.DeptDesc + "\"",
                    position = "\"" + v.Position + "\"",
                    cost_center = v.CostCenter,
                    cost_center_name = "\"" + v.CostCenterName + "\"",
                    start_date = v.StartDate,
                    pass_date = v.PassDate,
                    employee_status = v.EmpStatus,
                    note = "\"" + v.Note + "\""

                });

                sb.AppendFormat(
                    "{0},{1},{2},{3},{4}"
                    + ",{5},{6},{7},{8},{9}"
                    + ",{10},{11},{12},{13},{14}"
                    + ",{15},{16},{17},{18},{19},{20}"
                    , "EmpID"
                    , "Title TH", "First name TH", "Last name TH", "Nickname TH", "Title EN"
                    , "First name EN", "Last name EN", "Nickname EN", "Gender", "Nationallity"
                    , "Dept code", "Dept name", "Position"
                    , "Cost center", "CostCenterName", "Start date", "Pass date", "Employee Status"
                    , "Note"
                    , Environment.NewLine);

                string start_date_str = null;
                string pass_date_str = null;
                foreach (var i in forexport)
                {
                    start_date_str = i.start_date.HasValue ? i.start_date.Value.ToString("yyyy-MM-dd") : null;
                    pass_date_str = i.pass_date.HasValue ? i.pass_date.Value.ToString("yyyy-MM-dd") : null;
                    sb.AppendFormat(
                       "{0},{1},{2},{3},{4}"
                       + ",{5},{6},{7},{8},{9}"
                       + ",{10},{11},{12},{13},{14}"
                       + ",{15},{16},{17},{18},{19},{20}"
                       , i.emp_id
                       , i.title_th, i.fname_th, i.lname_th, i.nickname_th, i.title_en
                       , i.fname_en, i.lname_en, i.nick_en, i.gender, i.nationality
                       , i.dept_code, i.dept_name, i.position
                       , i.cost_center, i.cost_center_name, start_date_str, pass_date_str, i.employee_status
                       , i.note
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
                response.AddHeader("content-disposition", "attachment;filename=Employee_Data_Report.CSV ");
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
        //Export Excel
        public void ExportToExcel(List<EmployeeModel> model)
        {
            try
            {
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                //Column name
                workSheet.Cells[1, 1].Value = "Picture";
                workSheet.Cells[1, 2].Value = "EmpID";
                workSheet.Cells[1, 3].Value = "Title TH";
                workSheet.Cells[1, 4].Value = "Fist name TH";
                workSheet.Cells[1, 5].Value = "Last name TH";
                workSheet.Cells[1, 6].Value = "Nickname TH";
                workSheet.Cells[1, 7].Value = "Title EN";
                workSheet.Cells[1, 8].Value = "First name EN";
                workSheet.Cells[1, 9].Value = "Last name EN";
                workSheet.Cells[1, 10].Value = "Nickname EN";
                workSheet.Cells[1, 11].Value = "Gender";
                workSheet.Cells[1, 12].Value = "Nationallity";
                workSheet.Cells[1, 13].Value = "Dept code";
                workSheet.Cells[1, 14].Value = "Dept name";
                workSheet.Cells[1, 15].Value = "Position";
                workSheet.Cells[1, 16].Value = "Cost Center";
                workSheet.Cells[1, 17].Value = "Cost Center Name";
                workSheet.Cells[1, 18].Value = "Start date";
                workSheet.Cells[1, 19].Value = "Pass date";
                workSheet.Cells[1, 20].Value = "Employee Status";
                workSheet.Cells[1, 21].Value = "Note";
                Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B7DEE8");
                workSheet.Cells["A1:U1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A1:U1"].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                workSheet.Cells["A1:U1"].Style.Font.Size = 12;
                workSheet.Cells["A1:U1"].Style.Font.Bold = true;
                workSheet.Cells["A1:U1"].Style.Font.Color.SetColor(Color.White);
                foreach (var dd in model.Select((val, i) => new { val = val, i = i }))
                {
                    int row = dd.i + 2;
                    workSheet.Row(row).Height = 80;
                    workSheet.Row(row).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    string imgPath = String.Concat(path_pic, dd.val.EmpID.ToString(), ".png");
                    if (System.IO.File.Exists(imgPath))
                    {
                        Image img = Image.FromFile(imgPath);
                        ExcelPicture pic = workSheet.Drawings.AddPicture(dd.val.EmpID.ToString(), img);
                        pic.SetPosition(row - 1, 0, 0, 0);
                        pic.SetSize(80, 80);
                    }

                    //workSheet.Cells[row, 1].Value = dd.i + 1;
                    workSheet.Cells[row, 2].Value = dd.val.EmpID;
                    workSheet.Cells[row, 3].Value = dd.val.Title_TH;
                    workSheet.Cells[row, 4].Value = dd.val.FName_TH;
                    workSheet.Cells[row, 5].Value = dd.val.LName_TH;
                    workSheet.Cells[row, 6].Value = dd.val.NName_TH;
                    workSheet.Cells[row, 7].Value = dd.val.Title_EN;
                    workSheet.Cells[row, 8].Value = dd.val.FName_EN;
                    workSheet.Cells[row, 9].Value = dd.val.LName_EN;
                    workSheet.Cells[row, 10].Value = dd.val.NName_EN;
                    workSheet.Cells[row, 11].Value = dd.val.Gender;
                    workSheet.Cells[row, 12].Value = dd.val.Nation;
                    workSheet.Cells[row, 13].Value = dd.val.DeptCode;
                    workSheet.Cells[row, 14].Value = dd.val.DeptDesc;
                    workSheet.Cells[row, 15].Value = dd.val.Position;
                    workSheet.Cells[row, 16].Value = dd.val.CostCenter;
                    workSheet.Cells[row, 17].Value = dd.val.CostCenterName;
                    workSheet.Cells[row, 18].Value = dd.val.StartDate.HasValue ? dd.val.StartDate.Value.ToString("dd/MM/yyyy") : "";
                    workSheet.Cells[row, 19].Value = dd.val.PassDate.HasValue ? dd.val.PassDate.Value.ToString("dd/MM/yyyy") : "";
                    workSheet.Cells[row, 20].Value = dd.val.EmpStatus;
                    workSheet.Cells[row, 21].Value = dd.val.Note;
                }

                workSheet.Protection.IsProtected = false;
                workSheet.Protection.AllowSelectLockedCells = false;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.Column(1).Width = 15;
                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;  filename=Employee_Data.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }


        //
        //GET: /EmployeeMasterReport/GetEmpSum
        public ActionResult GetEmpSum(int id)
        {
            string emp_type = id > 0 ? "Employee Ogura Clutch" : "Sub Contractor";

            var result = db.EmpLists.Where(x => x.EmpStatus.Trim().Contains(emp_type));
            var group = from re in result
                        group re by new { re.DeptDesc, re.Gender } into g
                        select new { department = g.Key.DeptDesc, gender = g.Key.Gender, total = g.Count() };

            var dept_list = group.Select(s => s.department).Distinct();
            EmpSumListModel empSumList = new EmpSumListModel();
            List<EmpSumModel> model_list = new List<EmpSumModel>();
            foreach (var item in dept_list)
            {
                EmpSumModel model = new EmpSumModel();
                var group_dept = group.Where(x => x.department == item).ToList();
                model.department = item;

                foreach (var g in group_dept)
                {
                    if (g.gender == "Male")
                    {
                        model.sum_male = g.total;
                    }
                    else
                    {
                        model.sum_female = g.total;
                    }
                }
                model.total = model.sum_male + model.sum_female;
                model_list.Add(model);
            }
            empSumList.title = id > 0 ? "Ogura Clutch Employees" : "Sub Contractor Employees";
            empSumList.sum_male = group.Where(x => x.gender == "Male").Select(s => s.total).Sum();
            empSumList.sum_female = group.Where(x => x.gender != "Male").Select(s => s.total).Sum();
            empSumList.total = group.Select(s => s.total).Sum();
            empSumList.EmpSumList = model_list;

            return PartialView("GetEmpSum", empSumList);
        }

        //
        //GET: /EmployeeMasterReport/GetTotalAll
        public JsonResult GetTotalAll(int id)
        {
            var result = db.EmpLists;
            var sum_male = result.Where(x => x.Gender == "Male").Count();
            var sum_female = result.Where(x => x.Gender == "Female").Count();
            var json_result = new { sum_male = sum_male, sum_female = sum_female };
            return Json(json_result, JsonRequestBehavior.AllowGet);
        }



        // GET: /EmployeeMasterReport/GetProvince
        public JsonResult GetProvince()
        {
            var province = db.Emp_MasterRegion.Select(s => s.province).Distinct()
                .OrderBy(o => o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(province, JsonRequestBehavior.AllowGet);
        }
        // POST: /EmployeeMasterReport/GetDistrict
        public JsonResult GetDistrict(string province)
        {
            var district = db.Emp_MasterRegion.Where(w => w.province == province).Select(s => s.district).Distinct()
                .OrderBy(o => o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(district, JsonRequestBehavior.AllowGet);
        }
        // POST: /EmployeeMasterReport/GetSubDistrict
        public JsonResult GetSubDistrict(string district)
        {
            var subdistrict = db.Emp_MasterRegion.Where(w => w.district == district).Select(s => s.parish).Distinct()
                .OrderBy(o => o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(subdistrict, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /EmployeeMasterReport/GetEmpId/
        [HttpPost]
        public JsonResult GetEmpId(int Prefix)
        {
            var empid = (from el in db.EmpLists
                         where el.EmpID.ToString().StartsWith(Prefix.ToString())
                         select new { label = el.EmpID, val = el.EmpID }).Take(10).ToList();
            return Json(empid);
        }
        //
        // POST: /EmployeeMasterReport/GetFName/
        [HttpPost]
        public JsonResult GetFName(string Prefix)
        {
            var fname = (from el in db.EmpLists
                         where el.FName_EN.StartsWith(Prefix)
                         select new { label = el.FName_EN, val = el.FName_EN }).Take(10).ToList();
            return Json(fname);
        }
        //
        // POST: /EmployeeMasterReport/GetLName/
        [HttpPost]
        public JsonResult GetLName(string Prefix)
        {
            var lname = (from el in db.EmpLists
                         where el.LName_EN.StartsWith(Prefix)
                         select new { label = el.LName_EN, val = el.LName_EN }).Take(10).ToList();
            return Json(lname);
        }
        //
        // POST: /EmployeeMasterReport/GetNName/
        [HttpPost]
        public JsonResult GetNName(string Prefix)
        {
            var nname = (from el in db.EmpLists
                         where el.NName_EN.StartsWith(Prefix)
                         select new { label = el.NName_EN, val = el.NName_EN }).Take(10).ToList();
            return Json(nname);
        }
        //
        // POST: /EmployeeMasterReport/GetIDNumber/
        [HttpPost]
        public JsonResult GetIDNumber(string Prefix)
        {
            var idnumber = (from el in db.EmpLists
                            where el.IDcode.StartsWith(Prefix)
                            select new { label = el.IDcode, val = el.IDcode }).Take(10).ToList();
            return Json(idnumber);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }        
    }
}
