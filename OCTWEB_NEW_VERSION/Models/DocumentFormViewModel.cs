using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OCTWEB_NET45.Models
{
    public class DocumentFormViewModel
    {
        // ข้อมูลจาก DocumentList
        public int? Id { get; set; }

        [Required]
        public string Request_from { get; set; }

        [Required]
        public string Request_type { get; set; }

        [Required]
        public int Requester_id { get; set; }

        [Required]
        public string Document_type { get; set; }

        [Required]
        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        public DateTime Effective_date { get; set; }

        public string Status { get; set; }

        public DateTime? Created_at { get; set; }

        public DateTime? Updated_at { get; set; }

        // รายการเอกสารย่อย
        public List<DocumentDetailViewModel> DocumentDetails { get; set; } = new List<DocumentDetailViewModel>();

        // พื้นที่การใช้งาน (จาก Area)
        public List<int> SelectedAreaIds { get; set; } = new List<int>();
        //public List<AreaViewModel> AvailableAreas { get; set; } = new List<AreaViewModel>();

        // ผู้อนุมัติในขั้นตอนต่าง ๆ
        public List<ApprovalStepViewModel> ApprovalSteps { get; set; } = new List<ApprovalStepViewModel>();

        public List<AreaItemViewModel> AvailableAreas { get; set; }
    }

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

    public class AreaViewModel
    {
        public int Area_id { get; set; }
        public string Area_name { get; set; }
    }

    public class ApprovalStepViewModel
    {
        public int? Id { get; set; }

        public int Step { get; set; }

        public int? Approver_id { get; set; }

        public DateTime? Approved_at { get; set; }

        public string Status { get; set; }

        public string Comment { get; set; }
    }

    public class AreaItemViewModel
    {
        public int Id { get; set; }
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public bool IsSelected { get; set; }
    }
}
