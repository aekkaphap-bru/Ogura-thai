using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class BenefitRequestModel
    {
        // ----------- Primary Info --------------
        public int Id { get; set; }

        // ----------- Employee Info --------------
        public string EmpId { get; set; }
        public string PicEmp { get; set; }
        public string EmpName { get; set; }
        public string EmpLastname { get; set; }
        public string Dep { get; set; }

        // ----------- Benefit Details --------------
        [Required(ErrorMessage = "TypeBenefit is required")]
        public string TypeBenef { get; set; }
        [Required(ErrorMessage = "Relation is required")]
        public string Relation { get; set; }

        // ----------- Related Person Info --------------
        

        [Required(ErrorMessage = "First name (TH) is required")]
        [StringLength(50, ErrorMessage = "First name (TH) cannot exceed 50 characters")]
        public string TRName { get; set; }

        [Required(ErrorMessage = "First name (EN) is required")]
        [StringLength(50, ErrorMessage = "First name (EN) cannot exceed 50 characters")]
        public string FRName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LRName { get; set; }
   

        // ----------- Salary Cycle --------------
        [Required(ErrorMessage = "Month is required")]
        public string Months { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
        public string Years { get; set; }
        public string SalaryCycle { get; set; }

        // ----------- Document --------------
        public string Document { get; set; }

        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "pdf,doc,docx,jpg,jpeg,png", ErrorMessage = "Allowed file types: PDF, DOC, DOCX, JPG, JPEG, PNG")]
        public HttpPostedFileBase DocumentFile { get; set; }
        public string UploadedFiles { get; set; }

        // ----------- Status / Time --------------
        public string Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateTimeFormatted { get; set; }

        public int RowNumber { get; set; }
    }

    public class BenefitRequestViewModel
    {
        // ----------- Collection / List --------------
        //public IPagedList<BenefitRequestModel> BenefitRequestList { get; set; }
        public SearchBenefitRequestViewModel SearchModel { get; set; } 
        public IEnumerable<BenefitRequestModel> BenefitRequestList { get; set; }

        // ----------- Dropdown Data --------------
        public List<SelectListItem> DepartmentsList { get; set; }
        public List<SelectListItem> RequestTypesList { get; set; }
        public List<SelectListItem> MontsList { get; set; }
        public List<SelectListItem> YearsList { get; set; }

        // ----------- Employee Info --------------
        public string EmpId { get; set; }
        public string Dep { get; set; }
        public string PicEmp { get; set; }
        

        // ----------- Benefit Data --------------
        [Required, Display(Name = "Benefit Type")]
        public string TypeBenef { get; set; }

        [Required, Display(Name = "Relationship")]
        public string Relation { get; set; }

        // ----------- Related Person Info --------------
        [Required, StringLength(50), Display(Name = "Title")]
        public string TRName { get; set; }

        [Required, StringLength(50), Display(Name = "First Name")]
        public string FRName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LRName { get; set; }

        // ----------- Salary Cycle --------------
        [Required, Display(Name = "Month")]
        public string Months { get; set; }

        [Required, Display(Name = "Year")]
        public string Years { get; set; }

        // ----------- Document Upload --------------
        public String PathDefaultWS { get; set; }

        // ----------- Others --------------
        public DateTime? CreateTime { get; set; }
        public string RequestType { get; set; }
        public string Department { set; get; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }


    }

    public class SearchBenefitRequestViewModel
    {
        public string EmpId { set; get; }
        public string Department { set; get; }
        public string RequestType { get; set; }
        public string Months { get; set; }
        public string Years { get; set; }
        public int? Page { get; set; }

    }

    public class BenefitDropdownConfig
    {
        public List<string> Relations { get; set; }
        public List<string> Titles { get; set; }
        public bool AllowCustom { get; set; }
    }


}

//public class BenefitRequest
//{

//    public int Id { get; set; }
//    public string EmpId { get; set; }
//    public string PicEmp { get; set; }
//    public string EmpName { get; set; }
//    public string EmpLastname { get; set; }
//    public string Dep { get; set; }
//    public string TypeBenef { get; set; }
//    public string Relation { get; set; }
//    public string Titlename { get; set; }
//    public string Name { get; set; }
//    public string Lastname { get; set; }
//    public string SalaryCycle { get; set; }
//    public string Document { get; set; }
//    public string CreateTimeFormatted { get; set; }
//    public string Status { get; set; }

//    //public string PathDefaultWS { get; set; }

//    [Required(ErrorMessage = "Month is required")]
//    public string Months { get; set; }

