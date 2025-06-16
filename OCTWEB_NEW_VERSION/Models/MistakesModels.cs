using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class SearchMistModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int total { get; set; }
        private DateTime? _SetDefaultStartDate = null;
        private DateTime? _SetDefaultEndDate = null;
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
        public string reason_str { get; set; }
        public int? levelwarning_id { get; set; }
        public int count_card_white { get; set; }
        public int count_card_blue { get; set; }
        public int count_card_yellow { get; set; }
        public int count_card_orange { get; set; }
        public int count_card_red { get; set; }
        public int count_total { get; set; }

        public DateTime? start_date
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
        public DateTime? end_date
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
        public IPagedList<EmpMistakesModel> EmpMistakesPagedList { get; set; }
        public IPagedList<EmpMistakesModel> FormerEmpMistakesPagedList { get; set; }
        public List<SelectListItem> SelectReason { get; set; }
        public List<SelectListItem> SelectLevalWarning { get; set; }
        public List<SelectListItem> SelectDepartment { get; set; }
        public List<SelectListItem> SelectPosition { get; set; }
        public List<SelectListItem> SelectCostCenter { get; set; }
        public List<EmpMistakesModel> EmpMistakesList { get; set; }
    }

    public class EmpMistakesModel
    {
        [Required]
        [Display(Name = "Emp ID")]
        public int? emp_id { get; set; }
        public string emp_profile { get; set; }
        public string nickname { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string department { get; set; }
        public string department_code { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public string costcenter_code { get; set; }
        [Required]
        [Display(Name = "Type of mistake")]
        public string reason { get; set; }
        [Required]
        [Display(Name = "Level of warning")]
        public int levelwarning_id { get; set; }
        public string levelwarning { get; set; }
        [Required]
        [Display(Name = "shift")]
        public string shift { get; set; }
        public string amount_mistake_date { get; set; }
        public string amount_stopwork_date { get; set; }
        public string expire_date_str { get; set; }
        public int amount_expire_date { get; set; }
        [Required]
        [Display(Name = "Mistake date")]
        public DateTime? mistake_date { get; set; }
        public DateTime? expire_date { get; set; }
        public DateTime? stopwork_start_date { get; set; }
        public DateTime? stopwork_end_date { get; set; }
        public DateTime? create_date { get; set; }
        public DateTime? layoff_date { get; set; }

        [MaxLength(200)]
        [Display(Name = "Detail")]
        public string detail_reason { get; set; }
        public List<SelectListItem> SelectReason { get; set; }
        public List<SelectListItem> SelectLevalWarning { get; set; }
        public List<SelectListItem> SelectShift { get; set; }
        public int mistake_id { get; set; }
  
    }

    public class DetailEmpMistakesModel
    {
        [Required]
        [Display(Name = "Emp ID")]
        public int? emp_id { get; set; }
        public string emp_profile { get; set; }
        public string nickname { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string department { get; set; }
        public string department_code { get; set; }
        public string position { get; set; }
        public string costcenter { get; set; }
        public string costcenter_code { get; set; }


        public List<SelectListItem> SelectReason { get; set; }

    }

    public class MistaksSettingModel 
    {
        public string check { get; set; }

        public List<int> levelwarning_id { get; set; }
        [MaxLength(150)]
        [Display(Name = "Level of warning")]
        public string levelwarning { get; set; }
       
        public List<int> reason_id { get; set; }
        [MaxLength(250)]
        [Display(Name = "Type for mistake employee")]
        public string reason { get; set; }
       
        public List<SelectListItem> SelectLevelwarning { get; set; }
        public List<SelectListItem> SelectReason { get; set; }

        public List<SelectListItem> SelectWarningReason { get; set; }


    }



}