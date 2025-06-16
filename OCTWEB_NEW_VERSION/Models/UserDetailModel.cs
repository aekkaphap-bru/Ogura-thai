using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PagedList;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class UserDetailModel
    {
        public int USE_Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "User Account: ")]
        public string USE_Account { get; set; }
        public string USE_Password { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "First Name: ")]
        public string USE_FName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Last Name: ")]
        public string USE_LName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nick Name: ")]
        public string USE_NickName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Tel. : ")]
        public string USE_TelNo { get; set; }

        //[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$")]
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Email: ")]
        public string USE_Email { get; set; }

        [Display(Name = "User Status: ")]
        public int USE_Status { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Nationality: ")]
        public string USE_Nationality { get; set; }
        public IEnumerable<SelectListItem> National { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Department ID: ")]
        public int Dep_Id { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "User Code: ")]
        public Nullable<int> USE_Usercode { get; set; }

        public string Department_name { get; set; }

        public enum Nationality
        {
            Thai,
            Japanese
        }


        public bool BoolStatus
        {
            get { return USE_Status == 1; }
            set { USE_Status = value ? 1 : 0; }
        }


        /*
        public bool BoolStatus
        {
            get { return USE_Status == 1; }
            set
            {
                if (value)
                    USE_Status = 1;
                else
                    USE_Status = 0;
            }
        }
         */
    }

    public class UserDetailPagedListModel
    {
        public int? Page { get; set; }

        
        public int? Usercode { get; set; }

        public string Fname { get; set; }
        public string Nickname { get; set; }
        public int? Dep { get; set; }
        public string National { get; set; }
        public IPagedList<UserDetailModel> UserDetailModelPagedList { get; set; }

        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Nationals { get; set; }
    }
    /*
    public class SearchConditionsModel {
        public string searchUserCode { get; set; }
        public string searchName { get; set; }
        public string searchNickName { get; set; }

        public int[] SelectedDepartment { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        public string SelectedNational { get; set; }
        public IEnumerable<SelectListItem> National { get; set; }
    }
     */
}