//    [Required(ErrorMessage = "Year is required")]
//    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
//    public string Years { get; set; }

//    [Required(ErrorMessage = "First name (TH) is required")]
//    [StringLength(50, ErrorMessage = "First name (TH) cannot exceed 50 characters")]
//    public string TRName { get; set; }

//    [Required(ErrorMessage = "First name (EN) is required")]
//    [StringLength(50, ErrorMessage = "First name (EN) cannot exceed 50 characters")]
//    public string FRName { get; set; }

//    [Required(ErrorMessage = "Last name is required")]
//    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
//    public string LRName { get; set; }
//}

//public class BenefitRequestViewModel
//{
//    //public PagedList<BenefitRequest> BenefitRequestList { get; set; }

//    //ใช้สำหรับการแสดงข้อมูลในหน้า View
//    public IPagedList<BenefitRequest> BenefitRequestList { get; set; }
//    public String PathDefaultWS { get; set; }
//    public HttpPostedFileBase DocumentFile { get; set; }

//    public string EmpId { get; set; }  // ID พนักงาน
//    public string PicEmp { get; set; }

//    [Required]
//    [Display(Name = "Benefit Type")]
//    public string TypeBenef { get; set; }  // ประเภทการเบิก

//    [Required]
//    [Display(Name = "Relationship")]
//    public string Relation { get; set; }  // ความสัมพันธ์

//    [Required, StringLength(50)]
//    public string TRName { get; set; } // คำนำหน้า (Title)

//    [Required, StringLength(50)]
//    public string FRName { get; set; } // ชื่อ (First Name)

//    [Required, StringLength(50)]
//    public string LRName { get; set; } // นามสกุล (Last Name)

//    [Required]
//    [Display(Name = "Month")]
//    public string Months { get; set; }  // เดือน

//    [Required]
//    [Display(Name = "Year")]
//    public string Years { get; set; }  // ปี

//    [Required]
//    [Display(Name = "Department")]
//    public string Dep { get; set; }  // แผนก

//    public string Document { get; set; }  // ไฟล์แนบ

//    public DateTime CreateTime { get; set; } // รายการ Benefit Types

//    public string Department { set; get; }
//    public string RequestType { get; set; }



//    //public IEnumerable<SelectListItem> Departments { get; set; }
//    public List<SelectListItem> DepartmentsList { get; set; } // เพิ่ม Property สำหรับ Department
//    public List<SelectListItem> RequestTypesList { get; set; } // เพิ่ม Property สำหรับ RequestType
//    public List<SelectListItem> MontsList { get; set; }
//    public List<SelectListItem> YearsList { get; set; }

//}

//public class CreateBenefit
//{
//    [Required(ErrorMessage = "Employee ID is required")]
//    [StringLength(10, ErrorMessage = "Employee ID cannot exceed 10 characters")]
//    public string EmpId { get; set; }

//    [Required(ErrorMessage = "Department is required")]
//    [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters")]
//    public string Dep { get; set; }

//    [Required(ErrorMessage = "Benefit type is required")]
//    public string TypeBenef { get; set; }

//    [Required(ErrorMessage = "Relationship is required")]
//    public string Relation { get; set; }

//    [Required(ErrorMessage = "Month is required")]
//    public string Months { get; set; }

//    [Required(ErrorMessage = "Year is required")]
//    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
//    public string Years { get; set; }

//    [Required(ErrorMessage = "First name (TH) is required")]
//    [StringLength(50, ErrorMessage = "First name (TH) cannot exceed 50 characters")]
//    public string TRName { get; set; }

//    [Required(ErrorMessage = "First name (EN) is required")]
//    [StringLength(50, ErrorMessage = "First name (EN) cannot exceed 50 characters")]
//    public string FRName { get; set; }

//    [Required(ErrorMessage = "Last name is required")]
//    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
//    public string LRName { get; set; }

//    // การอัปโหลดไฟล์
//    [DataType(DataType.Upload)]
//    [FileExtensions(Extensions = "pdf,doc,docx,jpg,jpeg,png", ErrorMessage = "Allowed file types: PDF, DOC, DOCX, JPG, JPEG, PNG")]
//    public HttpPostedFileBase DocumentFile { get; set; }
//    public string UploadedFiles { get; set; } // เพิ่ม property สำหรับรับชื่อไฟล์
//}

//public class SearchBenefitRequestViewModel
//{
//    public string EmpId {  set; get; }
//    public string Department { set; get; }
//    public string RequestType { get; set; }
//    public string Months { get; set; }
//    public string Years { get; set; }
//    public int? Page { get; set; }

//}

