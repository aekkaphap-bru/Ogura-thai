using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class LeaveModel
    {
        private DateTime _SetDefaultDate = DateTime.Today;
        [Required]
        [Display(Name = "Emp ID")]
        public int? emp_id { get; set; }
        public string emp_profile { get; set; }
        public string titlename { get; set; }
        public string titlename_th { get; set; }
        public string nickname { get; set; }
        public string nickname_th { get; set; }
        public string fname { get; set; }
        public string fname_th { get; set; }
        public string lname { get; set; }
        public string lname_th { get; set; }
        public string department { get; set; }
        public string department_code { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public string costcenter_code { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string create_by { get; set; }
        public DateTime? transection_date { get; set; }
        public int id { get; set; }
        public int status_leave_code { get; set; }
        public string status_leave { get; set; }

        [Required]
        [Display(Name = "DATE")]
        public DateTime create_date
        {
            get
            {
                return _SetDefaultDate;
            }
            set
            {
                _SetDefaultDate = value;
            }
        }
        [Required]
        [Display(Name = "Start Date")]
        public DateTime start_date { get; set; }
        [Required]
        [Display(Name = "End Date")]
        public DateTime end_date { get; set; }
        [Required]
        [Display(Name = "shift")]
        public string shift { get; set; }

        [Required]
        [Display(Name = "Total Day")]
        [Range(0.06, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public double totalday { get; set; }
        [Required]
        [Display(Name = "Type of leave")]
        public int typeleave_id { get; set; }
        public string typeleave { get; set; }

        [Display(Name = "Reason for leave")]
        public string reasonleave { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Reason for leave")]
        public string reason_detail { get; set; }

        public string approved { get; set; }


        [MaxLength(100)]
        [Display(Name = "Reason Not Approved")]
        public string reason_not_approved { get; set; }
        public List<SelectListItem> SelectTypeLeave { get; set; }
        public List<SelectListItem> SelectReasonLeave { get; set; }

    }

    public class SearchLeave
    {
        public bool rights_62 { get; set; } //Use for give permission to add holiday 
        public bool rights_70 { get; set; } 
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int total { get; set; }
        public int? emp_id { get; set; }
        public string emp_profile { get; set; }
        public string nickname { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string deptcode { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public string costcenter_code { get; set; }
        public int? typeleave_id { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public IPagedList<LeaveModel> LeavePagedList { get; set; }
        public List<SelectListItem> SelectTypeLeave { get; set; }
        public List<SelectListItem> SelectDepartment { get; set; }
        public List<SelectListItem> SelectPosition { get; set; }
        public List<SelectListItem> SelectCostCenter { get; set; }
        public List<SelectListItem> SelectYearLeave { get; set; }
        public string yearleave { get; set; }
        public string calendar_head { get; set; }
        public List<CalendarSet> CalendarList { get; set; }
        public List<LeaveModel> LeaveList { get; set; }

        public double business { get; set; }
        public double sick_cert { get; set; }
        public double vacation { get; set; }
        public double other { get; set; }
        public double sick { get; set; }
        public double s_vacation { get; set; }
        public double special_business { get; set; }
        public double total_days { get; set; }

        public int? _emp_id { get; set; }
        public string _pic { get; set; }
        public string _name { get; set; }
        public string _nameTh { get; set; }
        public string _department { get; set; }
        public string _position { get; set; }
        public string _costname { get; set; }

        public List<EventViewModel> event_list { get; set; }


    }

    public class CalendarSet
    {
        public int day_num { get; set; }
        public string day_str { get; set; }
        public string month_str { get; set; }

        public string sunday_str { get; set; }
        public string monday_str { get; set; }
        public string tuesday_str { get; set; }
        public string wednesday_str { get; set; }
        public string thursday_str { get; set; }
        public string friday_str { get; set; }
        public string saturday_str { get; set; }

        public string sunday_holiday_str { get; set; }
        public string monday_holiday_str { get; set; }
        public string tuesday_holiday_str { get; set; }
        public string wednesday_holiday_str { get; set; }
        public string thursday_holiday_str { get; set; }
        public string friday_holiday_str { get; set; }
        public string saturday_holiday_str { get; set; }

        public int num_sun { get; set; }
        public int num_mon { get; set; }
        public int num_tues { get; set; }
        public int num_wednes { get; set; }
        public int num_thurs { get; set; }
        public int num_fri { get; set; }
        public int num_satur { get; set; }

    }

    public class TransectionLeaveModel
    {
        public int Id { get; set; }
        public int Emp_Id { get; set; }
        public System.DateTime DateSt { get; set; }
        public System.DateTime Leave_startdate { get; set; }
        public System.DateTime Leave_enddate { get; set; }
        public string Leave_time { get; set; }
        public string Leave_timeend { get; set; }
        public string Shift { get; set; }
        public double Date_leave { get; set; }
        public double Date_Hour { get; set; }
        public string Reasons_for_leave { get; set; }
        public int Type_ReasonId { get; set; }
        public string Approved_emp { get; set; }
        public string ReasonnotApEmp { get; set; }
        public string Approved_hr { get; set; }
        public string ReasonnotApHr { get; set; }
        public string Status_pay { get; set; }
        public string M_certificate { get; set; }
        public Nullable<System.DateTime> DT_UpEmpN { get; set; }
        public Nullable<System.DateTime> DT_UpHRN { get; set; }
        public string AddbyEmp { get; set; }
        public string AddAppbyHr { get; set; }
        public Nullable<int> Status_SunSat { get; set; }
        public string Note { get; set; }
        public Nullable<int> Status_SendmailHR { get; set; }
        public Nullable<int> StatusWork { get; set; }
        public string StatusWorkDetail { get; set; }
        public string Status_hrchk { get; set; }
        public string Status_hrchkChar { get; set; }
    }

    public class ExportToCsvSumModel
    {
        public int item { get; set; }
        public int emp_id { get; set; }
        public string titlename { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string nickname { get; set; }
        public string department { get; set; }
        public string department_code { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public double business { get; set; }
        public double sick_cert { get; set; }
        public double vacation { get; set; }
        public double other { get; set; }
        public double sick { get; set; }
        public double s_vacation { get; set; }
        public double special_business { get; set; }
    }

    public class LeaveSettingModel
    {
        public string check { get; set; }
        private DateTime _SetDefaultDate = DateTime.Today;
        private DateTime _SetDefaultDateEnd = DateTime.Today;

        [Required]
        public DateTime holiday_date
        {
            get
            {
                return _SetDefaultDate;
            }
            set
            {
                _SetDefaultDate = value;
            }
        }
        [Required]
        public DateTime holiday_date_end
        {
            get
            {
                return _SetDefaultDateEnd;
            }
            set
            {
                _SetDefaultDateEnd = value;
            }
        }
        public List<int> holiday_id { get; set; }
        [Required]
        [MaxLength(50)]
        [Display(Name = "Holiday")]
        public string detail { get; set; }

        public List<SelectListItem> SelectHoliday { get; set; }

    }

    public class EventViewModel
    {
        public Int64 id { get; set; }

        public String title { get; set; }

        public String start { get; set; }

        public String end { get; set; }

        public bool allDay { get; set; }

        public String display { get; set; }

        public String backgroundColor { get; set; }

        public String borderColor { get; set; }

        public String textColor { get; set; }

        public String color { get; set; }

        public bool overlap { get; set; }

        public string[] classNames { get; set; }

        public String type { get; set; }

        public String typeleave { get; set; }

    }

}