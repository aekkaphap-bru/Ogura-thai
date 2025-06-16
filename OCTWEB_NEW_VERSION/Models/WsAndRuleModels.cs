using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class WsAndRuleModels
    {
    }

    public class WSR_CompRuleModel
    {
        public int item { get; set; }
        public int CR_Id { get; set; }
        public string CR_Name { get; set; }
        public string CR_File { get; set; }
        public string CR_Note { get; set; }
        public int CRT_Id { get; set; }
        public string CR_Date { get; set; }
        public int CR_Rev { get; set; }
        public string CR_Update { get; set; }
        public string CR_Number { get; set; }
        public string CompRuleType { get; set; }
    }

    public class CompanyRuleSearchModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public string FileName { get; set; }

        public int? searchCompanyRuleTypeId { get; set; }
        public string searchCompanyRuleName { get; set; }
        public string searchCompanyRuleNo { get; set; }
        public IEnumerable<SelectListItem> SelectCompanyRuleType { get; set; }
        
        public IPagedList<WSR_CompRuleModel> WSR_CompRuleModelPagedList { get; set; }
    }

   /*
    public class WSR_WorkingStandardEditModel
    {
        public int WS_Id { get; set; }
        public string WS_Name { get; set; }
        public string WS_Number { get; set; }
        public string WS_File { get; set; }
        public string WS_Note { get; set; }
        public int WST_Id { get; set; }
        public string WS_Date { get; set; }
        public string WS_Update { get; set; }
        public Nullable<int> WSP_Id { get; set; }
        public string WS_Rev { get; set; }
        public string WorkingStandardType { get; set; }
        public string WorkingStandardProcess { get; set; }
    }
     */

    public class WorkigStandardSearchModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public string FileName { get; set; }

        public int? searchDepartmentId { get; set; }
        public string searchWorkingStandardType { get; set; }
        public string searchWorkingStandardProcess { get; set; }
        public string searchWorkingStandardName { get; set; }
        public string searchWorkingStandardNo { get; set; }
        public int? searchSortBy { get; set; }

        public string date_str { get; set; }

        public IEnumerable<SelectListItem> SelectDepartment { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardType { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardProcess { get; set; }
       // public IEnumerable<SelectListItem> SelectWorkingStandardName { get; set; }
       // public IEnumerable<SelectListItem> SelectWorkingStandardNo { get; set;}
        public IEnumerable<SelectListItem> SelectSearchSortBy { get; set; }

        public IPagedList<WSR_WorkingStandardEditModel> WSR_WorkingStandardModelPagedList { get; set; }
    }

    public class Export_WorkingStandardModel
    {
        public int item { get; set; }
        public string WS_Name { get; set; }
        public string WS_Number { get; set; }
        public string WS_File { get; set; }
        public string WS_Note { get; set; }
        public System.DateTime WS_Date { get; set; }
        public System.DateTime WS_Update { get; set; }
        public string UpDate_str { get; set; }
        public string WS_Rev { get; set; }
        public string WorkingStandardType { get; set; }
        public string WorkingStandardProcess { get; set; }
    }

    public class CompanyRuleTypeSetModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public string searcCompanyRuleType { get; set; }
        public IPagedList<WSR_CompRuleTypeModel> WSR_CompRuleTypeModelPagedList { get; set; }
    }

    public class WSR_CompRuleTypeModel
    {
        public int item { get; set; }
        public int CRT_Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string CRT_Type { get; set; }
        public string CRT_Note { get; set; }
    }


    public class WSR_CompRuleSetModel
    {
        public int item { get; set; }
        public int CR_Id { get; set; }
        private DateTime _SetDefaultDate = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Update ")]
        public DateTime CR_Update
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

        //[Required]
        [DataType(DataType.Text)]
        [Display(Name = "Company Rule Name (English) ")]
        //[RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$",ErrorMessage = "Special characters are not allowed.")]
        public string CR_Name_eng { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Company Rule Name (ภาษาไทย) ")]
        public string CR_Name_th { get; set; }

        public string CR_File { get; set; }
        public string CR_Note { get; set; }
        public int CRT_Id { get; set; }
        public DateTime CR_Date { get; set; } 

        [Required]
        [Display(Name = "Revision ")]
        public int CR_Rev { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[0-9a-zA-Z-]*$", ErrorMessage = "Special characters and spaces are not allowed.")]
        [Display(Name = "Company Rule No ")]
        public string CR_Number { get; set; }

        [Required]
        [Display(Name = "Company Rule Type ")]
        public int CompanyRuleTypeId { get; set; }
        public IEnumerable<SelectListItem> SelectCompanyRuleType { get; set; }
    }

    public class CompanyRuleSetModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public string FileName { get; set; }

        public int? searchCompanyRuleTypeId { get; set; }
        public string searchCompanyRuleName { get; set; }
        public string searchCompanyRuleNo { get; set; }
        public IEnumerable<SelectListItem> SelectCompanyRuleType { get; set; }
        
        public IPagedList<WSR_CompRuleModel> WSR_CompRuleModelPagedList { get; set; }
    }

    public class WSR_WorkingStandardTypeEditModel
    {
        public int item { get; set; }
        public int WST_Id { get; set; }

        [Required]
        [MaxLength(60)]
        [Display(Name = "Working Standard Type ")]
        public string WST_Type { get; set; }

        [Display(Name = "Select Department ")]
        public int WST_Dept { get; set; }

        [MaxLength(250)]
        public string WST_Note { get; set; }
        public string DepartmentName { get; set; }
        public IEnumerable<SelectListItem> SelectDepartmentId { get; set; }
    }

    public class WorkingStandardTypeSetModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public int searchDepartmentId { get; set; }
        public string searchWorkingStandardType { get; set; }
        public IEnumerable<SelectListItem> SelectDepartmentId { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardType { get; set; }

        public IPagedList<WSR_WorkingStandardTypeEditModel> WSR_WorkingStandardTypeEditModelPagedList { get; set; }
    }

    public class WSR_WorkingStandardProcessEditModel
    {
        public int item { get; set; }
        public int WSP_Id { get; set; }

        [Required]
        [MaxLength(60)]
        [Display(Name = "Working Standard Process ")]
        public string WSP_ProcessName { get; set; }

        [Display(Name = "Working Standard Type ")]
        public int WST_Id { get; set; }

        [MaxLength(200)]
        public string WSP_Note { get; set; }

        public string WorkingStandardType { get; set; }
        public string DepartmentName { get; set; }

        public int searchDepartmentId { get; set; }
        public IEnumerable<SelectListItem> SelectDepartmentId { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardTypeId { get; set; }
  
    }

    public class WorkingStandardProcessSetModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public int? searchDepartmentId { get; set; }
        public string searchWorkingStandardType { get; set; }
        public string searchWorkingStandardProcess { get; set; }
        public IEnumerable<SelectListItem> SelectDepartmentId { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardType { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardProcess { get; set; }

        public IPagedList<WSR_WorkingStandardProcessEditModel> WSR_WorkingStandardProcessEditModelPagedList { get; set; }
    }


    public class WorkingStandardSetModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public string FileName { get; set; }

        public int? searchDepartmentId { get; set; }
        public string searchWorkingStandardType { get; set; }
        public string searchWorkingStandardProcess { get; set; }
        public string searchWorkingStandardName { get; set; }
        public string searchWorkingStandardNo { get; set; }
        public int? searchSortBy { get; set; }

        public string date_str { get; set; }

        public IEnumerable<SelectListItem> SelectDepartment { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardType { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardProcess { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardName { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardNo { get; set; }
        public IEnumerable<SelectListItem> SelectSearchSortBy { get; set; }

        public IPagedList<WSR_WorkingStandardEditModel> WSR_WorkingStandardModelPagedList { get; set; }
    }

    public class WSR_WorkingStandardEditModel
    {
        public int item { get; set; }
        public int WS_Id { get; set; }
        public string WS_Name { get; set; }

        //[Required]
        [MaxLength(100)]
        [Display(Name = "Working Standard Name (English)")]
        public string WS_Name_eng { get; set; }
        
        [MaxLength(100)]
        [Display(Name = "Working Standard Name (English)")]
        public string WS_Name_th { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Z0-9_.\s-]{1,15}$", ErrorMessage = "Special characters and spaces are not allowed and maximum length of 15 .")] //^[0-9a-zA-Z-]*$
        [Display(Name = "Working Standard No. ")]
        public string WS_Number { get; set; }

        public string WS_File { get; set; }

        [MaxLength(250)]
        public string WS_Note { get; set; }

        [Required]
        [Display(Name = "Working Standard Type ")]
        public int WST_Id { get; set; }

        public System.DateTime WS_Date { get; set; }
        private DateTime _SetDefaultDate = DateTime.Now;
        private string _SetDefaultRevision = "00";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Update ")]
        public System.DateTime WS_Update
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
   
        public string UpDate_str { get; set; }

        [Display(Name = "Working Standard Process ")]
        public int? WSP_Id { get; set; }

        
        [Required]
        [MinLength(1)]
        [RegularExpression("^[0-9]{1,50}$", ErrorMessage = "Revision must be numeric")]
        [Display(Name = "Revision ")]
        public string WS_Rev 
        {
            get { return _SetDefaultRevision; }
            set{ _SetDefaultRevision =value;} 
        }

        public string WorkingStandardType { get; set; }
        public string WorkingStandardProcess { get; set; }

        public int? searchDepartmentId { get; set; }
        public IEnumerable<SelectListItem> SelectDepartment { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardType { get; set; }
        public IEnumerable<SelectListItem> SelectWorkingStandardProcess { get; set; }

        public bool checktraining { get; set; }

        //public List<string> selected_trainWS { get; set; }
        //public IEnumerable<SelectListItem> SelectWSTrainingSection { get; set; }
        public int dept_id { get; set; }
        //public string[] rightValues { get; set; }
        //public IEnumerable<SelectListItem> SelectWSTrainingSection_Right { get; set; }

        //public List<Emp_TrainingWS_temporaryModel> TrainingWS_temp_list { get; set; }

        public List<string> selected_trainWS { get; set; } = new List<string>();
        public IEnumerable<SelectListItem> SelectWSTrainingSection { get; set; }

    }

    public class Emp_TrainingWS_temporaryModel
    {
        public int Id { get; set; }
        public string Train_NumberWS { get; set; }
        public string Train_Header { get; set; }
        public string Train_HeaderThai { get; set; }
        public string Train_DateWS { get; set; }
        public Nullable<int> Train_Status { get; set; }
        public string Trai_NameArea { get; set; }
        public string Trai_Rev { get; set; }
        public string Train_DateChkSendMail { get; set; }
    }

    public class WSTrainingForSendEmailModel
    {
        public string Train_NumberWS { get; set; }
        public string Train_Header { get; set; }
        public string Train_HeaderThai { get; set; }
        public string Train_DateWS { get; set; }
        public string Trai_NameArea { get; set; }

    }
}