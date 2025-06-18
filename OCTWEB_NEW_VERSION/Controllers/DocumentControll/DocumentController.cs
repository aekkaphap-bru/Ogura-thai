using Microsoft.AspNet.Identity;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;

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
                    // Reload available areas if model is invalid
                    model.AvailableAreas = LoadAvailableAreas();
                    return View(model);
                }

                // Validate business rules
                if (!ValidateDocumentRequest(model))
                {
                    model.AvailableAreas = LoadAvailableAreas();
                    return View(model);
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Create main document record
                        var document = CreateDocumentRecord(model);
                        db.DocumentLists.Add(document);
                        db.SaveChanges();

                        // Process document details
                        ProcessDocumentDetails(model, document.LId);

                        // Process selected areas
                        ProcessSelectedAreas(model, document.LId);

                        // Process file uploads
                        ProcessFileUploads(model, document.LId);

                        // Create approval workflow
                        CreateApprovalWorkflow(document.LId);

                        db.SaveChanges();
                        transaction.Commit();

                        TempData["SuccessMessage"] = "บันทึกคำร้องเรียบร้อยแล้ว หมายเลขคำร้อง: " + document.LId;
                        return RedirectToAction("List");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + ex.Message);
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

                        // Update document details
                        UpdateDocumentDetails(model, document.LId);

                        // Update selected areas
                        UpdateSelectedAreas(model, document.LId);

                        // Process new file uploads
                        ProcessFileUploads(model, document.LId);

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
                            Num_pages = detail.Num_pages?.Trim(),
                            Num_copies = detail.Num_copies ?? 1,
                            File_excel = detail.File_excel,
                            File_pdf = detail.File_pdf,
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

        private void ProcessFileUploads(DocumentFormViewModel model, int documentId)
        {
            // Implementation for file uploads
            // This would handle saving uploaded files to server and updating file paths in database
            var uploadPath = Server.MapPath("~/Uploads/Documents/" + documentId);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Process each document detail's files
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);
                    file.SaveAs(filePath);

                    // Update database with file path
                    // Implementation depends on how you want to store file references
                }
            }
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

        //private DocumentFormViewModel MapDocumentToViewModel(DocumentList document)
        //{
        //    var model = new DocumentFormViewModel
        //    {
        //        Id = document.LId,
        //        Request_from = document.Request_from,
        //        Request_type = document.Request_type,
        //        Requester_id = document.Requester_id,
        //        Document_type = document.Document_type,
        //        Effective_date = document.Effective_date,
        //        Status = document.Status,
        //        Created_at = document.Created_at,
        //        Updated_at = document.Updated_at
        //    };

        //    // Map document details
        //    if (document.DocumentDetails != null)
        //    {
        //        model.DocumentDetails = document.DocumentDetails.Select(d => new DocumentDetailViewModel
        //        {
        //            Id = d.Id,
        //            WS_number = d.WS_number,
        //            WS_name = d.WS_name,
        //            Revision = d.Revision,
        //            Num_pages = d.Num_pages,
        //            Num_copies = d.Num_copies,
        //            File_excel = d.File_excel,
        //            File_pdf = d.File_pdf,
        //            Change_detail = d.Change_detail
        //        }).ToList();
        //    }

        //    // Load available areas
        //    model.AvailableAreas = LoadAvailableAreas();

        //    return model;
        //}

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