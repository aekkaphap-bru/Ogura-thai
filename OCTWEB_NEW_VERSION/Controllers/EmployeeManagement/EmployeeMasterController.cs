using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.EmployeeManagement
{
    [Authorize]
    public class EmployeeMasterController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_pic = ConfigurationManager.AppSettings["path_pic"];
        private string path_job = ConfigurationManager.AppSettings["path_job"];

        //
        // GET: /EmployeeMaster/EmpList
        [CustomAuthorize(32)]//Employee list setup
        public ActionResult EmpList(SearchEmpModel model)
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

                int? emp_id  = model.emp_id;
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
                if(emp_id.HasValue )
                {
                    query = query.Where(x => x.EmpID == emp_id);
                }
                if(!String.IsNullOrEmpty(nickname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.NName_EN)
                        && x.NName_EN.ToLowerInvariant().Contains(nickname.ToLowerInvariant()));
                }
                if(!String.IsNullOrEmpty(fname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.FName_EN)
                        && x.FName_EN.ToLowerInvariant().Contains(fname.ToLowerInvariant()));
                }
                if(!String.IsNullOrEmpty(lname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.LName_EN)
                        && x.LName_EN.ToLowerInvariant().Contains(lname.ToLowerInvariant()));
                }
                if(!String.IsNullOrEmpty(deptname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.DeptDesc)
                        && x.DeptDesc == deptname);
                }
                if (!String.IsNullOrEmpty(position))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Position)
                        && x.Position == position);
                }
                if(!String.IsNullOrEmpty(costcenter))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CostCenter)
                        && x.CostCenter == costcenter);
                }
                if(!String.IsNullOrEmpty(nationality))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Nation)
                        && x.Nation == nationality);
                }
                if(!String.IsNullOrEmpty(idnumber))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.IDcode)
                        && x.IDcode.ToLowerInvariant().Contains(idnumber.ToLowerInvariant()));
                }
                if(!String.IsNullOrEmpty(emp_status))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.EmpStatus)
                        && x.EmpStatus == emp_status);
                }
                if (!String.IsNullOrEmpty(education))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Educate)
                        && x.Educate == education);
                }
                if (dateworking_start.HasValue)
                {
                    DateTime sd = new DateTime(dateworking_start.Value.Year, dateworking_start.Value.Month, dateworking_start.Value.Day, 0, 0, 0);
                    query = query.Where(x => x.StartDate.HasValue && x.StartDate >=  sd);
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
                     IDcode  = s.IDcode,
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
   
                }).OrderBy(o=>o.EmpID).ToList();
                //Concat Nick Name
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_EN)).ToList().ForEach(f => f.name_eng = String.Concat(f.name_eng, " (", f.NName_EN, ")"));
                emp_list.Where(x => !String.IsNullOrEmpty(x.NName_TH)).ToList().ForEach(f => f.name_th = String.Concat(f.name_th, " (", f.NName_TH, ")"));
                //Add Employee Picture
                foreach(var i in emp_list)
                {
                    //Get image path
                    string imgPath = String.Concat(path_pic, i.EmpID.ToString(), ".png");
                    //Check file exist
                    if(System.IO.File.Exists(imgPath))
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
                pageSize = emp_list.Count() > 0 ? emp_list.Count() : 30 ;
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
                var data_last = db.Logs.Where(x => x.Log_System == "Administration" && x.Log_Type == "Employee List" )
                    .OrderByDescending(o=>o.Log_Date).FirstOrDefault();
                if(data_last != null)
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
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /EmployeeMaster/EmpList {0}", ex.ToString());
                return View("Error");
            }   
        }

        //
        // POST: /EmployeeMaster/EmpList
        [HttpPost]
        [CustomAuthorize(32)]//Employee list setup
        public ActionResult EmpList(FormCollection form,SearchEmpModel model)
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
                        && x.Educate == education);
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
                ViewBag.errorMessage = String.Format("Error: Get /EmployeeMaster/EmpList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /EmployeeMaster/EmpDetail
        [CustomAuthorize(32)]//Employee list setup
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
                    pvd1_start_date = s.StartDatePVD1,
                    pvd1_end_date = s.EndDatePVD1,
                    pvd2_start_date = s.StartDatePVD2,
                    pvd2_end_date = s.EndDatePVD2,
                }).FirstOrDefault();

                //Get permission
                if (Session["USE_Id"] != null)
                {
                    List<int> hr_rights_id = new List<int>() { 38, 44, 50, 52, 58, 60 };
                    int use_id = Convert.ToInt32(Session["USE_Id"]);

                    List<int> rights = db.UserRights.Where(x => x.USE_Id == use_id
                        && hr_rights_id.Contains(x.RIH_Id)).Select(s => s.RIH_Id).ToList();

                    //To see employees IDcode/PassportID/TaxID/SSO (38)
                    if (!rights.Contains(38))
                    {
                        model.rights_see_IDcode_PassportID_TaxID_SSO = false;
                        model.IDcode = !String.IsNullOrEmpty(model.IDcode) ? "*****************" : "";
                        model.PPID = !String.IsNullOrEmpty(model.PPID) ? "*****************" : "";
                        model.TaxID = !String.IsNullOrEmpty(model.TaxID) ? "*****************" : "";
                        model.SSO = !String.IsNullOrEmpty(model.SSO) ? "*****************" : "";
                    }
                    else
                    {
                        model.rights_see_IDcode_PassportID_TaxID_SSO = true;
                    }
                    //To see employees birth date info (44)
                    if(!rights.Contains(44))
                    {
                        model.rights_see_birth_date = false;
                        model.BDate = null;
                    }
                    else
                    {
                        model.rights_see_birth_date = true;
                    }
                    //To unlock page and edit the info (50)
                    model.rights_unlockpage = rights.Contains(50) ? true : false; 
                    //To layoff employees (52)
                    model.rights_layoff = rights.Contains(52) ? true : false;
                    //Can see Training Employee (58)
                    model.rights_see_training = rights.Contains(58) ? true : false;
                    //Can see Work Leave All (60)
                    model.rights_see_leave = rights.Contains(60) ? true : false;

                }
                else
                {
                    model.IDcode = !String.IsNullOrEmpty(model.IDcode) ? "*****************" : "";
                    model.PPID = !String.IsNullOrEmpty(model.PPID) ? "*****************" : "";
                    model.TaxID = !String.IsNullOrEmpty(model.TaxID) ? "*****************" : "";
                    model.SSO = !String.IsNullOrEmpty(model.SSO) ? "*****************" : "";
                    model.BDate = null;
                    model.rights_unlockpage = false;
                    model.rights_see_training = false;
                    model.rights_layoff = false;
                    model.rights_see_leave = false;
                    model.rights_see_IDcode_PassportID_TaxID_SSO = false;
                    model.rights_see_birth_date = false;
                }

                //Get Select Options
                GetSelectOption(model);

                //Get image path
                string imgPath = String.Concat(path_pic, model.EmpID.ToString(), ".png");
                //Check file exist
                if(System.IO.File.Exists(imgPath))
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
                if(System.IO.File.Exists(jobPath))
                {
                    model.jobdes_path = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf"); ;
                }
                //Set Address string
                string addr_build = !String.IsNullOrEmpty(model.Addr_Build) ?
                    String.Concat(model.Addr_Build," ") : null;
                string addr_no = !String.IsNullOrEmpty(model.Addr_No) ?
                    String.Concat("เลขที่ ",model.Addr_No," "): null;
                string addr_alle = !String.IsNullOrEmpty(model.Addr_Alle) ?
                    String.Concat("ซ.",model.Addr_Alle," "): null;
                string addr_road = !String.IsNullOrEmpty(model.Addr_Rd) ?
                    String.Concat("ถ.",model.Addr_Rd," "): null;
                string addr_moo = !String.IsNullOrEmpty(model.Addr_Vill) ?
                    String.Concat("ม.",model.Addr_Vill," ") : null;
                string addr_province = null;
                string addr_district = null;
                string addr_sub_district = null;
                if(!String.IsNullOrEmpty(model.Addr_Prvnc))
                {
                    addr_province = model.Addr_Prvnc == "กรุงเทพมหานคร" ?
                        String.Concat(model.Addr_Prvnc," "):
                        String.Concat("จ.",model.Addr_Prvnc," ");
                }
                if(!String.IsNullOrEmpty(model.Addr_Dtrct))
                {
                    addr_district = model.Addr_Prvnc == "กรุงเทพมหานคร" ?
                        String.Concat("เขต",model.Addr_Dtrct," "):
                        String.Concat("อ.",model.Addr_Dtrct," ");
                }
                if(!String.IsNullOrEmpty(model.Addr_Prsh))
                {
                    addr_sub_district = model.Addr_Prvnc == "กรุงเทพมหานคร" ?
                        String.Concat("แขวง",model.Addr_Prsh," "):
                        String.Concat("ต.",model.Addr_Prsh," ");
                }
                model.address_str = String.Concat(addr_build, addr_no, addr_alle
                    , addr_road, addr_moo, addr_sub_district, addr_district, addr_province, model.Addr_Post);

                //Set Working period
                if(model.StartDate.HasValue)
                {
                    DateTime date1 = DateTime.Today;
                    DateTime date2 = model.StartDate.Value;
                    int months = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
                    int diff_days = Math.Abs(date1.Day - date2.Day );
                    int diff_months = 0;
                    int diff_years = 0;
                    if(months > 0)
                    {
                        diff_months = months % 12;
                        diff_years = months / 12;
                    }
                    string diff_years_str = diff_years > 0 ? String.Concat(diff_years.ToString(), " years ") : null;
                    string diff_months_str = diff_months > 0 ?  String.Concat(diff_months.ToString()," months "): null;
                    string diff_days_str = diff_days >0 ? String.Concat(diff_days.ToString()," days"): null;
                    model.working_period = String.Concat(diff_years_str, diff_months_str, diff_days_str);
                }
                //Set Age
                if (model.BDate.HasValue)
                {
                    DateTime date1 = DateTime.Today;
                    DateTime date2 = model.BDate.Value;
                    int months = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
                    int diff_years = 0;
                    if (months > 0)
                    {
                        diff_years = months / 12;
                    }
                    model.age_str = String.Concat(diff_years.ToString()," years old");
                }
                //Set Leave date
                int year = DateTime.Today.Year;
                if (model.EmpStatus == "Employee Ogura Clutch")
                {
                    model.leave_start_date = new DateTime(year-1, 11, 21);
                    model.leave_end_date = new DateTime(year, 11, 20);
                }
                else
                {
                    model.leave_start_date = new DateTime(year, 1, 1);
                    model.leave_end_date = new DateTime(year, 12, 31);
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
        // /EmployeeMaster/GetSelectOption
        public EmployeeModel GetSelectOption(EmployeeModel model)
        {
            string province = model.Addr_Prvnc;
            string district = model.Addr_Dtrct;
            string subdistrict = model.Addr_Prsh;
            //GetCode
            string postcode = model.Addr_Post;
            

            //Select Gender
            List<SelectListItem> selectGender = new List<SelectListItem>();
            selectGender.Add(new SelectListItem() { Value = "Male", Text = " Male " });
            selectGender.Add(new SelectListItem() { Value = "Female", Text = " Female " });
            //Select Title En
            List<SelectListItem> selectTitleEn = new List<SelectListItem>();
            selectTitleEn.Add(new SelectListItem() { Value = "Mr.", Text = " Mr. " });
            selectTitleEn.Add(new SelectListItem() { Value = "Mrs.", Text = " Mrs. " });
            selectTitleEn.Add(new SelectListItem() { Value = "Ms.", Text = " Ms. " });
            //Select Title Th
            List<SelectListItem> selectTitleTh = new List<SelectListItem>();
            selectTitleTh.Add(new SelectListItem() { Value = "นาย", Text = " นาย " });
            selectTitleTh.Add(new SelectListItem() { Value = "นาง", Text = " นาง " });
            selectTitleTh.Add(new SelectListItem() { Value = "น.ส.", Text = " น.ส. " });
            //Select Province
            List<SelectListItem> selectProvince = db.Emp_MasterRegion.Select(s => s.province).Distinct()
                .OrderBy(o=>o)
                .Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            //Select District
            var query_district = !String.IsNullOrEmpty(province) ? db.Emp_MasterRegion.Where(x => x.province == province)
                : db.Emp_MasterRegion.Take(0);
            List<SelectListItem> selectDistrict = query_district.Select(s => s.district).Distinct()
                .OrderBy(o => o)
                .Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            //Select Sub-District
            var query_subdistrict = !String.IsNullOrEmpty(district) ? db.Emp_MasterRegion.Where(x => x.district == district)
                : db.Emp_MasterRegion.Take(0);
            List<SelectListItem> selectSubDistrict = query_subdistrict.Select(s => s.parish).Distinct()
                .OrderBy(o => o)
                .Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
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
                    Value = s.CostCenterName,
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

            model.SelectGender = selectGender;
            model.SelectTitleEn = selectTitleEn;
            model.SelectTitleTh = selectTitleTh;
            model.SelectProvince = selectProvince;
            model.SelectDistrict = selectDistrict;
            model.SelectSubDistrict = selectSubDistrict;
            model.SelectEducation = selectEducation;
            model.SelectDepartment = selectDeptCode;
            model.SelectPosition = selectPosition;
            model.SelectCostCenter = selectCostCenter;
            model.SelectEmployeeStatus = selectEmpStatus;
            model.SelectNationality = selectNationality;
  
            return model;
        }

        //
        // POST: /EmployeeMaster/EmpDetail
        [HttpPost]
        [CustomAuthorize(32)]//Employee list setup
        public ActionResult EmpDetail(HttpPostedFileBase imgfile,HttpPostedFileBase jobfile
            ,FormCollection form, EmployeeModel model)
        {
            try
            {
                //Get Select Options
                GetSelectOption(model);

                var supportedTypes = new[] { "png" };
                var supportedTypes_pdf = new[] { "pdf" };
                if(!ModelState.IsValid)
                {
                    return View(model);
                }
                if (form["SaveJobFile"] != null)
                {
                    if (jobfile != null)
                    {
                        string _fn = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf");
                        var fileExtPdf = System.IO.Path.GetExtension(jobfile.FileName).Substring(1);
                        string _pathPdf = System.IO.Path.Combine(path_job, _fn);
                        if (!supportedTypes_pdf.Contains(fileExtPdf))
                        {
                            ViewBag.ErrorMessage = "Invalid file extension, Only .PDF file.";
                            return View(model);
                        }
                        jobfile.SaveAs(_pathPdf);
                        ViewBag.Message = String.Format("Uploaded file successfully, {0} . ", _fn);
                        return View(model);
                    }
                    ViewBag.ErrorMessage = "No file.";
                    return View(model);
                }
                var emp = db.EmpLists.Where(x => x.EmpID == model.EmpID).FirstOrDefault();
                if (model.rights_see_IDcode_PassportID_TaxID_SSO)
                {
                    emp.IDcode = model.IDcode;
                    emp.PPID = model.PPID;
                    emp.TaxID = model.TaxID;
                    emp.SSO = model.SSO;
                }                
                emp.Title_TH = model.Title_TH;
                emp.FName_TH = model.FName_TH;
                emp.LName_TH = model.LName_TH;
                emp.NName_TH = model.NName_TH;
                emp.Title_EN = model.Title_EN;
                emp.FName_EN = model.FName_EN;
                emp.LName_EN = model.LName_EN;
                emp.NName_EN = model.NName_EN;
                emp.Gender = model.Gender;
                emp.Nation = model.Nation;
                if(model.rights_see_birth_date){
                    emp.BDate = model.BDate;
                }            
                emp.Disabled_stat = model.Disabled_stat;
                emp.Disabled_By = model.Disabled_By;
                emp.Addr_Build = model.Addr_Build;
                emp.Addr_No = model.Addr_No;
                emp.Addr_Alle = model.Addr_Alle;
                emp.Addr_Rd = model.Addr_Rd;
                emp.Addr_Vill = model.Addr_Vill;
                emp.Addr_Prsh = model.Addr_Prsh;
                emp.Addr_Dtrct = model.Addr_Dtrct;
                emp.Addr_Prvnc = model.Addr_Prvnc;
                emp.Addr_Post = model.Addr_Post;
                emp.Educate = model.Educate;
                emp.In_Email = model.In_Email;
                emp.Ex_Email = model.Ex_Email;
                emp.Mobile = model.Mobile;
                emp.Tel = model.Tel;
                emp.Bank_Acc = model.Bank_Acc;
                if(!String.IsNullOrEmpty(model.DeptDesc))
                {
                    var deptcode = db.emp_department.Where(x=>!String.IsNullOrEmpty(x.DeptName)
                        && x.DeptName == model.DeptDesc)
                        .Select(s=>s.DeptCode).FirstOrDefault();
                    emp.DeptCode = deptcode;
                    emp.DeptDesc = model.DeptDesc;
                }
                emp.Position = model.Position;
                if(!String.IsNullOrEmpty(model.CostCenterName)){
                    var costcenter = db.Emp_CostCenter.Where(x=>!String.IsNullOrEmpty(x.CostCenterName)
                        && x.CostCenterName == model.CostCenterName)
                        .Select(s=>s.CostCenter).FirstOrDefault();
                    emp.CostCenter = costcenter;
                    emp.CostCenterName = model.CostCenterName;
                }

                emp.StartDate = model.StartDate;
                emp.PassDate = model.PassDate;
                emp.StartPosition = model.StartPosition;
                emp.Note = model.Note;
                emp.EmpStatus = model.EmpStatus;
                emp.ReasonPPS = model.ReasonPPS;
                //PVD
                emp.StartDatePVD1 = model.pvd1_start_date;
                emp.EndDatePVD1 =  model.pvd1_end_date;
                emp.StartDatePVD2 = model.pvd2_start_date;
                emp.EndDatePVD2 = model.pvd2_end_date;

                if(model.DatePasSport.HasValue){
                    emp.DatePasSport = model.DatePasSport;
                    DateTime date_expired = model.DatePasSport.Value;
                    date_expired = date_expired.AddMonths(3);
                    emp.DatePasSportExpire = date_expired.ToString("d MMMMM yyyy");
                }
               
                if(imgfile != null)
                {
                    string _Filename = String.Concat(model.EmpID.ToString(), System.IO.Path.GetExtension(imgfile.FileName));
                    string _path = System.IO.Path.Combine(path_pic,_Filename);
                    var fileExt = System.IO.Path.GetExtension(imgfile.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.ErrorMessage = "Invalid file extension, Only image .png file.";
                        return View(model);
                    }
                    imgfile.SaveAs(_path);
                }
                if (jobfile != null)
                {
                    string _fn = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf");
                    var fileExtPdf = System.IO.Path.GetExtension(jobfile.FileName).Substring(1);
                    string _pathPdf = System.IO.Path.Combine(path_job, _fn);
                    if (!supportedTypes_pdf.Contains(fileExtPdf))
                    {
                        ViewBag.ErrorMessage = "Invalid file extension, Only .PDF file.";
                        return View(model);
                    }
                    jobfile.SaveAs(_pathPdf);
                }
                //Save Edit
                db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                //Save log
                string user_nickname = null;
                if (Session["NickName"] != null)
                {
                    user_nickname = Session["NickName"].ToString();
                }
                string log_detail = String.Concat(emp.EmpID
                                    , "/", emp.FName_EN
                                    , " ", emp.LName_EN
                                    , "/", emp.DeptDesc);
                log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                Log logmodel = new Log()
                {
                    Log_Action = "edit",
                    Log_Type = "Employee List",
                    Log_System = "Administration",
                    Log_Detail = log_detail,
                    Log_Action_Id = emp.EmpID,
                    Log_Date = DateTime.Now,
                    Log_by = user_nickname
                };
                db.Logs.Add(logmodel);
                db.SaveChanges();
               

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

                TempData["shortMessage"] = String.Format("Edited successfully, Employee ID: {0} . ", model.EmpID.ToString());
                return RedirectToAction("EmpDetail", new { id = model.EmpID });

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        //
        //GET: /EmployeeMaster/Create
        [CustomAuthorize(32)]//Employee list setup
        public ActionResult Create()
        {
            try
            {
                EmployeeModel model = new EmployeeModel();
                //Get Select Options
                GetSelectOption(model);
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
        //POST: /EmployeeMaster/Create
        [HttpPost]
        [CustomAuthorize(32)]//Employee list setup
        public ActionResult Create(HttpPostedFileBase imgfile,HttpPostedFileBase jobfile
            , FormCollection form, EmployeeModel model)
        {
            try
            {
                //Get Select Options
                GetSelectOption(model);

                var supportedTypes = new[] { "png" };
                var supportedTypes_pdf = new[] { "pdf" };
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if(model.EmpID <= 0)
                {
                    ViewBag.ErrorMessage = "Invalid Employee ID, Only positive number.";
                    return View(model);
                }
                else
                {   //Emp Id is used
                    var check_empid = db.EmpLists.Where(x => x.EmpID == model.EmpID).FirstOrDefault();
                    var check_empformer = db.Former_EmpList.Where(x => x.EmpID == model.EmpID).FirstOrDefault();
                    if (check_empid != null)
                    {
                        ViewBag.ErrorMessage = String.Format("Duplicate Employee ID, {0} is used by {1} {2}. "
                            , check_empid.EmpID.ToString(), check_empid.FName_EN, check_empid.LName_EN);
                        return View(model);
                    }
                    if (check_empformer != null)
                    {
                        ViewBag.ErrorMessage = String.Format("Duplicate Former Employee ID, {0} is used by {1} {2}. "
                            , check_empformer.EmpID.ToString(), check_empformer.FName_EN, check_empformer.LName_EN);
                        return View(model);
                    }
                }
                var emp = new EmpList();
                emp.EmpID = model.EmpID;
                emp.IDcode = model.IDcode;
                emp.PPID = model.PPID;
                emp.TaxID = model.TaxID;
                emp.SSO = model.SSO;
                emp.Title_TH = model.Title_TH;
                emp.FName_TH = model.FName_TH;
                emp.LName_TH = model.LName_TH;
                emp.NName_TH = model.NName_TH;
                emp.Title_EN = model.Title_EN;
                emp.FName_EN = model.FName_EN;
                emp.LName_EN = model.LName_EN;
                emp.NName_EN = model.NName_EN;
                emp.Gender = model.Gender;
                emp.Nation = model.Nation;
                emp.BDate = model.BDate;
                emp.Disabled_stat = model.Disabled_stat;
                emp.Disabled_By = model.Disabled_By;
                emp.Addr_Build = model.Addr_Build;
                emp.Addr_No = model.Addr_No;
                emp.Addr_Alle = model.Addr_Alle;
                emp.Addr_Rd = model.Addr_Rd;
                emp.Addr_Vill = model.Addr_Vill;
                emp.Addr_Prsh = model.Addr_Prsh;
                emp.Addr_Dtrct = model.Addr_Dtrct;
                emp.Addr_Prvnc = model.Addr_Prvnc;
                emp.Addr_Post = model.Addr_Post;
                emp.Educate = model.Educate;
                emp.In_Email = model.In_Email;
                emp.Ex_Email = model.Ex_Email;
                emp.Mobile = model.Mobile;
                emp.Tel = model.Tel;
                emp.Bank_Acc = model.Bank_Acc;
                if (!String.IsNullOrEmpty(model.DeptDesc))
                {
                    var deptcode = db.emp_department.Where(x => !String.IsNullOrEmpty(x.DeptName)
                        && x.DeptName == model.DeptDesc)
                        .Select(s => s.DeptCode).FirstOrDefault();
                    emp.DeptCode = deptcode;
                    emp.DeptDesc = model.DeptDesc;
                }
                emp.Position = model.Position;
                if (!String.IsNullOrEmpty(model.CostCenterName))
                {
                    var costcenter = db.Emp_CostCenter.Where(x => !String.IsNullOrEmpty(x.CostCenterName)
                        && x.CostCenterName == model.CostCenterName)
                        .Select(s => s.CostCenter).FirstOrDefault();
                    emp.CostCenter = costcenter;
                    emp.CostCenterName = model.CostCenterName;
                }

                emp.StartDate = model.StartDate;
                emp.PassDate = model.PassDate;
                emp.StartPosition = model.StartPosition;
                emp.Note = model.Note;
                emp.EmpStatus = model.EmpStatus;
                emp.ReasonPPS = model.ReasonPPS;
                if (model.DatePasSport.HasValue)
                {
                    emp.DatePasSport = model.DatePasSport;
                    DateTime date_expired = model.DatePasSport.Value;
                    date_expired = date_expired.AddMonths(3);
                    emp.DatePasSportExpire = date_expired.ToString("d MMMMM yyyy");
                }
                //Save Povider fund
                emp.StartDatePVD1 = model.pvd1_start_date;
                emp.EndDatePVD1 = model.pvd1_end_date;
                emp.StartDatePVD2 = model.pvd2_start_date;
                emp.EndDatePVD2 = model.pvd2_end_date;

                //Save image file
                if (imgfile != null)
                {
                    string _Filename = String.Concat(model.EmpID.ToString(), System.IO.Path.GetExtension(imgfile.FileName));
                    string _path = System.IO.Path.Combine(path_pic, _Filename);
                    var fileExt = System.IO.Path.GetExtension(imgfile.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.ErrorMessage = "Invalid file extension, Only image .png file.";
                        return View(model);
                    }
                    imgfile.SaveAs(_path);
                }
                //Save job description file
                if (jobfile != null)
                {
                    string _fn = String.Concat(model.EmpID.ToString(), "_jobdescription.pdf");
                    var fileExtPdf = System.IO.Path.GetExtension(jobfile.FileName).Substring(1);
                    string _pathPdf = System.IO.Path.Combine(path_job, _fn);
                    if (!supportedTypes_pdf.Contains(fileExtPdf))
                    {
                        ViewBag.ErrorMessage = "Invalid file extension, Only .PDF file.";
                        return View(model);
                    }
                    jobfile.SaveAs(_pathPdf);
                }

                var result = db.EmpLists.Add(emp);
                //Save log
                if (result != null)
                {
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat(result.EmpID
                                        , "/", result.FName_EN
                                        , " ", result.LName_EN
                                        , "/", result.DeptDesc);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Employee List",
                        Log_System = "Administration",
                        Log_Detail = log_detail,
                        Log_Action_Id = result.EmpID,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                }
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Created successfully, Employee ID: {0} . ", result.EmpID.ToString());
                return RedirectToAction("EmpDetail", new {id = result.EmpID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        // GET: /EmployeeMaster/EmpDetail
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult EmpCourse(int id)
        {
            try
            {
                EmployeeModel model = new EmployeeModel();
                model.id = id;
                
                return View(model);
            }
            catch(Exception ex){
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        // GET: /EmployeeMaster/GetEmpCourse
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

        //
        //GET: /EmployeeMaster/Layoff
        [CustomAuthorize(52)]//To layoff employees
        public ActionResult Layoff(int id)
        {
            try
            {
                LayoffModel model = new LayoffModel();
                //Select type layoff
                List<SelectListItem> select_typelayoff = db.Emp_MasterTypeLayoff
                    .Select(s => new SelectListItem { Value = s.TypeLayoff, Text = s.TypeLayoff }).ToList();
                //Select reason layoff
                List<SelectListItem> select_reasonlayoff = new List<SelectListItem>();
                select_reasonlayoff.Add(new SelectListItem { Value = "Resign", Text = "Resign" });
                select_reasonlayoff.Add(new SelectListItem { Value = "Layoff", Text = "Layoff" });
                  
                model.id = id;
                model.SelectTypeLayoff = select_typelayoff;
                model.SelectReasonLayoff = select_reasonlayoff;

                return View(model);
            }
            catch(Exception ex){
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        //
        //POST: /EmployeeMaster/Layoff
        [HttpPost]
        [CustomAuthorize(52)]//To layoff employees
        public ActionResult Layoff(LayoffModel model)
        {
            try
            {
                //Select Tyoe layoff
                List<SelectListItem> select_typelayoff = db.Emp_MasterTypeLayoff
                    .Select(s => new SelectListItem { Value = s.TypeLayoff, Text = s.TypeLayoff }).ToList();
                //Select reason layoff
                List<SelectListItem> select_reasonlayoff = new List<SelectListItem>();
                select_reasonlayoff.Add(new SelectListItem { Value = "Resign", Text = "Resign" });
                select_reasonlayoff.Add(new SelectListItem { Value = "Layoff", Text = "Layoff" });

                model.SelectTypeLayoff = select_typelayoff;
                model.SelectReasonLayoff = select_reasonlayoff;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                EmpList emp = db.EmpLists.Where(x => x.EmpID == model.id).FirstOrDefault();
                Former_EmpList emp_out = new Former_EmpList();
                		emp_out.EmpID = emp.EmpID;
                        emp_out.IDcode = emp.IDcode;
                        emp_out.PPID = emp.PPID;
                        emp_out.TaxID = emp.TaxID;
                        emp_out.SSO = emp.SSO;
                        emp_out.Title_TH = emp.Title_TH;
                        emp_out.FName_TH = emp.FName_TH;
                        emp_out.LName_TH = emp.LName_TH;
                        emp_out.NName_TH = emp.NName_TH;
                        emp_out.Title_EN = emp.Title_EN;
                        emp_out.FName_EN = emp.FName_EN;
                        emp_out.LName_EN = emp.LName_EN;
                        emp_out.NName_EN = emp.NName_EN;
                        emp_out.Gender = emp.Gender;
                        emp_out.Nation = emp.Nation;
                        emp_out.BDate = emp.BDate;
                        emp_out.Disabled_stat = emp.Disabled_stat;
                        emp_out.Disabled_By = emp.Disabled_By;
                        emp_out.Addr_Build = emp.Addr_Build;
                        emp_out.Addr_No = emp.Addr_No;
                        emp_out.Addr_Alle = emp.Addr_Alle;
                        emp_out.Addr_Rd = emp.Addr_Rd;
                        emp_out.Addr_Vill = emp.Addr_Vill;
                        emp_out.Addr_Prsh = emp.Addr_Prsh;
                        emp_out.Addr_Dtrct = emp.Addr_Dtrct;
                        emp_out.Addr_Prvnc = emp.Addr_Prvnc;
                        emp_out.Addr_Post = emp.Addr_Post;
                        emp_out.Educate = emp.Educate;
                        emp_out.In_Email = emp.In_Email;
                        emp_out.Ex_Email = emp.Ex_Email;
                        emp_out.Mobile = emp.Mobile;
                        emp_out.Tel = emp.Tel;
                        emp_out.Bank_Acc = emp.Bank_Acc;
                        emp_out.DeptCode = emp.DeptCode;
                        emp_out.DeptDesc = emp.DeptDesc;
                        emp_out.Position = emp.Position;
                        emp_out.CostCenter = emp.CostCenter;
                        emp_out.CostCenterName = emp.CostCenterName;
                        emp_out.StartDate = emp.StartDate;
                        emp_out.PassDate = emp.PassDate;
                        emp_out.StartPosition = emp.StartPosition;
                        emp_out.LayoffDate = model.layoff_date;
                        emp_out.LayoffReas = model.reasonlayoff;
                        emp_out.LayoffDetail = String.Concat(model.typelayoff," ",model.other_type);
                        emp_out.Note = emp.Note;
                        emp_out.EmpStatus = emp.EmpStatus;
                        emp_out.ReasonPPS = emp.ReasonPPS;
                        emp_out.DatePasSport = emp.DatePasSport;
                        emp_out.DatePasSportExpire = emp.DatePasSportExpire;

                //Add
                db.Former_EmpList.Add(emp_out);
                //Remove
                db.EmpLists.Remove(emp);
                //Save log
                string user_nickname = null;
                if (Session["NickName"] != null)
                {
                    user_nickname = Session["NickName"].ToString();
                }
                string log_detail = String.Concat(emp.EmpID
                                    , "/", emp.FName_EN
                                    , " ", emp.LName_EN
                                    , "/", emp.DeptDesc);
                log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                Log logmodel = new Log()
                {
                    Log_Action = "delete",
                    Log_Type = "Employee List",
                    Log_System = "Administration",
                    Log_Detail = log_detail,
                    Log_Action_Id = emp.EmpID,
                    Log_Date = DateTime.Now,
                    Log_by = user_nickname
                };
                db.Logs.Add(logmodel);
                //Save all
                db.SaveChanges();
                if (emp != null)
                {
                    SendEmailLayoff(emp.EmpID,emp.FName_EN,emp.LName_EN,emp.DeptDesc,emp.Position,emp.CostCenterName);
                }

                TempData["shortMessage"] = String.Format("Layoff successfully, Employee ID: {0} . ", model.id.ToString());
                return RedirectToAction("EmpList");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        // /Employee/SendEmailLayoff
        public ActionResult SendEmailLayoff(int emp_id, string fname, string lname, string dept, string position, string costcenter)
        {
            try
            {
                SendMailCenterModel model = new SendMailCenterModel();
                //Send mail 
                int idgroup = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_REMOVE_ACCOUNT").Select(s => s.USG_Id).FirstOrDefault();
                var userlist = db.GroupMembers.Where(x => x.USG_Id == idgroup).Select(x => x.USE_Id).ToList();
                var tomail = db.UserDetails.Where(x => userlist.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();
                if (tomail == null || !tomail.Any())
                {
                    TempData["shortMessage"] = String.Format("Send email failed, User group name 'SEND_MAIL_REMOVE_ACCOUNT' is null. ");
                    return RedirectToAction("Layoff");
                }
                //Send mail CC
                int idgroup_cc = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_REMOVE_ACCOUNT_CC").Select(s => s.USG_Id).FirstOrDefault();
                var userlist_cc = db.GroupMembers.Where(x => x.USG_Id == idgroup_cc).Select(x => x.USE_Id).ToList();
                var tomail_cc = db.UserDetails.Where(x => userlist_cc.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();


                //Send e-mail
                //WorkingStandardSearch/DownloadFile?fileName=114000OCT-IT-FM018.pdf
                var callbackUrl = Url.Action("DownloadFile", "WorkingStandardSearch", new { fileName = "114000OCT-IT-FM018.pdf" }, protocol: Request.Url.Scheme);

                string html = "<h3 style='font-size: 20px'> Dear HR Department, </h3>"
                             + "<p style='font-size: 20px'>เนื่องจาก มีพนักงานลาออก ขอให้ทางหัวหน้าแผนก (Advisor / Head department) ,ทำเอกสารแจ้ง ขอยกเลิกบัญชีผู้ใช้ที่ลาออกแล้ว ส่งให้ทางแผนก IT ดำเนินการด้วยค่ะ</p>"
                             + "<p style='font-size: 16px'> **Form remove account (เอกสารสำหรับยกเลิกบัญชี)  "
                             + " OCT-IT-FM018 REMOVE USER ACCOUNT (All Account login) <a href=\"" + callbackUrl + "\"> OCT-IT-FM018 </a> </p>"
                             + "<p style='font-size: 16px'>Or click this link : "
                             + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"

                             + "<table style='font-size:18px'>"
                             + "<tr><td>Employee Detail </td><td></td></tr>"
                             + "<tr><td>Emp ID : </td><td>" + emp_id + "</td></tr>"
                             + "<tr><td>Name : </td><td>" + fname + " " + lname + "</td></tr>"
                             + "<tr><td>Department : </td><td>" + dept + "</td></tr>"
                             + "<tr><td>Position : </td><td>" + position + "</td></tr>"
                             + "<tr><td>Cost Center : </td><td>" + costcenter + "</td></tr></table><br/><br/>"

                             + "<p style='font-size: 16px'>*Do Not Reply This E-Mail!</p><br/><br/><br/><br/>"
                             + "<p style='font-size: 16px'>Best Regards,<br/>"
                             + "HR department</p>";

                model.To = tomail;
                model.Tocc = tomail_cc; ;
                model.Subject = "Remove Account";
                model.Body = html;
                SendMailCenterController.SendMail(model);
                TempData["shortMessage"] = String.Format("Send email successfully, To {0} ", String.Join(", ", tomail));

                return RedirectToAction("Layoff");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

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


        //  /EmployeeMaster/ExportToCsv
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
                    idnumer = v.IDcode,
                    passport_id = v.PPID,
                    tax_number = v.TaxID,
                    sso = v.SSO,
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
                    birth_date = v.BDate,
                    disabled_status = v.Disabled_stat.ToString(),
                    disabled_by = v.Disabled_By,
                    building = "\"" + v.Addr_Build + "\"" ,
                    house_no = "\"" + v.Addr_No + "\"",
                    alley = "\"" + v.Addr_Alle + "\"",
                    road = "\"" + v.Addr_Rd + "\"",
                    village_no = "\"" + v.Addr_Vill + "\"",
                    parish = "\"" + v.Addr_Prsh + "\"",
                    district = "\"" + v.Addr_Dtrct + "\"",
                    province = "\"" + v.Addr_Prvnc + "\"",
                    postal_code = "\"" + v.Addr_Post + "\"",
                    education = v.Educate,
                    internal_email = v.In_Email,
                    external_email = v.Ex_Email,
                    mobile = v.Mobile,
                    back_account = "\"" + v.Bank_Acc + "\"",
                    dept_code = "\"" + v.DeptCode + "\"",
                    dept_name = "\"" + v.DeptDesc + "\"" ,
                    position = "\""+ v.Position + "\"",
                    cost_center = v.CostCenter,
                    cost_center_name = "\"" + v.CostCenterName + "\"" ,
                    start_date = v.StartDate,
                    pass_date = v.PassDate,
                    employee_status = v.EmpStatus,
                    note = "\"" + v.Note + "\""  

                });

                sb.AppendFormat(
                    "{0},{1},{2},{3},{4}"
                    +",{5},{6},{7},{8},{9}"
                    +",{10},{11},{12},{13},{14}"
                    +",{15},{16},{17},{18},{19}"
                    +",{20},{21},{22},{23},{24}"
                    +",{25},{26},{27},{28},{29}"
                    +",{30},{31},{32},{33},{34}"
                    +",{35},{36},{37},{38},{39}"
                    +",{40},{41}"
                    ,"EmpID", "ID Number", "Passport ID", "Tax Number",	"SSO"
                    ,"Title TH", "First name TH", "Last name TH",	"Nickname TH",  "Title EN"
                    ,"First name EN", "Last name EN", "Nickname EN", "Gender", "Nationallity"
                    ,"Birth date" ,"Disabled status",	"Disabled by",	"Buidling",	"House No"
                    ,"Alley", "Road", "Village No",	"Parish", "District"
                    ,"Province", "Postal code", "Education", "Internal Email", "External Email"
                    ,"Mobile Tel",	"Bank Account"	,"Dept code",	"Dept name", "Position"
                    ,"Cost center", "CostCenterName", "Start date",	"Pass date", "Employee Status"
                    ,"Note"
                    , Environment.NewLine);

                string birth_date_str = null;
                string start_date_str = null;
                string pass_date_str = null;
                foreach (var i in forexport)
                {
                    birth_date_str = i.birth_date.HasValue ? i.birth_date.Value.ToString("yyyy-MM-dd"): null;
                    start_date_str = i.start_date.HasValue ? i.start_date.Value.ToString("yyyy-MM-dd") : null;
                    pass_date_str = i.pass_date.HasValue ? i.pass_date.Value.ToString("yyyy-MM-dd") : null;
                    sb.AppendFormat(
                       "{0},{1},{2},{3},{4}"
                       + ",{5},{6},{7},{8},{9}"
                       + ",{10},{11},{12},{13},{14}"
                       + ",{15},{16},{17},{18},{19}"
                       + ",{20},{21},{22},{23},{24}"
                       + ",{25},{26},{27},{28},{29}"
                       + ",{30},{31},{32},{33},{34}"
                       + ",{35},{36},{37},{38},{39},{40},{41}"
                       , i.emp_id ,i.idnumer, i.passport_id,i.tax_number, i.sso
                       , i.title_th, i.fname_th, i.lname_th, i.nickname_th, i.title_en
                       , i.fname_en, i.lname_en, i.nick_en, i.gender, i.nationality
                       , birth_date_str, i.disabled_status, i.disabled_by, i.building, i.house_no
                       , i.alley, i.road, i.village_no, i.parish, i.district
                       , i.province, i.postal_code, i.education, i.internal_email, i.external_email
                       , i.mobile, i.back_account, i.dept_code, i.dept_name,i.position
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
                response.AddHeader("content-disposition", "attachment;filename=Employee_Data_Setup.CSV ");
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
        //GET: /EmployeeMaster/GetEmpSum
        public ActionResult GetEmpSum(int id)
        {
            string emp_type = id > 0 ? "Employee Ogura Clutch" : "Sub Contractor";

            var result = db.EmpLists.Where(x => x.EmpStatus.Trim().Contains(emp_type));
            var group = from re in result
                        group re by new { re.DeptDesc, re.Gender } into g
                        select new { department = g.Key.DeptDesc, gender = g.Key.Gender, total = g.Count() };

            var dept_list = group.Select(s=>s.department).Distinct();
            EmpSumListModel empSumList = new EmpSumListModel();
            List<EmpSumModel> model_list = new List<EmpSumModel>();
            foreach(var item in dept_list)
            {
                EmpSumModel model = new EmpSumModel();
                var group_dept = group.Where(x => x.department == item).ToList();
                model.department = item;

                foreach(var g in group_dept)
                {
                    if(g.gender == "Male"){
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
        //GET: /EmployeeMaster/GetTotalAll
        public JsonResult GetTotalAll(int id)
        {
            var result = db.EmpLists;
            var sum_male = result.Where(x => x.Gender == "Male").Count();
            var sum_female = result.Where(x => x.Gender == "Female").Count();
            var json_result = new { sum_male = sum_male, sum_female = sum_female };
            return Json(json_result, JsonRequestBehavior.AllowGet);
        }



        // GET: /EmployeeMaster/GetProvince
        public JsonResult GetProvince()
        {
            var province = db.Emp_MasterRegion.Select(s => s.province).Distinct()
                .OrderBy(o=>o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(province, JsonRequestBehavior.AllowGet);
        }
        // POST: /EmployeeMaster/GetDistrict
        public JsonResult GetDistrict(string province)
        {
            var district = db.Emp_MasterRegion.Where(w => w.province == province).Select(s=>s.district).Distinct()
                .OrderBy(o => o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(district, JsonRequestBehavior.AllowGet);
        }
        // POST: /EmployeeMaster/GetSubDistrict
        public JsonResult GetSubDistrict(string district)
        {
            var subdistrict = db.Emp_MasterRegion.Where(w => w.district == district).Select(s=>s.parish).Distinct()
                .OrderBy(o => o)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(subdistrict, JsonRequestBehavior.AllowGet);
        }
        // POST: /EmployeeMaster/GetPostCode
        [HttpPost]
        public JsonResult GetPostCode(string subdistrict, string district, string province)
        {
            var postcode = db.Emp_MasterRegion
                .Where(p => p.parish == subdistrict && p.district == district && p.province == province)
                .Select(c => c.code)
                .FirstOrDefault();

            return Json(postcode, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /EmployeeMaster/GetEmpId/
        [HttpPost]
        public JsonResult GetEmpId(int Prefix)
        {
            var empid = (from el in db.EmpLists
                         where el.EmpID.ToString().StartsWith(Prefix.ToString())
                         select new { label = el.EmpID, val = el.EmpID }).Take(10).ToList();
            return Json(empid);
        }
        //
        // POST: /EmployeeMaster/GetFName/
        [HttpPost]
        public JsonResult GetFName(string Prefix)
        {
            var fname = (from el in db.EmpLists
                               where el.FName_EN.StartsWith(Prefix)
                               select new { label = el.FName_EN, val = el.FName_EN }).Take(10).ToList();
            return Json(fname);
        }
        //
        // POST: /EmployeeMaster/GetLName/
        [HttpPost]
        public JsonResult GetLName(string Prefix)
        {
            var lname = (from el in db.EmpLists
                         where el.LName_EN.StartsWith(Prefix)
                         select new { label = el.LName_EN, val = el.LName_EN }).Take(10).ToList();
            return Json(lname);
        }
        //
        // POST: /EmployeeMaster/GetNName/
        [HttpPost]
        public JsonResult GetNName(string Prefix)
        {
            var nname = (from el in db.EmpLists
                         where el.NName_EN.StartsWith(Prefix)
                         select new { label = el.NName_EN, val = el.NName_EN }).Take(10).ToList();
            return Json(nname);
        }
        //
        // POST: /EmployeeMaster/GetIDNumber/
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
