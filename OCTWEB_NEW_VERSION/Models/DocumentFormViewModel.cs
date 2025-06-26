using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OCTWEB_NET45.Models
{
    /// <summary>
    /// ViewModel สำหรับฟอร์มสร้าง/แก้ไขเอกสาร
    /// </summary>
    public class DocumentFormViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Request_from { get; set; }
        [Required]
        public string Request_type { get; set; }
        [Required]
        public int Requester_id { get; set; }
        public string Requester_name { get; set; }
        [Required]
        public string Document_type { get; set; }
        [Required]
        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Effective_date { get; set; }
        public string Status { get; set; }
        public string Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public List<DocumentDetailViewModel> DocumentDetails { get; set; } = new List<DocumentDetailViewModel>();
        public List<int> SelectedAreaIds { get; set; } = new List<int>();
        public List<ApprovalStepViewModel> ApprovalSteps { get; set; } = new List<ApprovalStepViewModel>();
        public List<AreaItemViewModel> AvailableAreas { get; set; }
        [StringLength(255)]
        public string File_excel { get; set; }
        [StringLength(255)]
        public string File_pdf { get; set; }
    }
    /// <summary>
    /// ViewModel สำหรับรายละเอียดเอกสารย่อย
    /// </summary>
    public class DocumentDetailViewModel
    {
        public int? Id { get; set; }
        public string WS_number { get; set; }
        public string WS_name { get; set; }
        public string Revision { get; set; }
        public string Num_pages { get; set; }
        public int? Num_copies { get; set; }
        public string File_excel { get; set; }
        public string File_pdf { get; set; }
        public string Change_detail { get; set; }
    }


    /// <summary>
    /// ViewModel สำหรับขั้นตอนการอนุมัติ
    /// </summary>
    public class ApprovalStepViewModel
    {
        public int? Id { get; set; }
        public int Step { get; set; }
        public int? Approver_id { get; set; }
        public DateTime? Approved_at { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
        // <summary>
        /// ViewModel สำหรับพื้นที่การใช้งาน
        /// </summary>
        public class AreaItemViewModel
    {
        public int Id { get; set; }
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public bool IsSelected { get; set; }
        public int FId { get; set; }
        public int LId { get; set; }
        public int WS_TS_Id { get; set; }
        public string Area_name { get; set; }
    }

    public class DocumentListDisplayViewModel
    {
        public int LId { get; set; }
        public DateTime? Created_at { get; set; }
        public string Status { get; set; }
        public string WS_name { get; set; }
        public string WS_number { get; set; }
        public string RequesterName { get; set; }
    }

}