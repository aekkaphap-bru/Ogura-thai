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

namespace OCTWEB_NET45.Controllers.Leave
{
    [Authorize]
    public class LeaveReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_pic = ConfigurationManager.AppSettings["path_pic"];

        //
        // GET: /LeaveReport/LeaveList
        [CustomAuthorize(64)]//Leave Report
        public ActionResult LeaveList(SearchLeave model)
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
                if (!model.Page.HasValue)
                {
                    model.yearleave = DateTime.Today.Year.ToString();
                }
                int? emp_id = model.emp_id;
                string nickname = model.nickname;
                string fname = model.fname;
                string lname = model.lname;
                string deptcode = model.deptcode;
                string position = model.position;
                string costcenter_code = model.costcenter_code;
                int? typeleave_id = model.typeleave_id;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                //Show by department login
                string user_dep_id = null;
                if (Session["UserCode"] != null)
                {
                    int usercode = Convert.ToInt32(Session["UserCode"].ToString());
                    user_dep_id = db.EmpLists.Where(x => x.EmpID == usercode).Select(s => s.DeptCode).FirstOrDefault();
                    model.deptcode = user_dep_id;
                    deptcode = user_dep_id;
                }

                IEnumerable<Emp_Transection_Leave> query_leave = model.Page.HasValue ? db.Emp_Transection_Leave : db.Emp_Transection_Leave.Take(0);
                if (start_date.HasValue)
                {
                    DateTime sd = new DateTime(start_date.Value.Year, start_date.Value.Month, start_date.Value.Day, 0, 0, 0);
                    query_leave = query_leave.Where(x => x.Leave_startdate >= sd);
                }
                if (end_date.HasValue)
                {
                    DateTime ed = new DateTime(end_date.Value.Year, end_date.Value.Month, end_date.Value.Day, 23, 59, 59);
                    query_leave = query_leave.Where(x => x.Leave_startdate <= ed);
                }
                if (typeleave_id.HasValue)
                {
                    query_leave = query_leave.Where(x => x.Type_ReasonId == typeleave_id);
                }
                /*
                if (!String.IsNullOrEmpty(model.yearleave))
                {
                    DateTime yearleave_s = new DateTime(Convert.ToInt32(model.yearleave), 1, 1);
                    DateTime yearleave_e = new DateTime(Convert.ToInt32(model.yearleave), 12, 31);
                    query_leave = query_leave.Where(x => x.Leave_startdate >= yearleave_s && x.Leave_startdate <= yearleave_e);
                }*/
                List<EmployeeModel> query_emp = new List<EmployeeModel>();
                query_emp = db.EmpLists.Select(s => new EmployeeModel
                {
                    EmpID = s.EmpID,
                    Title_EN = s.Title_EN,
                    NName_EN = s.NName_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    Title_TH = s.Title_TH,
                    NName_TH = s.NName_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    DeptDesc = s.DeptDesc,
                    DeptCode = s.DeptCode,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                }).ToList();
                if (emp_id.HasValue)
                {
                    query_emp = query_emp.Where(x => x.EmpID == emp_id).ToList();
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.NName_EN)
                      && x.NName_EN.ToLowerInvariant().Contains(nickname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.FName_EN)
                       && x.FName_EN.ToLowerInvariant().Contains(fname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.LName_EN)
                        && x.LName_EN.ToLowerInvariant().Contains(lname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(deptcode))
                {
                    query_emp = query_emp.Where(x => x.DeptCode == deptcode).ToList();
                }
                if (!String.IsNullOrEmpty(position))
                {
                    query_emp = query_emp.Where(x => x.Position == position).ToList();
                }
                if (!String.IsNullOrEmpty(costcenter_code))
                {
                    query_emp = query_emp.Where(x => x.CostCenter == costcenter_code).ToList();
                }
                if (!String.IsNullOrEmpty(user_dep_id))
                {
                    query_emp = query_emp.Where(x => x.DeptCode == user_dep_id).ToList();
                }
                List<LeaveModel> emp_leave_list = query_leave.Join(query_emp, l => l.Emp_Id, e => e.EmpID, (l, e) => new { leave = l, emp = e })
                    .Select(s => new LeaveModel
                    {
                        id = s.leave.Id,
                        emp_id = s.leave.Emp_Id,
                        titlename = s.emp.Title_EN,
                        titlename_th = s.emp.Title_TH,
                        lname = s.emp.LName_EN,
                        lname_th = s.emp.LName_TH,
                        fname = s.emp.FName_EN,
                        fname_th = s.emp.FName_TH,
                        nickname = s.emp.NName_EN,
                        nickname_th = s.emp.NName_TH,
                        department = s.emp.DeptDesc,
                        department_code = s.emp.DeptCode,
                        position = s.emp.Position,
                        costcenter = s.emp.CostCenterName,
                        costcenter_code = s.emp.CostCenter,
                        start_date = s.leave.Leave_startdate,
                        end_date = s.leave.Leave_enddate,
                        totalday = s.leave.Date_leave,
                        shift = s.leave.Shift,
                        typeleave_id = s.leave.Type_ReasonId,
                        typeleave = (from tl in db.Emp_Typesick
                                     where tl.Id == s.leave.Type_ReasonId
                                     select tl.Type_Reason).FirstOrDefault(),
                        reason_detail = s.leave.Reasons_for_leave,
                        start_time = s.leave.Leave_time,
                        end_time = s.leave.Leave_timeend,
                        create_by = s.leave.AddbyEmp,
                        create_date = s.leave.DateSt,
                        approved = s.leave.Approved_emp,
                        reason_not_approved = s.leave.ReasonnotApEmp,
                        transection_date = s.leave.DT_UpEmpN,
                        status_leave_code = s.leave.StatusWork.HasValue ? s.leave.StatusWork.Value : 0,
                        status_leave = (from sl in db.Emp_MasterStatusLeave
                                        where sl.StatusLeaveCode == s.leave.StatusWork
                                        select sl.StatusLeave).FirstOrDefault(),
                    }).OrderBy(o => o.create_date).ToList();

                
                //Set LeaveList model
                model.LeaveList = emp_leave_list;
                //Set Grouping by typeleave
                /*   1  Business(ลากิจ)
                     2  Sick Cert ( ลาป่วย มีใบรับรองแพทย์ )
                     3  Vacation (ลาพักร้อน)
                     4  Other (อื่นๆ)
                     5  Sick ( ลาป่วย ไม่มีใบรับรองแพทย์ )
                     6  S Vacation (ลาพักร้อนฉุกเฉิน)
                     7  Special Business (ลากิจพิเศษ) */
                model.business = emp_leave_list.Where(x => x.typeleave_id == 1).Select(s => s.totalday).Sum();
                model.sick_cert = emp_leave_list.Where(x => x.typeleave_id == 2).Select(s => s.totalday).Sum();
                model.vacation = emp_leave_list.Where(x => x.typeleave_id == 3).Select(s => s.totalday).Sum();
                model.other = emp_leave_list.Where(x => x.typeleave_id == 4).Select(s => s.totalday).Sum();
                model.sick = emp_leave_list.Where(x => x.typeleave_id == 5).Select(s => s.totalday).Sum();
                model.s_vacation = emp_leave_list.Where(x => x.typeleave_id == 6).Select(s => s.totalday).Sum();
                model.special_business = emp_leave_list.Where(x => x.typeleave_id == 7).Select(s => s.totalday).Sum();
                model.total_days = emp_leave_list.Select(s => s.totalday).Sum();

