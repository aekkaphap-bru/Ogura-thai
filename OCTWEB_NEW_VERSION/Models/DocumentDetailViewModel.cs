using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OCTWEB_NET45.Models
{
    /// <summary>
    /// ViewModel หลักสำหรับฟอร์มเอกสาร ที่ปรับปรุงแล้ว
    /// </summary>
    public class DocumentMasterModel
    {

        // --- Document Master ---
        public int LId { get; set; } // เปลี่ยนชื่อจาก Id เป็น LId เพื่อให้สอดคล้องกับ Database Model และ Controller

        public int? Id { get; set; } // ID ของเอกสารหลัก (อาจเป็น null สำหรับเอกสารใหม่)
        [Required]
        public string Request_from { get; set; }

        [Required]
        public string Request_type { get; set; }

        [Required]
        public int Requester_id { get; set; }

        public string Requester_name { get; set; }
        public byte[] RequesterSignature { get; set; }

        [Required]
        public string Document_type { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Effective_date { get; set; }

        public string Status { get; set; }
        public string DarNo { get; set; }
        public DateTime? Updated_at { get; set; }

        // --- Child Collections ---

        /// <summary>
        /// รายการเอกสารย่อย ผู้ใช้สามารถเพิ่มได้หลายรายการ แต่ละรายการมีไฟล์แนบของตัวเอง
        /// </summary>
        //public List<DocumentDetailViewModel> DocumentDetails { get; set; } = new List<DocumentDetailViewModel>();
        public DocumentDetailModel DocumentDetail { get; set; }

        /// <summary>
        /// ข้อมูลรายละเอียดของเอกสาร (สำหรับเอกสารเดียว)
        /// </summary>
        /// DocumentDetailModel
        //public DocumentDetailModel DocumentDetail { get; set; } = new DocumentDetailModel();

        /// <summary>
        /// รายการพื้นที่ทั้งหมดที่มีให้เลือก พร้อมสถานะว่าถูกเลือกหรือไม่ (IsSelected)
        /// </summary>

        public List<AreaItemViewModel> AvailableAreas { get; set; }

        /// <summary>
        /// ขั้นตอนการอนุมัติทั้งหมดของเอกสารนี้
        /// </summary>
        public List<ApprovalStepViewModel> ApprovalSteps { get; set; } = new List<ApprovalStepViewModel>();

        // --- Data for Form Controls (e.g., Dropdowns) ---
        public List<CustomSelectListItem> RequestTypes { get; set; }


        // --- Helper Properties for View Logic ---
        public int CurrentUserId { get; set; }
        public bool CurrentUserPosition { get; set; }

    }

    /// <summary>
    /// ViewModel สำหรับรายละเอียดเอกสารย่อย (ไม่เปลี่ยนแปลง)
    /// แต่ละรายการคือเอกสาร 1 บรรทัดในฟอร์ม
    /// </summary>
    public class DocumentDetailModel
    {
        public int DId { get; set; } // ID ของตาราง DocumentDetails
        public string WS_number { get; set; }
        public string WS_name { get; set; }
        public string Revision { get; set; }
        public string Num_pages { get; set; }
        public int? Num_copies { get; set; }
        public string Change_detail { get; set; }

        // ไฟล์แนบจะอยู่ในระดับนี้ ซึ่งถูกต้องแล้ว
        [StringLength(255)]
        public string File_excel { get; set; }

        [StringLength(255)]
        public string File_pdf { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับพื้นที่การใช้งานที่ปรับปรุงใหม่ให้ง่ายขึ้น
    /// ใช้สำหรับแสดง Checkbox list ในฟอร์ม
    /// </summary>
    public class AreaViewModel
    {
        public int Id { get; set; } // Id ของพื้นที่จากตาราง Master (WS_Tech_Spec)
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public bool IsSelected { get; set; } // ใช้ property นี้ในการรับ-ส่งค่าว่าถูกเลือกหรือไม่
    }

    /// <summary>
    /// ViewModel สำหรับขั้นตอนการอนุมัติ (ไม่เปลี่ยนแปลง)
    /// </summary>
    public class ApprovalModel
    {
        public int AId { get; set; }
        public int LId { get; set; }
        public int Step { get; set; }
        public int? Approver_id { get; set; }
        public string ApproverName { get; set; }
        public DateTime? Approved_at { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string StepName { get; set; }

        // ข้อมูลรีวิวจะอยู่ที่นี่ ซึ่งถูกต้องแล้ว
        public bool? FMEA_Review { get; set; }
        public bool? ControlPlan_Review { get; set; }
        public bool? ProcessFlow_Review { get; set; }
    }

    public class ApproverInfos
    {
        public int Step { get; set; }
        public string ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public byte[] SignatureImage { get; set; } // <-- for Approver's signature
    }

    public class CustomSelectListItems
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string ThaiText { get; set; }
        public string ThaiDescription { get; set; }
    }

    public class AreaItemViewModels
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


    // หมายเหตุ: Class อื่นๆ เช่น ApproverInfo, CustomSelectListItem,
    // DocumentListDisplayViewModel, และ ApprovalRequestViewModel
    // มีวัตถุประสงค์เฉพาะและไม่ซ้ำซ้อน จึงคงไว้ตามเดิม
}