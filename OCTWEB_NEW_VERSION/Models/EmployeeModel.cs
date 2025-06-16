using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class SearchEmpModel
    {
        private DateTime? _SetDefaultStartDate = null;
        private DateTime? _SetDefaultEndDate = null;

        public int count_employee_search { get; set; }
        public string count_employee_search_detail { get; set; }
        public int count_contractor_search { get; set; }
        public string count_contractor_search_detail { get; set; }
        public int count_all_search { get; set; }
        public string count_all_search_detail { get; set; }

        public string date_dataupdate { get; set; }
        public int count_employee { get; set; }
        public int count_contractor { get; set; }
        public int count_all { get; set; }
        public bool rights_45 { get; set; } //To search with ID code
        public bool rights_49 { get; set; } //To export data
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public int? emp_id { get; set; }
        public string nickname { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string deptname { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public string nationality { get; set; }
        public string idnumber { get; set; }
        public DateTime? dateworking_start
        {
            get
            {
                return _SetDefaultStartDate;
            }
            set
            {
                _SetDefaultStartDate = value;
            }
        }
        public DateTime? dateworking_end
        {
            get
            {
                return _SetDefaultEndDate;
            }
            set
            {
                _SetDefaultEndDate = value;
            }
        }
        public string emp_status { get; set; }
        public string education { get; set; }

        public IPagedList<EmployeeModel> EmployeePagedList { get; set; }
        public List<EmployeeModel> EmployeeList { get; set; }
        public List<SelectListItem> SelectDepartment { get; set; }
        public List<SelectListItem> SelectPosition { get; set; }
        public List<SelectListItem> SelectCostCenter { get; set; }
        public List<SelectListItem> SelectEducation { get; set; }
        public List<SelectListItem> SelectNational { get; set; }
        public List<SelectListItem> SelectEmpStatus { get; set; }

    }

    public class EmployeeModel
    {
        public int id { get; set; }
        public string logo { get; set; }
        public string name_eng { get; set; }
        public string name_th { get; set; }
        public string age_str { get; set; }
        public string address_str { get; set; }
        public string emp_profile { get; set; }
        public string jobdes_path { get; set; }
        public string working_period { get; set; }
        public string position_period { get; set; }
        public bool rights_unlockpage { get; set; } //To unlock page and edit the info (50)
        public bool rights_layoff { get; set; } //To layoff employees (52)
        public bool rights_see_training { get; set; }  //Can see Training Employee (58)
        public bool rights_see_leave { get; set; } //Can see Work Leave All(60)
        public bool rights_see_IDcode_PassportID_TaxID_SSO {get;set;} // //To see employees IDcode/PassportID/TaxID/SSO (38)
        public bool rights_see_birth_date { get; set; } //To see employees birth date info (44)

        public List<SelectListItem> SelectGender { get; set; }
        public List<SelectListItem> SelectTitleEn { get; set; }
        public List<SelectListItem> SelectTitleTh { get; set; }
        public List<SelectListItem> SelectProvince { get; set; }
        public List<SelectListItem> SelectDistrict { get; set; }
        public List<SelectListItem> SelectSubDistrict { get; set; }
        public List<SelectListItem> SelectEducation { get; set; }
        public List<SelectListItem> SelectDepartment { get; set; }
        public List<SelectListItem> SelectPosition { get; set; }
        public List<SelectListItem> SelectCostCenter { get; set; }
        public List<SelectListItem> SelectEmployeeStatus { get; set; }
        public List<SelectListItem> SelectNationality { get; set; }
        public List<SelectListItem> SelectLayoffReason { get; set; }
        public List<SelectListItem> SelectLayoffType { get; set; }

        [Required]
        [Display(Name="Employee ID")]
        public int EmpID { get; set; }
        [MaxLength(25)]
        [Display(Name = "ID Number")]
        public string IDcode { get; set; }
        [MaxLength(25)]
        [Display(Name = "Passport ID")]
        public string PPID { get; set; }
        [MaxLength(25)]
        [Display(Name = "Tax ID")]
        public string TaxID { get; set; }
        [MaxLength(25)]
        [Display(Name = "SSO Number")]
        public string SSO { get; set; }
        [Required]
        [Display(Name = "Title(TH)")]
        public string Title_TH { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name(TH)")]
        public string FName_TH { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name(TH)")]
        public string LName_TH { get; set; }
        [MaxLength(25)]
        [Display(Name = "Nick Name(TH)")]
        public string NName_TH { get; set; }
        [Required]
        [Display(Name = "Title(EN)")]
        public string Title_EN { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "Fist Name(EN)")]
        public string FName_EN { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name(EN)")]
        public string LName_EN { get; set; }
        [MaxLength(25)]
        [Display(Name = "Nick Name(EN)")]
        public string NName_EN { get; set; }
        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }
        [Required]
        [Display(Name = "Nationality")]
        public string Nation { get; set; }
        public Nullable<System.DateTime> BDate { get; set; }
        public bool Disabled_stat { get; set; }
        public string Disabled_By { get; set; }
        [MaxLength(150)]
        [Display(Name = "Building")]
        public string Addr_Build { get; set; }
        [MaxLength(100)]
        [Display(Name = "House No")]
        public string Addr_No { get; set; }
        [MaxLength(100)]
        [Display(Name = "Sol/Alley")]
        public string Addr_Alle { get; set; }
        [MaxLength(100)]
        [Display(Name = "Road/Street")]
        public string Addr_Rd { get; set; }
        [MaxLength(100)]
        [Display(Name = "Moo")]
        public string Addr_Vill { get; set; }
        [MaxLength(100)]
        [Display(Name = "Sub-District")]
        public string Addr_Prsh { get; set; }
        [MaxLength(100)]
        [Display(Name = "District")]
        public string Addr_Dtrct { get; set; }
        [MaxLength(100)]
        [Display(Name = "Province")]
        public string Addr_Prvnc { get; set; }
        [MaxLength(100)]
        [Display(Name = "Post Code")]
        public string Addr_Post { get; set; }
        [MaxLength(50)]
        [Display(Name = "Eduction")]
        public string Educate { get; set; }
        [MaxLength(100)]
        [Display(Name = "Internal E-mail")]
        public string In_Email { get; set; }
        [MaxLength(100)]
        [Display(Name = "External E-mail")]
        public string Ex_Email { get; set; }
        [MaxLength(25)]
        [Display(Name = "Mobile")]
        public string Mobile { get; set; }
        [MaxLength(25)]
        [Display(Name = "Tel.")]
        public string Tel { get; set; }
        [MaxLength(50)]
        [Display(Name = "Bank Account")]
        public string Bank_Acc { get; set; }
        public string DeptCode { get; set; }
        public string DeptDesc { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> PassDate { get; set; }
        public Nullable<System.DateTime> StartPosition { get; set; }
        [MaxLength(300)]
        [Display(Name = "Note")]
        public string Note { get; set; }
        public string EmpStatus { get; set; }
        [MaxLength(100)]
        [Display(Name = "Reasons for the job probation pass a slow ")]
        public string ReasonPPS { get; set; }
        public Nullable<System.DateTime> DatePasSport { get; set; }
        public string DatePasSportExpire { get; set; }
        public Nullable<System.DateTime> LayoffDate { get; set; }
        [MaxLength(250)]
        [Display(Name="Layoff Reason")]
        public string LayoffReas { get; set; }
        [MaxLength(250)]
        [Display(Name="Layoff Detail")]
        public string LayoffDetail { get; set; }
        public string LayoffType { get; set; }

        public DateTime? leave_start_date { get; set; }
        public DateTime? leave_end_date { get; set; }

        //Povident fund
        public DateTime? pvd1_start_date { get; set; }
        public DateTime? pvd1_end_date { get; set; }
        public DateTime? pvd2_start_date { get; set; }
        public DateTime? pvd2_end_date { get; set; }
        public string pvd1_period { get; set; }
        public string pvd2_period { get; set; }

    }

    public class LayoffModel
    {
        public int id { get; set; }
        public string typelayoff { get; set; }
        public string reasonlayoff { get; set; }

        [MaxLength(150)]
        [Display(Name="Other(อื่นๆ)")]
        public string other_type { get; set; }
        public bool notime { get; set; }
        public DateTime? layoff_date { get; set; }
        public List<SelectListItem> SelectTypeLayoff { get; set; }
        public List<SelectListItem> SelectReasonLayoff { get; set; }
    }

    public class EmployeeFormerSendMailModel
    {
        public List<SelectListItem> SelectDepartmentId { get; set; }
        public int searchDepartmentId { get; set; }
        public EmployeeModel EmployeeModel { get; set; }
        public List<UserDetailModel> UserList { get; set; }
    }

    public class EmpSumModel
    {
        public string department { get; set; }
        public int total { get; set; }
        public int sum_male { get; set; }
        public int sum_female { get; set; }
        
    }
    public class EmpSumListModel
    {
        public List<EmpSumModel> EmpSumList { get; set; }
        public int total { get; set; }
        public int sum_male { get; set; }
        public int sum_female { get; set; }
        public string title { get; set; }

    }
}