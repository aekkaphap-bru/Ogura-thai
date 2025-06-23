using Microsoft.AspNet.Identity;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace OCTWEB_NET45.Controllers.DocumentControll
{
    [Authorize]
    [CustomAuthorize(73)]
    public class DocumentController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        public static class DocumentTypes
        {
            public const string Common = "Common";
            public const string ISO = "ISO 14001";
        }

        public static class DocumentStatus
        {
            public const string PendingDeptHead = "PENDING DEPT HEAD";
            public const string Approved = "APPROVED";
            public const string Rejected = "REJECTED";
            public const string Draft = "DRAFT";
        }

        // GET: Document/List
        public ActionResult List()
        {
            try
            {
                //var documents = db.DocumentLists
                //    .Include(d => d.DocumentDetails)
                //    .OrderByDescending(d => d.Created_at)
                //    .ToList();

                //return View(documents);
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "เกิดข้อผิดพลาดในการโหลดข้อมูล: " + ex.Message;
                return View(new List<DocumentList>());
            }
        }

        // GET: Document/Create
        public ActionResult Create()
        {
            try
            {
                var model = new DocumentFormViewModel
                {
                    Request_from = DocumentTypes.Common,
                    Status = DocumentStatus.Draft,
                    Requester_id = GetCurrentUserId(),
                    Effective_date = DateTime.Now.AddDays(7), // Default 7 days from now
                    DocumentDetails = new List<DocumentDetailViewModel>
                    {
                        new DocumentDetailViewModel() // เพิ่ม default document detail
                    }
                };

                // Load available areas
                model.AvailableAreas = LoadAvailableAreas();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "เกิดข้อผิดพลาดในการโหลดฟอร์ม: " + ex.Message;
                return View(new DocumentFormViewModel());
            }
        }

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DocumentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = 400;
                    var errors = ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
                    return Json(new { success = false, message = "ข้อมูลที่กรอกไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง", errors = errors });
                }

                if (!ValidateDocumentRequest(model))
                {
                    Response.StatusCode = 400; 
                    var errors = ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
                    return Json(new { success = false, message = "มีข้อผิดพลาดตามกฎทางธุรกิจ กรุณาตรวจสอบอีกครั้ง", errors = errors });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Create main document record
                        var document = CreateDocumentRecord(model);
                        db.DocumentLists.Add(document);
                        db.SaveChanges(); // Save to get document.LId before processing details/uploads

                        // Process document details
                        ProcessDocumentDetails(model, document.LId);

                        // Process selected areas
                        ProcessSelectedAreas(model, document.LId);

                        // Create approval workflow
                        CreateApprovalWorkflow(document.LId);

                        db.SaveChanges(); // Save all changes after processing details/areas/uploads/workflow
                        transaction.Commit();

                        // TempData["SuccessMessage"] จะไม่แสดงผลในการตอบกลับ AJAX แต่สามารถใช้สำหรับ redirect ได้
                        // TempData["SuccessMessage"] = "บันทึกคำร้องเรียบร้อยแล้ว หมายเลขคำร้อง: " + document.LId; 

                        // หากสำเร็จ: ส่ง JSON กลับไปพร้อมสถานะสำเร็จและ URL สำหรับ Redirect
                        return Json(new { success = true, message = $"ส่งคำร้องเรียบร้อยแล้ว หมายเลขคำร้อง: {document.LId}", redirectUrl = Url.Action("List", "Document") });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // บันทึกข้อผิดพลาด (เช่น Log ex)
                        Console.WriteLine($"Error during transaction: {ex.Message}");

                        Response.StatusCode = 500; // ตั้งค่าสถานะ HTTP เป็น Internal Server Error
                        return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in Create action: {ex.Message}");

                Response.StatusCode = 500;
                return Json(new { success = false, message = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง: " + ex.Message });
            }
        }

        // GET: Document/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    try
        //    {
        //        var document = db.DocumentLists
        //            .Include(d => d.DocumentDetails)
        //            .FirstOrDefault(d => d.LId == id);

        //        if (document == null)
        //        {
        //            TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
        //            return RedirectToAction("List");
        //        }

        //        // Check if current user can edit this document
        //        if (!CanEditDocument(document))
        //        {
        //            TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขคำร้องนี้";
        //            return RedirectToAction("List");
        //        }

        //        var model = MapDocumentToViewModel(document);
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "เกิดข้อผิดพลาด: " + ex.Message;
        //        return RedirectToAction("List");
        //    }
        //}

        // POST: Document/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DocumentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableAreas = LoadAvailableAreas();
                    return View(model);
                }

                var document = db.DocumentLists.FirstOrDefault(d => d.LId == model.Id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                    return RedirectToAction("List");
                }

                if (!CanEditDocument(document))
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขคำร้องนี้";
                    return RedirectToAction("List");
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Update document record
                        UpdateDocumentRecord(document, model);

                        // Process new file uploads
                        ProcessFileUploads(model, document.LId);

                        // Update document details
                        UpdateDocumentDetails(model, document.LId);

                        // Update selected areas
                        UpdateSelectedAreas(model, document.LId);

                        document.Updated_at = DateTime.Now;
                        db.SaveChanges();
                        transaction.Commit();

                        TempData["SuccessMessage"] = "แก้ไขคำร้องเรียบร้อยแล้ว";
                        return RedirectToAction("List");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("เกิดข้อผิดพลาดในการแก้ไขข้อมูล: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                model.AvailableAreas = LoadAvailableAreas();
                return View(model);
            }
        }

        //// GET: Document/Details/5
        //public ActionResult Details(int id)
        //{
        //    try
        //    {
        //        var document = db.DocumentLists
        //            .Include(d => d.DocumentDetails)
        //            .FirstOrDefault(d => d.LId == id);

        //        if (document == null)
        //        {
        //            TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
        //            return RedirectToAction("List");
        //        }

        //        var model = MapDocumentToViewModel(document);
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "เกิดข้อผิดพลาด: " + ex.Message;
        //        return RedirectToAction("List");
        //    }
        //}

        // POST: Document/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var document = db.DocumentLists.FirstOrDefault(d => d.LId == id);
                if (document == null)
                {
                    return Json(new { success = false, message = "ไม่พบข้อมูลคำร้อง" });
                }

                if (!CanDeleteDocument(document))
                {
                    return Json(new { success = false, message = "คุณไม่มีสิทธิ์ลบคำร้องนี้" });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete related records
                        var details = db.DocumentDetails.Where(d => d.Doc_id == id).ToList();
                        db.DocumentDetails.RemoveRange(details);

                        // Delete document
                        db.DocumentLists.Remove(document);
                        db.SaveChanges();
                        transaction.Commit();

                        return Json(new { success = true, message = "ลบคำร้องเรียบร้อยแล้ว" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("เกิดข้อผิดพลาดในการลบข้อมูล: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Private Helper Methods

        private List<AreaItemViewModel> LoadAvailableAreas()
        {
            return db.WS_Training_Section.Select(s => new AreaItemViewModel
            {
                Id = s.Id,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                IsSelected = false
            }).OrderBy(a => a.SectionCode).ToList();
        }

        private int GetCurrentUserId()
        {
            if (Session["UserCode"] != null)
            {
                return Convert.ToInt32(Session["UserCode"]);
            }
            throw new UnauthorizedAccessException("ไม่พบข้อมูลผู้ใช้งาน");
        }

        private bool ValidateDocumentRequest(DocumentFormViewModel model)
        {
            bool isValid = true;

            // Validate document details
            if (model.DocumentDetails == null || !model.DocumentDetails.Any())
            {
                ModelState.AddModelError("DocumentDetails", "กรุณาเพิ่มรายละเอียดเอกสารอย่างน้อย 1 รายการ");
                isValid = false;
            }
            else
            {
                for (int i = 0; i < model.DocumentDetails.Count; i++)
                {
                    var detail = model.DocumentDetails[i];
                    if (string.IsNullOrWhiteSpace(detail.WS_number))
                    {
                        ModelState.AddModelError($"DocumentDetails[{i}].WS_number", "กรุณากรอกหมายเลขเอกสาร");
                        isValid = false;
                    }
                    if (string.IsNullOrWhiteSpace(detail.WS_name))
                    {
                        ModelState.AddModelError($"DocumentDetails[{i}].WS_name", "กรุณากรอกชื่อเรื่อง");
                        isValid = false;
                    }
                }
            }

            // Validate selected areas
            if (model.AvailableAreas == null || !model.AvailableAreas.Any(a => a.IsSelected))
            {
                ModelState.AddModelError("AvailableAreas", "กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ");
                isValid = false;
            }

            // Validate effective date
            //if (model.Effective_date < DateTime.Now.Date)
            //{
            //    ModelState.AddModelError("Effective_date", "วันที่มีผลบังคับใช้ต้องไม่เป็นวันที่ในอดีต");
            //    isValid = false;
            //}

            return isValid;
        }

        private DocumentList CreateDocumentRecord(DocumentFormViewModel model)
        {
            return new DocumentList
            {
                Request_from = model.Request_from ?? DocumentTypes.Common,
                Request_type = model.Request_type,
                Requester_id = model.Requester_id,
                Document_type = model.Document_type,
                Effective_date = model.Effective_date,
                Status = DocumentStatus.PendingDeptHead,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now
            };
        }

        private void ProcessDocumentDetails(DocumentFormViewModel model, int documentId)
        {
            if (model.DocumentDetails != null && model.DocumentDetails.Any())
            {
                //// Process file uploads
                //ProcessFileUploads(model,documentId); 

                foreach (var detail in model.DocumentDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.WS_number) && !string.IsNullOrWhiteSpace(detail.WS_name))
                    {
                        var docDetail = new DocumentDetail
                        {
                            Doc_id = documentId,
                            WS_number = detail.WS_number.Trim(),
                            WS_name = detail.WS_name.Trim(),
                            Revision = detail.Revision?.Trim() ?? "01",
                            Num_pages = detail.File_excel,
                            Num_copies = detail.Num_copies ?? 1,
                            File_excel = "1",
                            File_pdf = "2",
                            Change_detail = detail.Change_detail?.Trim()
                        };
                        db.DocumentDetails.Add(docDetail);
                    }
                }
            }
        }

        private void ProcessSelectedAreas(DocumentFormViewModel model, int documentId)
        {
            if (model.AvailableAreas != null)
            {
                var selectedAreaIds = model.AvailableAreas.Where(a => a.IsSelected).Select(a => a.Id).ToList();
                model.SelectedAreaIds = selectedAreaIds;

                // Save selected areas to junction table if exists
                // Implementation depends on your database schema
            }
        }


        //private void ProcessOptionWorkingStandard(DocumentFormViewModel model, int documentId)
        //{

        //}



        private void ProcessFileUploads(DocumentFormViewModel model, int documentId)
        {
            string path_excel = ConfigurationManager.AppSettings["path_Document_Excel"];
            string path_pdf = ConfigurationManager.AppSettings["path_Document_Pdf"];

            if (!Directory.Exists(path_excel)) Directory.CreateDirectory(path_excel);
            if (!Directory.Exists(path_pdf)) Directory.CreateDirectory(path_pdf);

            var files = Request.Files;

            for (int i = 0; i < model.DocumentDetails.Count; i++)
            {
                var detail = model.DocumentDetails[i];

                string filePdfKey = $"DocumentDetails[{i}].File_pdf";
                string fileExcelKey = $"DocumentDetails[{i}].File_excel";

                var filePdf = files[filePdfKey];
                var fileExcel = files[fileExcelKey];

              

                if (filePdf != null && filePdf.ContentLength > 0)
                {
                    string pdfFileName = GenerateUniqueFileName("PDF", documentId, filePdf.FileName);
                    string fullPdfPath = Path.Combine(path_pdf, pdfFileName);
                    filePdf.SaveAs(fullPdfPath);

                    detail.File_excel = pdfFileName;
                }

                if (fileExcel != null && fileExcel.ContentLength > 0)
                {
                    string excelFileName = GenerateUniqueFileName("EXCEL", documentId, fileExcel.FileName);
                    string fullExcelPath = Path.Combine(path_excel, excelFileName);
                    fileExcel.SaveAs(fullExcelPath);
                }
            }
        }

        private string GenerateUniqueFileName(string prefix, int documentId, string originalFileName)
        {
            string ext = Path.GetExtension(originalFileName);
            string baseName = Path.GetFileNameWithoutExtension(originalFileName);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string guid = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"{prefix}_{documentId}_{baseName}_{timestamp}_{guid}{ext}";
        }

        private void CreateApprovalWorkflow(int documentId)
        {
            // Create initial approval steps
            // Implementation depends on your approval workflow requirements
            var initialStep = new
            {
                DocumentId = documentId,
                Step = 1,
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };
            // Add to appropriate approval table
        }

        private bool CanEditDocument(DocumentList document)
        {
            var currentUserId = GetCurrentUserId();

            // Allow edit if user is the requester and status is still draft or pending
            return document.Requester_id == currentUserId &&
                   (document.Status == DocumentStatus.Draft || document.Status == DocumentStatus.PendingDeptHead);
        }

        private bool CanDeleteDocument(DocumentList document)
        {
            var currentUserId = GetCurrentUserId();

            // Allow delete if user is the requester and status is draft
            return document.Requester_id == currentUserId && document.Status == DocumentStatus.Draft;
        }

        private void UpdateDocumentRecord(DocumentList document, DocumentFormViewModel model)
        {
            document.Request_type = model.Request_type;
            document.Document_type = model.Document_type;
            document.Effective_date = model.Effective_date;
            document.Updated_at = DateTime.Now;
        }

        private void UpdateDocumentDetails(DocumentFormViewModel model, int documentId)
        {
            // Remove existing details
            var existingDetails = db.DocumentDetails.Where(d => d.Doc_id == documentId).ToList();
            db.DocumentDetails.RemoveRange(existingDetails);

            // Add updated details
            ProcessDocumentDetails(model, documentId);
        }

        private void UpdateSelectedAreas(DocumentFormViewModel model, int documentId)
        {
            // Update selected areas
            ProcessSelectedAreas(model, documentId);
        }

        [HttpGet]
        public JsonResult GetWSData()
        {
            try
            {
                var wsData = db.WSR_WorkingStandardEdit
                    .Select(ws => new
                    {
                        ws.WS_Id,
                        ws.WS_Name,
                        ws.WS_Number
                    })
                    .OrderBy(ws => ws.WS_Number)
                    .ToList();

                return Json(wsData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}