                //Set Employee Detail
                if(emp_id.HasValue)
                {
                    var get_emp = db.EmpLists.Where(x => x.EmpID == emp_id.Value).FirstOrDefault();
                    if (get_emp != null)
                    {
                        model._emp_id = get_emp.EmpID;
                        model._name = String.Concat(get_emp.Title_EN, get_emp.FName_EN, " ", get_emp.LName_EN, " (", get_emp.NName_EN, ")");
                        model._nameTh = String.Concat(get_emp.Title_TH," ", get_emp.FName_TH, " ", get_emp.LName_TH, " (", get_emp.NName_TH, ")");
                        model._department = get_emp.DeptDesc;
                        model._position = get_emp.Position;
                        model._costname = get_emp.CostCenterName;
                    }
                    //Get image path
                    string imgPath = String.Concat(path_pic, emp_id.Value.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        model._pic = imgDataURL;
                    }
                }

                IPagedList<LeaveModel> emp_leave_pagedList = emp_leave_list.ToPagedList(pageIndex, pageSize);

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
                //Select Type Leave
                List<SelectListItem> selectTypeLeave = db.Emp_Typesick
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Type_Reason,
                    }).ToList();
                //Select Year Leave
                List<Emp_Transection_Leave> query_year = db.Emp_Transection_Leave.ToList();
                List<string> yearlist = query_year.Select(s => s.Leave_startdate.ToString("yyyy")).Distinct().ToList();
                List<SelectListItem> selectYear = yearlist
                    .Select(s => new SelectListItem
                    {
                        Value = s,
                        Text = s
                    }).ToList();

                model.SelectYearLeave = selectYear;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.SelectTypeLeave = selectTypeLeave;
                model.LeavePagedList = emp_leave_pagedList;
                model.total = emp_leave_list.Count();

                //Set Calendar
                //CalendarSet(model);
                FullCalendarSet(model);

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /LeaveReport/LeaveList {0}", ex.ToString());
                return View("Error");
            }
        }

        //Create Full Calendar
        public SearchLeave FullCalendarSet(SearchLeave model)
        {
            //Get holiday list
            var holiday_list = db.Emp_Holiday_sickleave
                .Select(s => new
                {
                    id = s.Id,
                    start_date = s.date_start,
                    end_date = s.date_end,
                    text = s.Detail,
                }).ToList();

            var emp_leave_list = model.LeaveList;
            var LeaveEvents = new List<EventViewModel>();
            //Set holidat
            foreach(var h in holiday_list.Select((value,i)=> new{i, value}))
            {
                if (h.value.start_date.HasValue && h.value.end_date.HasValue)
                {
                    var leave_event = new EventViewModel();

                    leave_event.id = h.i + 1;
                    leave_event.title = h.value.text;
                    leave_event.start = h.value.start_date.Value.ToString("yyyy-MM-dd");
                    leave_event.end = h.value.end_date.Value.AddDays(1).ToString("yyyy-MM-dd");
                    leave_event.allDay = true;
                    leave_event.color = "#F08080";
                    leave_event.textColor = "#0C090A";
                    leave_event.backgroundColor = "#F08080";
                    leave_event.borderColor = "#F08080";
                    leave_event.overlap = false;
                    leave_event.display = "background";
                    leave_event.classNames = new string[] {"bg-image-ho"};

                    LeaveEvents.Add(leave_event);
                }               
            }

            foreach (var l in emp_leave_list.Select((value, i) => new { i, value }))
            {
                var leave_event = new EventViewModel();

                leave_event.id = l.i + 1;
                leave_event.title = l.value.emp_id +"-"+ l.value.fname +" "+ l.value.lname+ " ("+ l.value.nickname + ")"+ "-"+ l.value.typeleave;
                leave_event.start = l.value.start_date.ToString("yyyy-MM-ddTHH:mm:ss");
                leave_event.end = l.value.end_date.ToString("yyyy-MM-ddTHH:mm:ss");
                leave_event.allDay = false;
                /*   1  Business(ลากิจ)
                    2  Sick Cert ( ลาป่วย มีใบรับรองแพทย์ )
                    3  Vacation (ลาพักร้อน)
                    4  Other (อื่นๆ)
                    5  Sick ( ลาป่วย ไม่มีใบรับรองแพทย์ )
                    6  S Vacation (ลาพักร้อนฉุกเฉิน)
                    7  Special Business (ลากิจพิเศษ) */
                if (l.value.typeleave_id == 1)//1  Business(ลากิจ)
                {
                    leave_event.backgroundColor = "#FF6600";
                    leave_event.borderColor = "#FF6600";
                    leave_event.textColor = "#000000";
                }
                else if (l.value.typeleave_id == 2)//2  Sick Cert ( ลาป่วย มีใบรับรองแพทย์ )
                {
                    leave_event.backgroundColor = "#FF66CC";
                    leave_event.borderColor = "#FF66CC";
                    leave_event.textColor = "#3A3B3C";
                }
                else if (l.value.typeleave_id == 3)//3  Vacation (ลาพักร้อน)
                {
                    leave_event.backgroundColor = "#00B050";
                    leave_event.borderColor = "#00B050";
                    leave_event.textColor = "#3A3B3C";
                }
                else if (l.value.typeleave_id == 4)//4  Other (อื่นๆ)
                {
                    leave_event.backgroundColor = "#0066FF";
                    leave_event.borderColor = "#0066FF";
                    leave_event.textColor = "#FFFFFF";
                }
                else if (l.value.typeleave_id == 5)//5  Sick ( ลาป่วย ไม่มีใบรับรองแพทย์ )
                {
                    leave_event.backgroundColor = "#FFCCFF";
                    leave_event.borderColor = "#FFCCFF";
                    leave_event.textColor = "#000000";
                }
                else if (l.value.typeleave_id == 6)//6 S Vacation (ลาพักร้อนฉุกเฉิน)
                {
                    leave_event.backgroundColor = "#CC0066";
                    leave_event.borderColor = "#CC0066";
                    leave_event.textColor = "#FFFFFF";
                }
                else if (l.value.typeleave_id == 7)//7  Special Business (ลากิจพิเศษ) 
                {
                    leave_event.backgroundColor = "#FF3300";
                    leave_event.borderColor = "#FF3300";
                    leave_event.textColor = "#FFFFFF";
                }
                
                LeaveEvents.Add(leave_event);
            }
            model.event_list = LeaveEvents;
            return model;
        }

        // Create Calendar
        public SearchLeave CalendarSet(SearchLeave model)
        {
            //--------------------------------------------------Calendar--------------------------------------------------------//
            DateTime? start_date = model.start_date;
            List<LeaveModel> emp_leave_list = model.LeaveList;

            //Set Calendar
            DateTime calendar_date = DateTime.Today;
            if (start_date.HasValue)
            {
                calendar_date = start_date.Value;
            }
            int year = calendar_date.Year;
            int month = calendar_date.Month;
            int num = DateTime.DaysInMonth(year, month);
            DateTime select_sd = new DateTime(year, month, 1);
            DateTime select_ed = new DateTime(year, month, num);
           
            //Create date in calendar list
            List<DateTime> date_list = Enumerable.Range(1, num)  // Days: 1, 2 ... 31 etc.
                .Select(day => new DateTime(year, month, day)) // Map each day to a date
                .ToList(); // Load dates into a list

            //Get holiday list
            var holiday_list = db.Emp_Holiday_sickleave.Where(x => x.date_start >= select_sd && x.date_start <= select_ed)
                .Select(s => new
                {
                    id = s.Id,
                    date = s.date_start,
                    text = s.Detail,
                }).ToList();
            //Select Leave between select startdate and end of month
            var first_month_calendar_list = emp_leave_list.Where(x => x.start_date <= select_ed).ToList();

            List<DateTime> newdate_list = new List<DateTime>();

            //Create New Date List 
            foreach (var i in first_month_calendar_list)
            {
                DateTime d1 = i.start_date;
                if (i.totalday > 1)
                {
                    int days = (int)Math.Ceiling(i.totalday); //round up
                    DateTime d2 = i.start_date;
                    for (var j = 2; j <= days; j++)
                    {
                        d2 = d2.AddDays(1);
                        foreach (var d in holiday_list)
                        {
                            if (d2.Date == d.date)
                            {
                                d2 = d2.AddDays(1);
                            }
                        }
                        if (d2.DayOfWeek == DayOfWeek.Saturday)
                        {
                            d2 = d2.AddDays(1);
                        }
                        if (d2.DayOfWeek == DayOfWeek.Sunday)
                        {
                            d2 = d2.AddDays(1);
                        }
                        newdate_list.Add(d2);
                    }
                }
                newdate_list.Add(d1);
            }
            //Create Calendar Model
            List<CalendarSet> calendar_list = new List<CalendarSet>();
            for (int i = 0; i < num; i++)
            {
                CalendarSet cal_model = new CalendarSet();
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Sunday)
                {
                    DateTime getdate = date_list[i];
                    cal_model.sunday_str = getdate.ToString("dd");
                    cal_model.sunday_holiday_str = holiday_list.Where(x => x.date == getdate).Select(s => s.text).FirstOrDefault();
                    cal_model.num_sun = newdate_list.Where(x => x.Date == getdate.Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Monday)
                {
                    cal_model.monday_str = date_list[i].ToString("dd");
                    cal_model.monday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_mon = newdate_list.Where(x => x.Date == date_list[i].Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Tuesday)
                {
                    cal_model.tuesday_str = date_list[i].ToString("dd");
                    cal_model.tuesday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_tues = newdate_list.Where(x => x.Date == date_list[i].Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Wednesday)
                {
                    cal_model.wednesday_str = date_list[i].ToString("dd");
                    cal_model.wednesday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_wednes = newdate_list.Where(x => x.Date == date_list[i].Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Thursday)
                {
                    cal_model.thursday_str = date_list[i].ToString("dd");
                    cal_model.thursday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_thurs = newdate_list.Where(x => x.Date == date_list[i].Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Friday)
                {
                    cal_model.friday_str = date_list[i].ToString("dd");
                    cal_model.friday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_fri = newdate_list.Where(x => x.Date == date_list[i].Date).Count();
                    i++;
                }
                if (i < num && date_list[i].DayOfWeek == DayOfWeek.Saturday)
                {
                    cal_model.saturday_str = date_list[i].ToString("dd");
                    cal_model.saturday_holiday_str = holiday_list.Where(x => x.date == date_list[i]).Select(s => s.text).FirstOrDefault();
                    cal_model.num_satur = newdate_list.Where(x => x.Date == date_list[i].Date).Count();

                }
                calendar_list.Add(cal_model);
            }
            model.calendar_head = calendar_date.ToString("MMMM yyyy");
            model.CalendarList = calendar_list;

            return model;
        }

        //
        // POST: /LeaveReport/LeaveList
        [HttpPost]
        [CustomAuthorize(64)]//Leave Report
        public ActionResult LeaveList(FormCollection form, SearchLeave model)
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
                int? typeleave_id = model.typeleave_id;
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                //Show by department login
                string user_dep_id = null;
                if (Session["UserCode"] != null)
                {
                    int usercode = Convert.ToInt32(Session["UserCode"].ToString());
                    user_dep_id = db.EmpLists.Where(x => x.EmpID == usercode).Select(s => s.DeptCode).FirstOrDefault();
                    model.deptcode = user_dep_id;
                    deptcode = user_dep_id;
                }

                IEnumerable<Emp_Transection_Leave> query_leave = db.Emp_Transection_Leave;
                if (start_date.HasValue)
                {
                    DateTime sd = new DateTime(start_date.Value.Year, start_date.Value.Month, start_date.Value.Day, 0, 0, 0);
                    query_leave = query_leave.Where(x => x.Leave_startdate >= sd);
                }
                if (end_date.HasValue)
                {
                    DateTime ed = new DateTime(end_date.Value.Year, end_date.Value.Month, end_date.Value.Day, 23, 59, 59);
                    query_leave = query_leave.Where(x => x.Leave_startdate <= ed);
                }
                if (typeleave_id.HasValue)
                {
                    query_leave = query_leave.Where(x => x.Type_ReasonId == typeleave_id);
                }
                /*
                if (!String.IsNullOrEmpty(model.yearleave))
                {
                    DateTime yearleave_s = new DateTime(Convert.ToInt32(model.yearleave), 1, 1);
                    DateTime yearleave_e = new DateTime(Convert.ToInt32(model.yearleave), 12, 31);
                    query_leave = query_leave.Where(x => x.Leave_startdate >= yearleave_s && x.Leave_startdate <= yearleave_e);
                }*/
                List<EmployeeModel> query_emp = new List<EmployeeModel>();
                query_emp = db.EmpLists.Select(s => new EmployeeModel
                {
                    EmpID = s.EmpID,
                    Title_EN = s.Title_EN,
                    NName_EN = s.NName_EN,
                    FName_EN = s.FName_EN,
                    LName_EN = s.LName_EN,
                    Title_TH = s.Title_TH,
                    NName_TH = s.NName_TH,
                    FName_TH = s.FName_TH,
                    LName_TH = s.LName_TH,
                    DeptDesc = s.DeptDesc,
                    DeptCode = s.DeptCode,
                    Position = s.Position,
                    CostCenter = s.CostCenter,
                    CostCenterName = s.CostCenterName,
                }).ToList();
                if (emp_id.HasValue)
                {
                    query_emp = query_emp.Where(x => x.EmpID == emp_id).ToList();
                }
                if (!String.IsNullOrEmpty(nickname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.NName_EN)
                      && x.NName_EN.ToLowerInvariant().Contains(nickname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(fname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.FName_EN)
                       && x.FName_EN.ToLowerInvariant().Contains(fname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(lname))
                {
                    query_emp = query_emp.Where(x => !String.IsNullOrEmpty(x.LName_EN)
                        && x.LName_EN.ToLowerInvariant().Contains(lname.ToLowerInvariant())).ToList();
                }
                if (!String.IsNullOrEmpty(deptcode))
                {
                    query_emp = query_emp.Where(x => x.DeptCode == deptcode).ToList();
                }
                if (!String.IsNullOrEmpty(position))
                {
                    query_emp = query_emp.Where(x => x.Position == position).ToList();
                }
                if (!String.IsNullOrEmpty(costcenter_code))
                {
                    query_emp = query_emp.Where(x => x.CostCenter == costcenter_code).ToList();
                }
                if (!String.IsNullOrEmpty(user_dep_id))
                {
                    query_emp = query_emp.Where(x => x.DeptCode == user_dep_id).ToList();
                }
                List<LeaveModel> emp_leave_list = query_leave.Join(query_emp, l => l.Emp_Id, e => e.EmpID, (l, e) => new { leave = l, emp = e })
                    .Select(s => new LeaveModel
                    {
                        id = s.leave.Id,
                        emp_id = s.leave.Emp_Id,
                        titlename = s.emp.Title_EN,
                        titlename_th = s.emp.Title_TH,
                        lname = s.emp.LName_EN,
                        lname_th =s.emp.LName_TH,
                        fname = s.emp.FName_EN,
                        fname_th = s.emp.FName_TH,
                        nickname = s.emp.NName_EN,
                        nickname_th = s.emp.NName_TH,
                        department = s.emp.DeptDesc,
                        department_code = s.emp.DeptCode,
                        position = s.emp.Position,
                        costcenter = s.emp.CostCenterName,
                        costcenter_code = s.emp.CostCenter,
                        start_date = s.leave.Leave_startdate,
                        end_date = s.leave.Leave_enddate,
                        totalday = s.leave.Date_leave,
                        shift = s.leave.Shift,
                        typeleave_id = s.leave.Type_ReasonId,
                        typeleave = (from tl in db.Emp_Typesick
                                     where tl.Id == s.leave.Type_ReasonId
                                     select tl.Type_Reason).FirstOrDefault(),
                        reason_detail = s.leave.Reasons_for_leave,
                        start_time = s.leave.Leave_time,
                        end_time = s.leave.Leave_timeend,
                        create_by = s.leave.AddbyEmp,
                        create_date = s.leave.DateSt,
                        approved = s.leave.Approved_emp,
                        reason_not_approved = s.leave.ReasonnotApEmp,
                        transection_date = s.leave.DT_UpEmpN,
                        status_leave_code = s.leave.StatusWork.HasValue ? s.leave.StatusWork.Value : 0,
                        status_leave = (from sl in db.Emp_MasterStatusLeave
                                        where sl.StatusLeaveCode == s.leave.StatusWork
                                        select sl.StatusLeave).FirstOrDefault(),
                    }).OrderBy(o => o.create_date).ToList();
                

                //Export Csv
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(emp_leave_list);
                }
                //Export Csv
                if (form["ExportToCsvSum"] == "ExportToCsvSum")
                {
                    string search_startdate = start_date.HasValue ? start_date.Value.ToString("yyyy-MM-dd") : "";
                    string search_enddate = end_date.HasValue ? end_date.Value.ToString("yyyy-MM-dd") : "";
                    ExportToCsvSum(emp_leave_list, search_startdate, search_enddate);
                }
                //Set LeaveList model
                model.LeaveList = emp_leave_list;
                //Set Grouping by typeleave
                /*   1  Business(ลากิจ)
                     2  Sick Cert ( ลาป่วย มีใบรับรองแพทย์ )
                     3  Vacation (ลาพักร้อน)
                     4  Other (อื่นๆ)
                     5  Sick ( ลาป่วย ไม่มีใบรับรองแพทย์ )
                     6  S Vacation (ลาพักร้อนฉุกเฉิน)
                     7  Special Business (ลากิจพิเศษ) */
                model.business = emp_leave_list.Where(x => x.typeleave_id == 1).Select(s => s.totalday).Sum();
                model.sick_cert = emp_leave_list.Where(x => x.typeleave_id == 2).Select(s => s.totalday).Sum();
                model.vacation = emp_leave_list.Where(x => x.typeleave_id == 3).Select(s => s.totalday).Sum();
                model.other = emp_leave_list.Where(x => x.typeleave_id == 4).Select(s => s.totalday).Sum();
                model.sick = emp_leave_list.Where(x => x.typeleave_id == 5).Select(s => s.totalday).Sum();
                model.s_vacation = emp_leave_list.Where(x => x.typeleave_id == 6).Select(s => s.totalday).Sum();
                model.special_business = emp_leave_list.Where(x => x.typeleave_id == 7).Select(s => s.totalday).Sum();
                model.total_days = emp_leave_list.Select(s => s.totalday).Sum();

                //Add Employee Picture
                /*foreach (var i in emp_leave_list)
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
                }*/
                //Set Employee Detail
                if (emp_id.HasValue)
                {
                    var get_emp = emp_leave_list.Where(x => x.emp_id == emp_id.Value).FirstOrDefault();
                    if (get_emp != null)
                    {
                        model._emp_id = get_emp.emp_id;
                        model._name = String.Concat(get_emp.titlename, get_emp.fname, " ", get_emp.lname, " (", get_emp.nickname, ")");
                        model._nameTh = String.Concat(get_emp.titlename_th, " ", get_emp.fname_th, " ", get_emp.lname_th, " (", get_emp.nickname_th, ")");
                        model._department = get_emp.department;
                        model._position = get_emp.position;
                        model._costname = get_emp.costcenter;
                    }
                    //Get image path
                    string imgPath = String.Concat(path_pic, emp_id.Value.ToString(), ".png");
                    //Check file exist
                    if (System.IO.File.Exists(imgPath))
                    {
                        //Convert image to byte array
                        byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
                        //Convert byte array to base64string
                        string imreBase64Data = Convert.ToBase64String(byteData);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        //Passing image data in model to view
                        model._pic = imgDataURL;
                    }
                }

                IPagedList<LeaveModel> emp_leave_pagedList = emp_leave_list.ToPagedList(pageIndex, pageSize);

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
                //Select Type Leave
                List<SelectListItem> selectTypeLeave = db.Emp_Typesick
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Type_Reason,
                    }).ToList();
                //Select Year Leave
                List<Emp_Transection_Leave> query_year = db.Emp_Transection_Leave.ToList();
                List<string> yearlist = query_year.Select(s => s.Leave_startdate.ToString("yyyy")).Distinct().ToList();
                List<SelectListItem> selectYear = yearlist
                    .Select(s => new SelectListItem
                    {
                        Value = s,
                        Text = s
                    }).ToList();

                model.SelectYearLeave = selectYear;
                model.SelectDepartment = selectDeptCode;
                model.SelectPosition = selectPosition;
                model.SelectCostCenter = selectCostCenter;
                model.SelectTypeLeave = selectTypeLeave;
                model.LeavePagedList = emp_leave_pagedList;
                model.total = emp_leave_list.Count();

                //Set Calendar
                //CalendarSet(model);
                FullCalendarSet(model);
                           
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /LeaveReport/LeaveList {0}", ex.ToString());
                return View("Error");
            }
        }
        
        //
        // GET: /LeaveReport/GetEmpDetail/
        public JsonResult GetEmpDetail(int emp_id)
        {

            var emp_detail = new Dictionary<string, string>();
            if (emp_id > 1000)
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
                        emp_detail.Add("pic", "");
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

        //Leave/ExportToCsv      
        public void ExportToCsv(List<LeaveModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    emp_id = v.emp_id,
                    titlename = v.titlename,
                    fname = v.fname,
                    lname = v.lname,
                    nickname = v.nickname,
                    department = "\"" + v.department + "\"",
                    department_code = v.department_code,
                    position = "\"" + v.position + "\"",
                    costcenter = "\"" + v.costcenter + "\"",
                    costcenter_code = v.costcenter_code,
                    start_date = v.start_date,
                    end_date = v.end_date,
                    totalday = v.totalday,
                    shift = v.shift,
                    typeleave_id = v.typeleave_id,
                    typeleave = v.typeleave,
                    reason_detail = "\"" + v.reason_detail + "\"",
                    start_time = v.start_time,
                    end_time = v.end_time,
                    create_by = v.create_by,
                    create_date = v.create_date,
                    approved = v.approved,
                    reason_not_approved = "\"" + v.reason_not_approved + "\"",
                    transection_date = v.transection_date,
                    status_leave_code = v.status_leave_code,
                    status_leave = v.status_leave,

                });



                sb.AppendFormat(
                    "{0},{1},{2},{3},{4}"
                    + ",{5},{6},{7},{8},{9}"
                    + ",{10},{11},{12},{13},{14}"
                    + ",{15},{16},{17},{18},{19},{20},{21},{22}"
                    , "No."
                    , "Leave Request Date(วันที่ยืนใบลา)"
                    , "DateStart(วันที่ลา)"
                    , "EndDate(ถึงวันที่)	"
                    , "Time Start(ช่วงเวลา)"
                    , "Date(จำนวนวันที่ลา)"
                    , "EmpId(รหัสพนักงาน)"
                    , "Title(คำนำหน้า)	"
                    , "Name(ชื่อ)"
                    , "LastName(นามสกุล)"
                    , "NickName(ชื่อเล่น)"
                    , "Department(แผนก)"
                    , "Position(ตำแหน่ง)"
                    , "CostName(Line ผลิต)	"
                    , "Reason leave(เหตุผลของการลา)"
                    , "Type leave(ประเภทของการลา)"
                    , "Shift(กะเช้า/กะดึก)"
                    , "Status Leave(สถานะการลา)"
                    , "Approved(อนุมัติ)	"
                    // ," Status HR Check(สถานะ HR ตรวจสอบ)"	
                    , "Modify Data(แก้ไขข้อมูล)"
                    , "Code Emp Update(รหัสพนักงาน)"
                    , "Reason not approved of Employee(เหตุผลที่ไม่ได้รับการอนุมัติจากหัวหน้างาน)"
                    , Environment.NewLine);

                foreach (var i in forexport)
                {
                    string transection_date = i.transection_date.HasValue ?
                        i.transection_date.Value.ToString("yyyy-MM-dd") : "";
                    sb.AppendFormat(
                       "{0},{1},{2},{3},{4}"
                       + ",{5},{6},{7},{8},{9}"
                       + ",{10},{11},{12},{13},{14}"
                       + ",{15},{16},{17},{18},{19},{20},{21},{22}"
                       , i.item
                       , i.create_date.ToString("yyyy-MM-dd")
                       , i.start_date.ToString("yyyy-MM-dd")
                       , i.end_date.ToString("yyyy-MM-dd")
                       , String.Concat(i.start_time, " - ", i.end_time)
                       , i.totalday
                       , i.emp_id
                       , i.titlename
                       , i.fname
                       , i.lname
                       , i.nickname
                       , i.department
                       , i.position
                       , i.costcenter
                       , i.reason_detail
                       , i.typeleave
                       , i.shift
                       , i.status_leave
                       , i.approved
                       , transection_date
                       , i.create_by
                       , i.reason_not_approved
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
                response.AddHeader("content-disposition", "attachment;filename=Employee_Leave.CSV ");
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

        //Leave/ExportToCsvSum      
        public void ExportToCsvSum(List<LeaveModel> model, string search_startdate, string search_enddate)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                List<int> emp_id_list = data.Select(s => s.emp_id.Value).ToList();

                List<ExportToCsvSumModel> forexport = db.EmpLists.Where(x => emp_id_list.Contains(x.EmpID))
                    .Select(v => new ExportToCsvSumModel
                    {
                        emp_id = v.EmpID,
                        titlename = v.Title_EN,
                        fname = v.FName_EN,
                        lname = v.LName_EN,
                        nickname = v.NName_EN,
                        department = "\"" + v.DeptDesc + "\"",
                        position = "\"" + v.Position + "\"",
                        costcenter = "\"" + v.CostCenterName + "\"",
                        business = 0.0,
                        sick_cert = 0.0,
                        vacation = 0.0,
                        other = 0.0,
                        sick = 0.0,
                        s_vacation = 0.0,
                        special_business = 0.0,

                    }).Distinct().ToList();

                foreach (var f in forexport)
                {
                    f.business = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 1).Select(s => s.totalday).Sum();
                    f.sick_cert = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 2).Select(s => s.totalday).Sum();
                    f.vacation = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 3).Select(s => s.totalday).Sum();
                    f.other = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 4).Select(s => s.totalday).Sum();
                    f.sick = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 5).Select(s => s.totalday).Sum();
                    f.s_vacation = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 6).Select(s => s.totalday).Sum();
                    f.special_business = model.Where(x => x.emp_id == f.emp_id && x.typeleave_id == 7).Select(s => s.totalday).Sum();
                }
                /*
                       1  Business(ลากิจ)
                       2  Sick Cert ( ลาป่วย มีใบรับรองแพทย์ )
                       3  Vacation (ลาพักร้อน)
                       4  Other (อื่นๆ)
                       5  Sick ( ลาป่วย ไม่มีใบรับรองแพทย์ )
                       6  S Vacation (ลาพักร้อนฉุกเฉิน)
                       7  Special Business (ลากิจพิเศษ) 
                 */
                sb.AppendFormat(
                    "{0},{1}"
                    , String.Concat("Employee Work Leave ", search_startdate, " - ", search_enddate)
                    , Environment.NewLine
                );
                sb.AppendFormat(
                    "{0},{1},{2},{3},{4}"
                    + ",{5},{6},{7},{8},{9}"
                    + ",{10},{11},{12},{13},{14}"
                    + ",{15},{16}"
                    , "No."
                    , "EmpId(รหัสพนักงาน)"
                    , "Title(คำนำหน้า)	"
                    , "Name(ชื่อ)"
                    , "LastName(นามสกุล)"
                    , "NickName(ชื่อเล่น)"
                    , "Department(แผนก)"
                    , "Position(ตำแหน่ง)"
                    , "CostName(Line ผลิต)"
                    , "Business(ลากิจ)"
                    , "Sick Cert (ลาป่วย มีใบรับรองแพทย์)"
                    , "Vacation (ลาพักร้อน)"
                    , "Other (อื่นๆ)"
                    , "Sick (ลาป่วย ไม่มีใบรับรองแพทย์)"
                    , "S Vacation (ลาพักร้อนฉุกเฉิน)"
                    , "Special Business (ลากิจพิเศษ)"
                    , Environment.NewLine);

                foreach (var item in forexport.Select((value, i) => new { i, value }))
                {
                    sb.AppendFormat(
                       "{0},{1},{2},{3},{4}"
                       + ",{5},{6},{7},{8},{9}"
                       + ",{10},{11},{12},{13},{14}"
                       + ",{15},{16}"
                       , item.i + 1
                       , item.value.emp_id
                       , item.value.titlename
                       , item.value.fname
                       , item.value.lname
                       , item.value.nickname
                       , item.value.department
                       , item.value.position
                       , item.value.costcenter
                       , item.value.business
                       , item.value.sick_cert
                       , item.value.vacation
                       , item.value.other
                       , item.value.sick
                       , item.value.s_vacation
                       , item.value.special_business
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
                response.AddHeader("content-disposition", "attachment;filename=Employee_Leave_Summary.CSV ");
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
        // POST: /LeaveReport/GetReasonLeave/
        [HttpPost]
        public JsonResult GetReasonLeave(int id)
        {
            var select_reason_leave = db.Emp_MasterReasonLeave.Where(x => x.Emp_Typesick == id)
                .Select(s => new { label = s.Reason, val = s.Reason }).ToList();
            return Json(select_reason_leave, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /LeaveReport/GetEmpId/
        [HttpPost]
        public JsonResult GetEmpId(int Prefix)
        {
            var empid = (from el in db.EmpLists
                         where el.EmpID.ToString().StartsWith(Prefix.ToString())
                         select new { label = el.EmpID, val = el.EmpID }).Take(10).ToList();
            return Json(empid);
        }
        //
        // POST: /LeaveReport/GetFName/
        [HttpPost]
        public JsonResult GetFName(string Prefix)
        {
            var fname = (from el in db.EmpLists
                         where el.FName_EN.StartsWith(Prefix)
                         select new { label = el.FName_EN, val = el.FName_EN }).Take(10).ToList();
            return Json(fname);
        }
        //
        // POST: /LeaveReport/GetLName/
        [HttpPost]
        public JsonResult GetLName(string Prefix)
        {
            var lname = (from el in db.EmpLists
                         where el.LName_EN.StartsWith(Prefix)
                         select new { label = el.LName_EN, val = el.LName_EN }).Take(10).ToList();
            return Json(lname);
        }
        //
        // POST: /LeaveReport/GetNName/
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