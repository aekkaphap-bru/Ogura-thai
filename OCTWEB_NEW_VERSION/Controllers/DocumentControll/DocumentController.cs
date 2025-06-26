using Microsoft.AspNet.Identity;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult List(string searchString, string statusFilter, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = statusFilter;

            var query = from d in db.DocumentLists
                        join u in db.UserDetails on d.Requester_id equals u.USE_Id into du
                        from u in du.DefaultIfEmpty()
                        join dd in db.DocumentDetails on d.LId equals dd.LId into ddGroup
                        select new DocumentListDisplayViewModel
                        {
                            LId = d.LId,
                            Created_at = d.Created_at,
                            Status = d.Status,
                            WS_name = ddGroup.Select(x => x.WS_name).FirstOrDefault(),
                            WS_number = ddGroup.Select(x => x.WS_number).FirstOrDefault(),
                            RequesterName = (u.USE_FName + " " + u.USE_LName).Trim()
                        };

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
d.LId.ToString().Contains(searchString) ||
                  d.WS_name.Contains(searchString) ||
                  d.RequesterName.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(d => d.Status == statusFilter);
            }

            query = query.OrderByDescending(d => d.Created_at);

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(query.ToPagedList(pageNumber, pageSize));
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
                    Effective_date = DateTime.Now.AddDays(7),
                    DocumentDetails = new List<DocumentDetailViewModel>
                      {
                          new DocumentDetailViewModel()
                      }
                };
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
                        var document = CreateDocumentRecord(model);
                        db.DocumentLists.Add(document);
                        db.SaveChanges();

                        ProcessFileUploads(model, document.LId);
                        ProcessDocumentDetails(model, document.LId);

                        ProcessSelectedAreas(model, document.LId);

                        CreateApprovalWorkflow(document.LId);

                        db.SaveChanges();
                        transaction.Commit();

                        Console.WriteLine($"Saving detail: PDF={model.File_pdf}, Excel={model.File_excel}");
                        return Json(new { success = true, message = $"ส่งคำร้องเรียบร้อยแล้ว หมายเลขคำร้อง: {document.LId}", redirectUrl = Url.Action("List", "Document") });
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(eve => eve.ValidationErrors)
                            .Select(e => $"Property: {e.PropertyName}, Error: {e.ErrorMessage}")
                            .ToList();

                        var fullErrorMessage = string.Join(" | ", errorMessages);

                        Console.WriteLine("Entity Validation Errors:\n" + fullErrorMessage);

                        Response.StatusCode = 500;
                        return Json(new
                        {
                            success = false,
                            message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + fullErrorMessage
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in Create action: {ex.Message}");
                Console.WriteLine($"Saving detail: PDF={model.File_pdf}, Excel={model.File_excel}");

                Response.StatusCode = 500;
                return Json(new { success = false, message = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง: " + ex.Message + "Saving detail: PDF" + model.File_pdf + "Excel" + model.File_excel });
            }
        }

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
                        UpdateDocumentRecord(document, model);

                        ProcessFileUploads(model, document.LId);
                        UpdateDocumentDetails(model, document.LId);

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

        // POST: Document/Dtails/5
        public ActionResult Details(int id)
        {
            try
            {
                var documentWithDetails = (from doc in db.DocumentLists
                                           where doc.LId == id
                                           join detail in db.DocumentDetails on doc.LId equals detail.LId into detailsGroup
                                           join area in db.DocumentFormAreas on doc.LId equals area.LId into areasGroup
                                           join user in db.UserDetails on (int?)doc.Requester_id equals user.USE_Usercode into userGroup
                                           from userDetail in userGroup.DefaultIfEmpty()
                                               // join เพิ่มเติมสำหรับ Section
                                           select new
                                           {
                                               Document = doc,
                                               Details = detailsGroup,
                                               Areas = (from a in areasGroup
                                                        join s in db.WS_Training_Section on a.WS_TS_Id equals s.Id
                                                        select new
                                                        {
                                                            a.FId,
                                                            a.LId,
                                                            a.WS_TS_Id,
                                                            s.SectionName,
                                                            s.SectionCode
                                                        }),
                                               Requester = userDetail
                                           }).FirstOrDefault();
                if (documentWithDetails == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                    return RedirectToAction("List");
                }

                var model = new DocumentFormViewModel
                {
                    Id = documentWithDetails.Document.LId,
                    Request_type = documentWithDetails.Document.Request_type,
                    Document_type = documentWithDetails.Document.Document_type,
                    Effective_date = documentWithDetails.Document.Effective_date,
                    Status = documentWithDetails.Document.Status,
                    Request_from = documentWithDetails.Document.Request_from,
                    Requester_id = documentWithDetails.Document.Requester_id,
                    Created_at = documentWithDetails.Document.Created_at?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("en-US")),
                    Requester_name = documentWithDetails.Requester != null
                                     ? documentWithDetails.Requester.USE_FName + " " + documentWithDetails.Requester.USE_LName
                                     : "ไม่พบข้อมูลผู้ร้องขอ",
                    DocumentDetails = documentWithDetails.Details.Select(dd => new DocumentDetailViewModel
                    {
                        WS_number = dd.WS_number,
                        WS_name = dd.WS_name,
                        Revision = dd.Revision,
                        Num_pages = dd.Num_pages,
                        Num_copies = dd.Num_copies,
                        File_excel = dd.File_excel,
                        File_pdf = dd.File_pdf,
                        Change_detail = dd.Change_detail
                    }).ToList(),
                    AvailableAreas = documentWithDetails.Areas.Select(a => new AreaItemViewModel
                    {
                        FId = a.FId,
                        LId = a.LId,
                        WS_TS_Id = a.WS_TS_Id?? 0,
                        SectionName = a.SectionName,
                        SectionCode = a.SectionCode
                    }).ToList()

                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาด: " + ex.Message;
                return RedirectToAction("List");
            }
        }

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
                        var details = db.DocumentDetails.Where(d => d.LId == id).ToList();
                        db.DocumentDetails.RemoveRange(details);
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

            if (model.AvailableAreas == null || !model.AvailableAreas.Any(a => a.IsSelected))
            {
                ModelState.AddModelError("AvailableAreas", "กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ");
                isValid = false;
            }

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
                            LId = documentId,
                            WS_number = detail.WS_number.Trim(),
                            WS_name = detail.WS_name.Trim(),
                            Revision = detail.Revision?.Trim() ?? "01",
                            Num_pages = detail.Num_pages?.Trim(),
                            Num_copies = detail.Num_copies ?? 1,
                            File_excel = detail.File_excel ?? "Error",
                            File_pdf = detail.File_pdf ?? "Error",
                            Change_detail = detail.Change_detail?.Trim()
                        };
                        db.DocumentDetails.Add(docDetail);
                        Console.WriteLine($"Saving detail: PDF={detail.File_pdf}, Excel={detail.File_excel}");

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

                if (model.AvailableAreas != null)
                {
                    var selectedAreas = model.AvailableAreas
                                            .Where(a => a.IsSelected)
                          .Select(a => new DocumentFormArea
                          {
                              LId = documentId,
                              WS_TS_Id = a.Id
                          }).ToList();

                    db.DocumentFormAreas.AddRange(selectedAreas);
                    db.SaveChanges();
                }

            }
        }
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
                    model.DocumentDetails[i].File_pdf = pdfFileName;


                }

                if (fileExcel != null && fileExcel.ContentLength > 0)
                {
                    string excelFileName = GenerateUniqueFileName("EXCEL", documentId, fileExcel.FileName);
                    string fullExcelPath = Path.Combine(path_excel, excelFileName);
                    fileExcel.SaveAs(fullExcelPath);
                    model.DocumentDetails[i].File_excel = excelFileName;
                }
            }
        }
        private string GenerateUniqueFileName(string prefix, int documentId, string originalFileName)
        {
            string ext = Path.GetExtension(originalFileName);
            string baseName = Path.GetFileNameWithoutExtension(originalFileName);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string guid = Guid.NewGuid().ToString("N").Substring(0, 6);

            int maxLength = 50;
            int staticLength = prefix.Length + 1 + documentId.ToString().Length + 1 + timestamp.Length + 1 + guid.Length;
            int allowedBaseNameLength = maxLength - staticLength;

            if (allowedBaseNameLength < 1)
                allowedBaseNameLength = 1;

            if (baseName.Length > allowedBaseNameLength)
                baseName = baseName.Substring(0, allowedBaseNameLength);

            return $"{prefix}_{documentId}_{baseName}_{timestamp}_{guid}{ext}";
        }
        private void CreateApprovalWorkflow(int documentId)
        {
            var initialStep = new
            {
                DocumentId = documentId,
                Step = 1,
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };
        }
        private bool CanEditDocument(DocumentList document)
        {
            var currentUserId = GetCurrentUserId();

            return document.Requester_id == currentUserId &&
                   (document.Status == DocumentStatus.Draft || document.Status == DocumentStatus.PendingDeptHead);
        }
        private bool CanDeleteDocument(DocumentList document)
        {
            var currentUserId = GetCurrentUserId();

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
            var existingDetails = db.DocumentDetails.Where(d => d.LId == documentId).ToList();
            db.DocumentDetails.RemoveRange(existingDetails);
            ProcessDocumentDetails(model, documentId);
        }
        private void UpdateSelectedAreas(DocumentFormViewModel model, int documentId)
        {
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