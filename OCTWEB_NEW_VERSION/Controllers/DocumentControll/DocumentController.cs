using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace OCTWEB_NET45.Controllers.DocumentControll
{

    public class ApproverConfigurationException : Exception
    {
        public string ErrorCode { get; }

        public ApproverConfigurationException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ApproverConfigurationException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    //[Authorize]
    [CustomAuthorize(73)]
    public class DocumentController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        #region Status Helper Methods
        public static class DocumentTypes
        {
            public const string Common = "Common";
            public const string ISO = "ISO 14001";
        }
        public static class DocumentStatus
        {
            public const string Editing = "EDITING";
            public const string PendingApproval = "PENDING APPROVAL";
            public const string Complete = "COMPLETE";
            public const string Rejected = "REJECTED";
        }
        public static class StepStatus
        {
            public const string Pending = "PENDING";
            public const string Approved = "APPROVED";
            public const string Rejected = "REJECTED";
            public const string Waiting = "WAITING";
        }
        #endregion
        #region Document Listing

        /// <summary>
        /// Displays a paginated list of documents with optional search and status filters.
        /// </summary>
        /// <param name="searchString">Optional keyword for filtering document name, ID, or requester</param>
        /// <param name="statusFilter">Optional status filter (e.g., "COMPLETE", "EDITING")</param>
        /// <param name="page">Page number for pagination</param>
        public ActionResult List(string searchString, string statusFilter, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = statusFilter;

            // Define the base query to be filtered
            IQueryable<DocumentList> documentsQuery = db.DocumentLists.AsQueryable();

            if (Session["UserCode"] != null)
            {
                int usercode = Convert.ToInt32(Session["UserCode"].ToString());
                int usercodeId = db.UserDetails
                     .Where(u => u.USE_Usercode == usercode)
                     .Select(u => u.USE_Id)
                     .FirstOrDefault();

                // Check if the user has Right 78 for full access
                bool hasRight78 = db.UserRights.Any(r => r.USE_Id == usercodeId && r.RIH_Id == 78);

                if (!hasRight78)
                {
                    // Get the current user's department code
                    var userDeptCode = db.EmpLists
                                         .Where(e => e.EmpID == usercode)
                                         .Select(e => e.DeptCode)
                                         .FirstOrDefault();

                    if (userDeptCode != null)
                    {
                        // Special case for user 2385 (	Mr.Takashi Ebara)
                        if (usercode == 2385)
                        {
                            var allowedDepts = new List<string> { userDeptCode, "A005" };
                            documentsQuery = documentsQuery.Where(d =>
                                db.EmpLists.Where(e => e.EmpID == d.Requester_id)
                                           .Any(e => allowedDepts.Contains(e.DeptCode)));
                        }
                        else
                        {
                            // Standard user: filter by their own department
                            documentsQuery = documentsQuery.Where(d =>
                                db.EmpLists.Where(e => e.EmpID == d.Requester_id)
                                           .Any(e => e.DeptCode == userDeptCode));
                        }
                    }
                    else
                    {
                        // If user has no department, they see no documents
                        documentsQuery = documentsQuery.Where(d => false);
                    }
                }
                // If user has Right 78, no department filter is applied, showing all documents.
            }
            else
            {
                // No user in session, return an empty list
                documentsQuery = documentsQuery.Where(d => false);
            }

            // Join the filtered documents with other tables to create the view model
            var query = from d in documentsQuery
                        join u in db.UserDetails on d.Requester_id equals u.USE_Id into du
                        from u in du.DefaultIfEmpty()
                        join dd in db.DocumentDetails on d.LId equals dd.LId into ddGroup
                        select new DocumentListDisplayViewModel
                        {
                            LId = d.LId,
                            DarNumber = d.DarNumber,
                            Created_at = d.Created_at,
                            Update_at = d.Updated_at,
                            Status = d.Status,
                            WS_name = ddGroup.Select(x => x.WS_name).FirstOrDefault(),
                            WS_number = ddGroup.Select(x => x.WS_number).FirstOrDefault(),
                            RequesterName = (u.USE_FName + " " + u.USE_LName).Trim()
                        };

            // Apply search string filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
                    d.LId.ToString().Contains(searchString) ||
                    d.DarNumber.ToString().Contains(searchString) ||
                    d.WS_name.Contains(searchString) ||
                    d.WS_number.Contains(searchString));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(d => d.Status == statusFilter);
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Order and paginate the final result
            var pagedList = query
                .OrderByDescending(d => d.Created_at)
                .ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        #endregion
        #region Document Creation - GET

        /// <summary>
        /// Displays the document creation form with default values and dropdowns.
        /// </summary>
        public ActionResult Create()
        {
            try
            {
                var model = new DocumentFormViewModel
                {
                    Request_from = DocumentTypes.Common,
                    Requester_id = GetCurrentUserId(),
                    Effective_date = DateTime.Now.AddDays(3),
                    DocumentDetails = new List<DocumentDetailViewModel> { new DocumentDetailViewModel() },
                    AvailableAreas = LoadAvailableAreas(),
                    RequestTypes = GetRequestTypes()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading form: " + ex.Message;
                return View(new DocumentFormViewModel());
            }
        }

        #endregion
        #region Document Creation - POST

        /// <summary>
        /// Handles the submission of a new document request, validates input, creates records, and triggers notifications.
        /// </summary>
        /// <param name="model">Document form view model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DocumentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid || !ValidateDocumentRequest(model))
                {
                    Response.StatusCode = 400;
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "Invalid submission. Please review and try again.",
                        errors
                    });
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
                        CreateApprovalWorkflow(document.LId, document.Requester_id);
                        db.SaveChanges();

                        transaction.Commit();

                        NotifyApproversOfNewRequest(document.LId, document.Requester_id);

                        return Json(new
                        {
                            success = true,
                            message = $"Request submitted successfully. Your request number is: {document.LId}",
                            redirectUrl = Url.Action("List", "Document")
                        });
                    }
                    catch (ApproverConfigurationException ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"[Config Error] Create failed: {ex.Message}");
                        Response.StatusCode = 400;

                        return Json(new
                        {
                            success = false,
                            errorCode = ex.ErrorCode,
                            message = $"Approval setup failed: {ex.Message}"
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"[Error] Unexpected error in Create: {ex}");
                        Response.StatusCode = 500;

                        return Json(new
                        {
                            success = false,
                            errorCode = "ERR_UNEXPECTED",
                            message = "Unexpected error. Please try again later."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Unexpected error: " + ex.Message });
            }
        }

        /// <summary>
        /// Notifies approvers with RIH_Id 74 in the same department (Advisor/Manager/Asst. Manager).
        /// </summary>
        private void NotifyApproversOfNewRequest(int documentId, int requesterId)
        {
            var currentUserId = GetCurrentUserId();

            var currentEmpInfo = db.UserDetails
                .Join(db.EmpLists, u => u.USE_Usercode, e => e.EmpID, (u, e) => new { u, e })
                .Where(x => x.u.USE_Usercode == currentUserId)
                .Select(x => new
                {
                    UserId = x.u.USE_Id,
                    DeptCode = x.e.DeptCode,
                    Position = x.e.Position,
                    FirstName = x.u.USE_FName,
                    LastName = x.u.USE_LName,
                    Email = x.u.USE_Email,
                    Dept = x.e.DeptDesc
                }).FirstOrDefault();

            if (currentEmpInfo == null) return;

            var targetPositions = new[] { "Advisor", "Manager", "Asst. Manager" };

            var potentialRecipients = db.UserRights
                .Where(r => r.RIH_Id == 74)
                .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
                .Join(db.EmpLists, x => x.ud.USE_Usercode, e => e.EmpID, (x, e) => new { UserDetail = x.ud, Emp = e })
                .Where(x => targetPositions.Contains(x.Emp.Position) && x.UserDetail.USE_Id != currentUserId)
                .Where(x => x.Emp.DeptCode == currentEmpInfo.DeptCode)
                .Select(x => x.UserDetail.USE_Email)
                .Where(email => !string.IsNullOrEmpty(email))
                .Distinct()
                .ToList();

            if (!potentialRecipients.Any()) return;

            NotificationService.SendApprovalRequestEmail(
                to: potentialRecipients,
                subject: $"OCT - DocumentControl | A new Document Action Request Form has been submitted",
                cc: new List<string>(),
                LId: documentId,
                url: Url,
                request: Request
            );
        }

        #endregion
        #region Document Editing - GET

        /// <summary>
        /// Loads the existing document for editing, including its details and selected areas.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// 

        public ActionResult Edit(int id)
        {
            try
            {
                var document = db.DocumentLists.FirstOrDefault(d => d.LId == id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบคำร้องที่คุณต้องการ กรุณาติดต่อเจ้าหน้าที่";
                    return RedirectToAction("List");
                }

                if (document.Status == DocumentStatus.Complete)
                {
                    TempData["ErrorMessage"] = "คำร้องนี้ดำเนินการเสร็จสิ้นแล้ว ไม่สามารถแก้ไขได้";
                    return RedirectToAction("List");
                }

                if (!CanEditDocument(document))
                {
                    TempData["ErrorMessage"] = "คุณไม่ได้รับสิทธิ์ในการแก้ไขคำร้องนี้ กรุณาติดต่อผู้ดูแลระบบ";
                    return RedirectToAction("List");
                }

                if (document.Status != DocumentStatus.Editing)
                {
                    ViewBag.ResubmitWarningMessage = "เอกสารรออนุมัติอยู่ หากแก้ไขระบบจะเริ่มอนุมัติใหม่ทั้งหมด คุณต้องการดำเนินการต่อหรือไม่?";
                }

                var documentWithDetails = (
                    from doc in db.DocumentLists
                    where doc.LId == id
                    join detail in db.DocumentDetails on doc.LId equals detail.LId into detailsGroup
                    join area in db.DocumentFormAreas on doc.LId equals area.LId into areasGroup
                    join user in db.UserDetails on (int?)doc.Requester_id equals user.USE_Usercode into userGroup
                    from userDetail in userGroup.DefaultIfEmpty()
                    select new
                    {
                        Document = doc,
                        Details = detailsGroup,
                        Areas = areasGroup.Select(a => new
                        {
                            a.FId,
                            a.LId,
                            a.WS_TS_Id,
                            Section = db.DocumentSections
                                        .Where(s => s.Id == a.WS_TS_Id)
                                        .Select(s => new { s.SectionName, s.SectionCode })
                                        .FirstOrDefault()
                        }),
                        Requester = userDetail
                    }
                ).FirstOrDefault();

                if (documentWithDetails == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                    return RedirectToAction("List");
                }

                // Set approval back to waiting if in progress
                var existingStep = db.DocumentApprovalSteps.FirstOrDefault(s => s.LId == id);
                if (existingStep != null)
                {
                    existingStep.Status = StepStatus.Waiting;
                }

                document.Status = DocumentStatus.Editing;
                db.SaveChanges();

                var availableAreas = LoadAvailableAreas();
                var selectedAreaIds = documentWithDetails.Areas.Select(a => a.WS_TS_Id ?? 0).ToList();
                foreach (var area in availableAreas)
                {
                    area.IsSelected = selectedAreaIds.Contains(area.Id);
                }

                var model = new DocumentFormViewModel
                {
                    Id = document.LId,
                    Request_type = document.Request_type,
                    Document_type = document.Document_type,
                    Effective_date = document.Effective_date,
                    Request_from = document.Request_from,
                    Requester_id = document.Requester_id,
                    AvailableAreas = availableAreas,
                    RequestTypes = GetRequestTypes(),

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
                    }).ToList()
                };

                if (!model.DocumentDetails.Any())
                {
                    model.DocumentDetails.Add(new DocumentDetailViewModel());
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "เกิดข้อผิดพลาดในการโหลดข้อมูล: " + ex.Message;
                return View(new DocumentFormViewModel());
            }
        }

        #endregion
        #region Document Editing - POST

        /// <summary>
        /// Handles updates to an existing document, including file re-uploads and workflow reset.
        /// </summary>
        /// <param name="model">Updated document form model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DocumentFormViewModel model)
        {
            try
            {

                //if (!ModelState.IsValid)
                //{
                //    model.AvailableAreas = LoadAvailableAreas();
                //    return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบ" });
                //}

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
                        UpdateApprovalWorkflow(document.LId, document.Requester_id);

                        
                        db.SaveChanges();
                        transaction.Commit();

                        NotifyApproversOfNewRequest(document.LId, document.Requester_id);

                        TempData["SuccessMessage"] = "แก้ไขคำร้องเรียบร้อยแล้ว";
                        return Json(new { success = true, message = "Editing Successfull", redirectUrl = Url.Action("List") });

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = "เกิดข้อผิดพลาดในการแก้ไขข้อมูล: " + ex.Message });
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

        #endregion
        #region Document Deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // ใช้ Transaction เพื่อให้แน่ใจว่าการลบข้อมูลทั้งหมดจะสำเร็จพร้อมกัน
            // หากเกิดข้อผิดพลาดขึ้นระหว่างทาง ข้อมูลทั้งหมดจะถูก Rollback กลับสู่สถานะเดิม
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. ค้นหาเอกสารหลักที่ต้องการลบ
                    var document = db.DocumentLists.FirstOrDefault(d => d.LId == id);
                    if (document == null)
                    {
                        // ถ้าไม่พบเอกสาร ให้ส่งข้อความแจ้งเตือนกลับไป
                        return Json(new { success = false, message = "ไม่พบข้อมูลคำร้องที่ต้องการลบ" });
                    }

                    // 2. ลบข้อมูลจากตารางที่เกี่ยวข้องทั้งหมด (Child tables)
                    // ลายเซ็นอนุมัติ (Approval Steps)
                    var approvalSteps = db.DocumentApprovalSteps.Where(s => s.LId == id).ToList();
                    if (approvalSteps.Any())
                    {
                        db.DocumentApprovalSteps.RemoveRange(approvalSteps);
                    }

                    // พื้นที่การใช้งาน (Form Areas)
                    var formAreas = db.DocumentFormAreas.Where(a => a.LId == id).ToList();
                    if (formAreas.Any())
                    {
                        db.DocumentFormAreas.RemoveRange(formAreas);
                    }

                    // รายละเอียดเอกสาร (Document Details)
                    var details = db.DocumentDetails.Where(d => d.LId == id).ToList();
                    if (details.Any())
                    {
                        db.DocumentDetails.RemoveRange(details);
                    }

                    // 3. ลบข้อมูลจากตารางหลัก (Parent table)
                    db.DocumentLists.Remove(document);

                    // 4. บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูล
                    db.SaveChanges();

                    // 5. ยืนยันการทำรายการ (Commit Transaction)
                    transaction.Commit();

                    // ส่งผลลัพธ์ว่าการลบสำเร็จ
                    return Json(new { success = true, message = "ลบคำร้องเรียบร้อยแล้ว" });
                }
                catch (Exception ex)
                {
                    // หากเกิดข้อผิดพลาด ให้ยกเลิกการทำรายการทั้งหมด
                    transaction.Rollback();

                    // สามารถเพิ่มการบันทึก Log ของ Exception ได้ที่นี่
                    // System.Diagnostics.Debug.WriteLine(ex.ToString());

                    // ส่งข้อความแจ้งเตือนข้อผิดพลาดกลับไป
                    return Json(new { success = false, message = "เกิดข้อผิดพลาดในการลบข้อมูล: " + ex.Message });
                }
            }
        }
        #endregion
        #region Document Details View

        /// <summary>
        /// Displays the full detail view of a document, including approval steps and reviewer information.
        /// </summary>
        /// <param name="id">Document ID</param>
        public ActionResult Details(int id)
        {
            var document = db.DocumentLists.Find(id);
            if (document == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                return RedirectToAction("List");
            }

            var documentWithDetails = (
                from doc in db.DocumentLists
                where doc.LId == id
                join detail in db.DocumentDetails on doc.LId equals detail.LId into detailsGroup
                join area in db.DocumentFormAreas on doc.LId equals area.LId into areasGroup
                join user in db.UserDetails on (int?)doc.Requester_id equals user.USE_Usercode into userGroup
                from userDetail in userGroup.DefaultIfEmpty()
                select new
                {
                    Document = doc,
                    Details = detailsGroup,
                    Areas = areasGroup.Select(a => new
                    {
                        a.FId,
                        a.LId,
                        a.WS_TS_Id,
                        Section = db.DocumentSections
                                    .Where(s => s.Id == a.WS_TS_Id)
                                    .Select(s => new { s.SectionName, s.SectionCode })
                                    .FirstOrDefault()
                    }),
                    Requester = userDetail
                }
            ).FirstOrDefault();

            if (documentWithDetails == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                return RedirectToAction("List");
            }

            var approvalSteps = (
                from step in db.DocumentApprovalSteps
                where step.LId == id
                join approver in db.UserDetails on step.Approver_id equals approver.USE_Id into approverGroup
                from approver in approverGroup.DefaultIfEmpty()
                orderby step.Step
                select new
                {
                    step.AId,
                    step.LId,
                    step.Step,
                    step.Approver_id,
                    ApproverFName = approver.USE_FName,
                    ApproverLName = approver.USE_LName,
                    step.Approved_at,
                    step.Status,
                    step.Comment
                }
            ).AsEnumerable()
            .Select(s => new ApprovalStepViewModel
            {
                AId = s.AId,
                LId = s.LId,
                Step = s.Step ?? 0,
                Approver_id = s.Approver_id,
                ApproverName = $"{s.ApproverFName} {s.ApproverLName}".Trim(),
                Approved_at = s.Approved_at,
                Status = s.Status,
                Comment = s.Comment
            })
            .ToList();


            foreach (var step in approvalSteps)
                step.StepName = GetStepName(step.Step);

            // Current User Info
            string userDep = null;
            string userPosition = null;

            if (Session["UserCode"] is string userCodeStr && int.TryParse(userCodeStr, out int userCode))
            {
                var userInfo = db.EmpLists
                    .Where(e => e.EmpID == userCode)
                    .Select(e => new { e.DeptCode, e.Position })
                    .FirstOrDefault();

                if (userInfo != null)
                {
                    userDep = userInfo.DeptCode;
                    userPosition = userInfo.Position;
                }
            }

            var requiredPositions = new[] { "Advisor", "Manager", "Asst. Manager" };
            bool isCorrectPosition = requiredPositions.Contains(userPosition);

            // Map to ViewModel
            var model = new DocumentFormViewModel
            {
                Id = documentWithDetails.Document.LId,
                Request_type = documentWithDetails.Document.Request_type,
                Document_type = documentWithDetails.Document.Document_type,
                Effective_date = documentWithDetails.Document.Effective_date,
                Request_from = documentWithDetails.Document.Request_from,
                Requester_id = documentWithDetails.Document.Requester_id,
                Created_at = documentWithDetails.Document.Created_at?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("en-US")),
                Requester_name = documentWithDetails.Requester != null
                                    ? $"{documentWithDetails.Requester.USE_FName} {documentWithDetails.Requester.USE_LName}"
                                    : "ไม่พบข้อมูลผู้ร้องขอ",
                Status = document.Status,
                CurrentUserId = GetCurrentUserId(),
                CurrentUserPosition = isCorrectPosition,
                ApprovalSteps = approvalSteps,
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
                    WS_TS_Id = a.WS_TS_Id ?? 0,
                    SectionName = a.Section?.SectionName,
                    SectionCode = a.Section?.SectionCode
                }).ToList()
            };

            return View(model);
        }

        #endregion
        #region Approval Workflow Processing

        /// <summary>
        /// Processes an approval or rejection action for a document at a specific step.
        /// </summary>
        /// <param name="model">Approval request input from approver</param>
        [HttpPost]
        public ActionResult ProcessApproval(ApprovalRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = $"Invalid input data: {errors}" });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var currentUserId = GetCurrentUserId();

                    var currentStep = db.DocumentApprovalSteps.FirstOrDefault(s => s.AId == model.StepAId && s.LId == model.DocumentId);
                    if (currentStep == null)
                        return Json(new { success = false, message = "Approval step not found." });

                    var document = db.DocumentLists.Find(model.DocumentId);
                    var documentdetail = db.DocumentDetails.FirstOrDefault(d => d.LId == model.DocumentId);

                    if (document == null)
                        return Json(new { success = false, message = "Document not found." });

                    var requester = db.UserDetails.FirstOrDefault(u => u.USE_Usercode == document.Requester_id);
                    var requesterEmail = requester?.USE_Email;

                    if (!IsUserAuthorizedToApprove(currentStep, currentUserId, document.Requester_id))
                    {
                        return Json(new { success = false, message = "You are not authorized to approve this step." });
                    }

                    if (currentStep.Status != StepStatus.Pending)
                    {
                        return Json(new { success = false, message = "This step is not in a pending state." });
                    }

                    //if (model.Action.ToLower() == "reject")
                    //{
                    //    if (string.IsNullOrWhiteSpace(model.Comment))
                    //        return Json(new { success = false, message = "Please provide a comment for rejection." });

                    //    RejectDocument(document, currentStep, model.Comment);
                    //    db.SaveChanges();
                    //    transaction.Commit();

                    //    if (!string.IsNullOrEmpty(requesterEmail))
                    //    {
                    //        NotificationService.SendSimpleEmail(
                    //            to: new List<string> { requesterEmail },
                    //            subject: $"[OCT] Your document request was REJECTED",
                    //            body: $"Your request has been rejected at step {currentStep.Step}. Please revise and resubmit."
                    //        );
                    //    }

                    //    return Json(new { success = true, message = "The request has been rejected and sent back for revision." });
                    //}

                    if (model.Action.ToLower() == "reject")
                    {
                        if (string.IsNullOrWhiteSpace(model.Comment))
                            return Json(new { success = false, message = "Please provide a comment for rejection." });

                        RejectDocument(document, currentStep, model.Comment);
                        db.SaveChanges();
                        transaction.Commit();

                        NotificationService.NotifyAfterReject(document, documentdetail, currentStep, requesterEmail, Url, Request);

                        return Json(new { success = true, message = "The request has been rejected and sent back for revision." });
                    }


                    // Handle Approval
                    ApproveStep(document, documentdetail, currentStep, model, currentUserId);
                    db.SaveChanges();
                    transaction.Commit();

                    NotificationService.NotifyAfterApproval(document, documentdetail, currentStep, requesterEmail, Url, Request);

                    NotificationService.NotifyProcessReviewTeamsIfNeeded(document, documentdetail, Url, Request);

                    return Json(new { success = true, message = "Approval completed successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
        }

        /// <summary>
        /// Rejects the document and resets all steps to WAITING, except the current one to REJECTED.
        /// </summary>
        private void RejectDocument(DocumentList document, DocumentApprovalStep currentStep, string comment)
        {
            var allSteps = db.DocumentApprovalSteps.Where(s => s.LId == document.LId).ToList();
            foreach (var step in allSteps)
            {
                if (step.AId == currentStep.AId)
                {
                    step.Status = StepStatus.Rejected;
                    step.Comment = comment;
                    step.Approved_at = DateTime.Now;
                }
                else
                {
                    step.Status = StepStatus.Waiting;
                    step.Comment = null;
                    step.Approved_at = null;
                }
            }

            document.Status = DocumentStatus.Rejected;
        }

        /// <summary>
        /// Approves the current step and transitions the workflow to the next step or completion.
        /// </summary>
        private void ApproveStep(DocumentList document, DocumentDetail documentDetail, DocumentApprovalStep currentStep, ApprovalRequestViewModel model, int approverId)
        {
            currentStep.Status = StepStatus.Approved;
            currentStep.Comment = model.Comment;
            currentStep.Approved_at = DateTime.Now;

            if (currentStep.Step == 1 && currentStep.Approver_id == 0)
            {
                currentStep.Approver_id = approverId;
            }

            var nextStep = db.DocumentApprovalSteps
                .Where(s => s.LId == document.LId && s.Step > currentStep.Step)
                .OrderBy(s => s.Step)
                .FirstOrDefault();

            if (nextStep != null)
            {
                nextStep.Status = StepStatus.Pending;
                document.Status = DocumentStatus.PendingApproval;
            }
            else
            {
                document.Status = DocumentStatus.Complete;
            }

            if (currentStep.Step == 3)
            {
                document.FMEAReview = model.FMEA_Review ?? false;
                document.ControlPlanReview = model.ControlPlan_Review ?? false;
                document.ProcessFlowReview = model.ProcessFlow_Review ?? false;
            }

            if (document.Status == DocumentStatus.Complete)
            {
                AssignDarNumber(document);
            }
        }

        /// <summary>
        /// Assigns a DAR number after all approvals are complete.
        /// </summary>
        private void AssignDarNumber(DocumentList document)
        {
            var currentYear = DateTime.Now.Year.ToString();
            var lastDar = db.DocumentLists
                .Where(d => d.DarNumber.StartsWith(currentYear + "-"))
                .OrderByDescending(d => d.DarNumber)
                .Select(d => d.DarNumber)
                .FirstOrDefault();

            int nextSequence = 1;
            if (!string.IsNullOrEmpty(lastDar))
            {
                var lastSeqStr = lastDar.Substring(5);
                if (int.TryParse(lastSeqStr, out int lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }

            document.DarNumber = $"{currentYear}-{nextSequence.ToString("D3")}";
        }

        /// <summary>
        /// Sends emails to the requester and the next approver if applicable.
        /// </summary>       
        


        /// <summary>
        /// Determines whether the current user is authorized to approve the specified step.
        /// </summary>
        private bool IsUserAuthorizedToApprove(DocumentApprovalStep step, int userId, int requesterId)
        {
            var approver = db.EmpLists.FirstOrDefault(e => e.EmpID == userId);
            var requester = db.EmpLists.FirstOrDefault(e => e.EmpID == requesterId);

            if (approver == null || requester == null) return false;

            if (step.Step == 1)
            {
                bool hasPermission = db.UserRights.Any(r =>
                    r.RIH_Id == 74 &&
                    db.UserDetails.Any(u => u.USE_Id == r.USE_Id && u.USE_Usercode == userId));

                var validPositions = new[] { "Advisor", "Manager", "Asst. Manager" };
                bool correctPosition = validPositions.Contains(approver.Position);
                bool sameDepartment = approver.DeptCode == requester.DeptCode;
                bool specialException = approver.DeptCode == "F101" && requester.DeptCode == "A005";

                return hasPermission && correctPosition && (sameDepartment || specialException);
            }
            else
            {
                return step.Approver_id == userId;
            }
        }
        #endregion
        #region File Upload Handling

        /// <summary>
        /// Handles file uploads for PDF and Excel files related to document details.
        /// </summary>
        /// <param name="model">Document form view model</param>
        /// <param name="documentId">Associated Document ID</param>
        private void ProcessFileUploads(DocumentFormViewModel model, int documentId)
        {
            string pathExcel = ConfigurationManager.AppSettings["path_Document_Excel"];
            string pathPdf = ConfigurationManager.AppSettings["path_Document_Pdf"];

            Directory.CreateDirectory(pathExcel);
            Directory.CreateDirectory(pathPdf);

            var files = Request.Files;

            for (int i = 0; i < model.DocumentDetails.Count; i++)
            {
                var detail = model.DocumentDetails[i];

                var pdfKey = $"DocumentDetails[{i}].File_pdf";
                var excelKey = $"DocumentDetails[{i}].File_excel";

                var filePdf = files[pdfKey];
                var fileExcel = files[excelKey];

                if (filePdf != null && filePdf.ContentLength > 0)
                {
                    string pdfFileName = GenerateUniqueFileName("PDF", documentId, filePdf.FileName);
                    filePdf.SaveAs(Path.Combine(pathPdf, pdfFileName));
                    model.DocumentDetails[i].File_pdf = pdfFileName;
                }

                if (fileExcel != null && fileExcel.ContentLength > 0)
                {
                    string excelFileName = GenerateUniqueFileName("EXCEL", documentId, fileExcel.FileName);
                    fileExcel.SaveAs(Path.Combine(pathExcel, excelFileName));
                    model.DocumentDetails[i].File_excel = excelFileName;
                }
            }
        }

        /// <summary>
        /// Generates a unique file name for uploaded files to avoid overwrites and support traceability.
        /// </summary>
        private string GenerateUniqueFileName(string prefix, int documentId, string originalFileName)
        {
            string ext = Path.GetExtension(originalFileName);
            string baseName = Path.GetFileNameWithoutExtension(originalFileName);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string guid = Guid.NewGuid().ToString("N").Substring(0, 6);

            int maxLength = 50;
            int staticLength = $"{prefix}_{documentId}_{timestamp}_{guid}".Length;
            int allowedBaseNameLength = Math.Max(1, maxLength - staticLength);

            if (baseName.Length > allowedBaseNameLength)
                baseName = baseName.Substring(0, allowedBaseNameLength);

            return $"{prefix}_{documentId}_{baseName}_{timestamp}_{guid}{ext}";
        }

        #endregion
        #region Document Export (Excel)

        /// <summary>
        /// Generates and exports a filled Excel file based on the document data.
        /// </summary>
        /// <param name="id">Document ID</param>
        public ActionResult ExportDarLog(int id)
        {
            try
            {
                string tempDirectory = ConfigurationManager.AppSettings["path_Document_Templat"];
                Directory.CreateDirectory(tempDirectory);

                string uniqueFileName = Guid.NewGuid().ToString();
                string templatePath = Path.Combine(tempDirectory, "DocumentFormTemplate.xlsx");
                string outputExcelPath = Path.Combine(tempDirectory, $"DocumentForm_{uniqueFileName}.xlsx");

                var rawData = GetRawData(id);
                if (rawData == null)
                    return HttpNotFound($"ไม่พบข้อมูลเอกสารที่ระบุ (ID: {id})");

                var viewModel = MapToViewModel(rawData);

                var details = db.DocumentDetails.Where(d => d.LId == id).ToList();
                var sectionCodes = GetSectionCodes(id);

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    FillStaticData(worksheet, viewModel);
                    FillRequestType(worksheet, viewModel.Request_type);
                    FillDocumentType(worksheet, viewModel.Document_type);
                    FillReviewChecks(worksheet, viewModel);
                    FillFileExistChecks(worksheet, details);
                    FillSectionCodes(worksheet, sectionCodes);
                    FillApprovers(worksheet, viewModel.Approvers, viewModel);
                    FillDetails(worksheet, details);

                    var pageSetup = worksheet.PageSetup;
                    pageSetup.PaperSize = XLPaperSize.A4Paper;
                    pageSetup.PageOrientation = XLPageOrientation.Landscape;
                    pageSetup.Scale = 100;

                    worksheet.Protect("1!supp0rtf0rex!64");

                    workbook.SaveAs(outputExcelPath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(outputExcelPath);
                    System.IO.File.Delete(outputExcelPath);

                    return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DAR_Log_Sheet_{viewModel.DarNo ?? "N-A"}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return Content("เกิดข้อผิดพลาด: " + ex.ToString());
            }
        }

        #endregion
        #region Area & User Helpers
        /// <summary>
        /// Loads all available document sections to be selected in the form.
        /// </summary>
        private List<AreaItemViewModel> LoadAvailableAreas()
        {
            return db.DocumentSections
                     .Select(s => new AreaItemViewModel
                     {
                         Id = s.Id,
                         SectionCode = s.SectionCode,
                         SectionName = s.SectionName,
                         IsSelected = false
                     })
                     .OrderBy(a => a.SectionCode)
                     .ToList();
        }

        /// <summary>
        /// Retrieves the currently logged-in user's ID from session.
        /// </summary>
        private int GetCurrentUserId()
        {
            if (Session["UserCode"] != null)
                return Convert.ToInt32(Session["UserCode"]);

            throw new UnauthorizedAccessException("ไม่พบข้อมูลผู้ใช้งาน");
        }


        /// <summary>
        /// Checks whether the user can edit a document based on ownership and status.
        /// </summary>
        private bool CanEditDocument(DocumentList document)
        {
            try
            {
                var userId = GetCurrentUserId();
                return document.Requester_id == userId &&
                       document.Status != DocumentStatus.Complete;
            }
            catch
            {
                return false;
            }
        }

        #endregion
        #region Validation & Mapping

        /// <summary>
        /// Validates business rules on the submitted document form model.
        /// </summary>
        private bool ValidateDocumentRequest(DocumentFormViewModel model)
        {
            bool isValid = true;

            if (model.DocumentDetails == null || !model.DocumentDetails.Any())
            {
                ModelState.AddModelError("DocumentDetails", "กรุณาเพิ่มรายละเอียดเอกสารอย่างน้อย 1 รายการ");
                isValid = false;
            }

            for (int i = 0; i < model.DocumentDetails?.Count; i++)
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

            if (model.AvailableAreas == null || !model.AvailableAreas.Any(a => a.IsSelected))
            {
                ModelState.AddModelError("AvailableAreas", "กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Maps dynamic raw data from DB into a strongly typed ViewModel.
        /// </summary>
        private DocumentFormViewModel MapToViewModel(dynamic data)
        {
            return new DocumentFormViewModel
            {
                DarNo = data.DarNumber,
                Effective_date = data.Effective_date,
                Updated_at = data.Updated_at,
                Requester_name = data.RequesterFName?.Trim(),
                Request_type = data.Request_type,
                Document_type = data.Document_type,
                ProcessFlow_Review = data.ProcessFlowReview,
                ControlPlan_Review = data.ControlPlanReview,
                FMEA_Review = data.FMEAReview,
                RequesterSignature = data.RequesterSignature,
                Approvers = ((IEnumerable<dynamic>)data.Approvers)
                            .Select(a => new ApproverInfo
                            {
                                Step = a.Step,
                                ApproverName = a.ApproverFName?.Trim(),
                                ApprovedAt = a.Approved_at,
                                SignatureImage = a.SignatureImage
                            }).ToList()
            };
        }

        #endregion
        #region Approval Workflow Helpers

        /// <summary>
        /// Creates a new approval workflow with steps 1–3 using defined approver rights.
        /// </summary>
        private void CreateApprovalWorkflow(int documentId, int requesterId)
        {
            if (requesterId == 0)
                throw new ApproverConfigurationException("Requester ID is missing or invalid.", "ERR_INVALID_REQUESTER");

            var steps = new List<DocumentApprovalStep>();

            for (int step = 1; step <= 3; step++)
            {
                var approverId = GetApproverIdForStep(step, requesterId);

                steps.Add(new DocumentApprovalStep
                {
                    LId = documentId,
                    Step = step,
                    Approver_id = approverId,
                    Status = (step == 1) ? StepStatus.Pending : StepStatus.Waiting,
                    Comment = ""
                });
            }

            db.DocumentApprovalSteps.AddRange(steps);
        }

        /// <summary>
        /// Gets the approver ID for a specific step using configured RIH_Id roles.
        /// </summary>
        private int GetApproverIdForStep(int step, int requesterId)
        {
            try
            {
                int? approverId;

                switch (step)
                {
                    case 1:
                        approverId = 0; // department head will approve dynamically
                        break;
                    case 2:
                        approverId = db.UserRights
                            .Join(db.UserDetails,
                                  ur => ur.USE_Id,
                                  ud => ud.USE_Id,
                                  (ur, ud) => new { ur, ud })
                            .Where(x => x.ur.RIH_Id == 75)
                            .Select(x => (int?)x.ud.USE_Usercode)
                            .FirstOrDefault();
                        break;
                    case 3:
                        approverId = db.UserRights
                            .Join(db.UserDetails,
                                  ur => ur.USE_Id,
                                  ud => ud.USE_Id,
                                  (ur, ud) => new { ur, ud })
                            .Where(x => x.ur.RIH_Id == 76)
                            .Select(x => (int?)x.ud.USE_Usercode)
                            .FirstOrDefault();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(step), $"Invalid approval step: {step}");
                }


                if (!approverId.HasValue)
                {
                    throw new ApproverConfigurationException($"No approver configured for step {step}.", $"ERR_APPROVER_{step}");
                }

                return approverId.Value;
            }
            catch (ApproverConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] GetApproverIdForStep({step}): {ex.Message}");
                throw new Exception($"Unexpected error while retrieving approver for step {step}", ex);
            }
        }

        /// <summary>
        /// Gets a friendly name for an approval step.
        /// </summary>
        private string GetStepName(int step)
        {
            switch (step)
            {
                case 1:
                    return "Department Head (Asst manager up)";
                case 2:
                    return "Document Control Center (DCC)";
                case 3:
                    return "Quality Management Representative (QMR)";
                default:
                    return "Unknown Approval Step";
            }
        }

        #endregion

        /// <summary>
        /// Returns a list of available request types for dropdown.
        /// </summary>

        private List<CustomSelectListItem> GetRequestTypes()
        {
            return new List<CustomSelectListItem>
                {
                    new CustomSelectListItem { Value = "New Issue", Text = "New Issue", Description = "For new documents.", ThaiText = "ลงทะเบียนครั้งแรก", ThaiDescription = "สำหรับเอกสารใหม่" },
                    new CustomSelectListItem { Value = "Revised", Text = "Revised", Description = "For revised documents.", ThaiText = "ลงทะเบียนเอกสารแก้ไข/ปรับปรุง", ThaiDescription = "สำหรับเอกสารที่มีการแก้ไข" },
                    new CustomSelectListItem { Value = "External", Text = "External", Description = "For external documents.", ThaiText = "ลงทะเบียนเอกสารจากภายนอก", ThaiDescription = "สำหรับเอกสารที่รับมาจากภายนอก" },
                    new CustomSelectListItem { Value = "Obsolete", Text = "Obsolete", Description = "To request cancellation.", ThaiText = "ขออนุมัติยกเลิก", ThaiDescription = "สำหรับเอกสารที่ไม่มีการใช้งานแล้ว" },
                    new CustomSelectListItem { Value = "Copy", Text = "Copy", Description = "To request a copy.", ThaiText = "ขออนุมัติสำเนา", ThaiDescription = "สำหรับเอกสารที่ต้องการสำเนา" }
                };
        }


        /// <summary>
        /// Maps view model data to a new DocumentList entity.
        /// </summary>
        private DocumentList CreateDocumentRecord(DocumentFormViewModel model)
        {
            return new DocumentList
            {
                Requester_id = model.Requester_id,
                Request_from = model.Request_from,
                Request_type = model.Request_type,
                Document_type = model.Document_type,
                Effective_date = model.Effective_date,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
                Status = DocumentStatus.PendingApproval
            };
        }


        /// <summary>
        /// Saves each document detail item linked to the document ID.
        /// </summary>
        private void ProcessDocumentDetails(DocumentFormViewModel model, int documentId)
        {
            if (model.DocumentDetails == null) return;

            foreach (var detail in model.DocumentDetails)
            {
                db.DocumentDetails.Add(new DocumentDetail
                {
                    LId = documentId,
                    WS_number = detail.WS_number,
                    WS_name = detail.WS_name,
                    Revision = detail.Revision,
                    Num_pages = detail.Num_pages,
                    Num_copies = detail.Num_copies,
                    Change_detail = detail.Change_detail,
                    File_excel = detail.File_excel,
                    File_pdf = detail.File_pdf
                });
            }
        }

        /// <summary>
        /// Saves selected area sections from the form into the DocumentFormAreas table.
        /// </summary>
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


        #region Notification Service
        public static class NotificationService
        {
            public static string SendApprovalRequestEmail(List<string> to, string subject, List<string> cc, int LId, UrlHelper url, HttpRequestBase request)
            {
                try
                {
                    var callbackUrl = url.Action("Details", "Document", new { id = LId }, protocol: request.Url.Scheme);

                    string html = $@"
                        <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.8; padding-left:20px;'>
                            
                           
                            <p><b>Dear Department Head,</b></p>

                            <p style='text-indent:2em;'>
                                There is a new document action request pending your approval in the OCT WEB system. 
                                Please review and take action within 3 working days.
                            </p>

                            <p style='text-indent:2em;'>
                                🔗 <a href='{callbackUrl}' target='_blank'>Click here to open OCT WEB - Document Control</a>
                            </p>

                            <br />

                            <hr style='border: none; border-top: 1px solid #ccc; margin: 20px 0;' />
                            <p><b>เรียน หัวหน้าแผนก,</b></p>

                            <p style='text-indent:2em;'>
                                มีคำร้องขอดำเนินการเอกสาร (Document action request) ที่ต้องการการอนุมัติ กรุณาดำเนินการตรวจสอบและอนุมัติภายใน 3 วันทำการ
                            </p>

                            <p style='text-indent:2em;'>
                                🔗 <a href='{callbackUrl}' target='_blank'>คลิกที่นี่เพื่อเปิดระบบ OCT WEB - Document Control</a>
                            </p>

                            <br />

                            <p style='color:gray; font-size:12px;'>
                                *This email was automatically generated by the system. Please do not reply.*
                            </p>
                        </div>
                    ";

                    var model = new SendMailCenterModel
                    {
                        To = to ?? new List<string>(),
                        Tocc = cc ?? new List<string>(),
                        Subject = subject,
                        Body = html
                    };

                    SendMailCenterController.SendMail(model);
                    return "ส่งอีเมลสำเร็จ";
                }
                catch (Exception ex)
                {
                    return "เกิดข้อผิดพลาดในการส่งอีเมล: " + ex.Message;
                }
            }


            public static void SendSimpleEmail(List<string> to, string subject, string body)
            {
                try
                {
                    var model = new SendMailCenterModel
                    {
                        To = to ?? new List<string>(),
                        Tocc = new List<string>(),
                        Subject = subject,
                        Body = $"<p style='font-size: 16px'>{body}</p>"
                    };

                    SendMailCenterController.SendMail(model);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
                }
            }


            public static void NotifyAfterApproval(DocumentList document, DocumentDetail documentDetail, DocumentApprovalStep currentStep, string requesterEmail, UrlHelper urlHelper, HttpRequestBase request)
            {
                using (var db = new OCTWEBTESTEntities())
                {
                    string darNo = document.DarNumber ?? "N/A";
                    string wsNo = documentDetail?.WS_number ?? $"WS-{document.LId}";
                    string wsName = documentDetail?.WS_name ?? "(ไม่ระบุชื่อเอกสาร)";
                    string updatedAt = document.Updated_at?.ToString("yyyy-MM-dd") ?? "(ไม่ระบุวันที่)";
                    string docLink = urlHelper.Action("Details", "Document", new { id = document.LId }, request.Url.Scheme);
                    var step2 = db.DocumentApprovalSteps.FirstOrDefault(s => s.LId == document.LId && s.Step == 2);
                    var step2Approver = db.UserDetails.FirstOrDefault(u => u.USE_Usercode == step2.Approver_id);

                    string StepName;
                    switch (currentStep.Step)
                    {
                        case 1:
                            StepName = "Department Head";
                            break;
                        case 2:
                            StepName = "DCC";
                            break;
                        case 3:
                            StepName = "QMR";
                            break;
                        default:
                            StepName = currentStep.Step.ToString();
                            break;
                    }

                    // ----------------------------
                    // 1. แจ้งผู้ร้องขอเอกสาร (Requester)
                    // ----------------------------
                    if (!string.IsNullOrEmpty(requesterEmail))
                    {

                        string ApproverName = db.DocumentApprovalSteps
                            .Where(s => s.LId == document.LId && s.Step == currentStep.Step)
                            .OrderBy(s => s.Step)
                            .Join(db.UserDetails, s => s.Approver_id, u => u.USE_Usercode, (s, u) => u.USE_FName + " " + u.USE_LName)
                            .FirstOrDefault() ?? "ระบบ";

                        string requesterBody = $@"
                        <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.7; padding-left:20px;'>

                            <p><b>เรียน ผู้ร้องขอ,</b></p>

                            <p style='text-indent: 2em;'>
                                คำร้องขอดำเนินการเอกสารหมายเลข <b>{wsNo}</b> ได้รับการอนุมัติแล้ว<br/>
                                โดยผู้อนุมัติ: <b>{ApproverName} ({StepName})</b>
                            </p>

                            <p style='text-indent: 2em;'>
                                🔗 <a href='{docLink}' target='_blank'>คลิกที่นี่เพื่อเปิดเอกสาร</a>
                            </p>

                            <br/>

                            <p style='color:gray; font-size:12px;'>
                                **อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ**
                            </p>
                        </div>";

                        NotificationService.SendSimpleEmail(
                            to: new List<string> { requesterEmail },
                            subject: $"คำร้องขอดำเนินการเอกสาร WS No: {wsNo} ได้รับการอนุมัติจาก {StepName} แล้ว",
                            body: requesterBody
                        );
                    }

                    // ----------------------------
                    // 2. แจ้งผู้อนุมัติขั้นตอนถัดไป
                    // ----------------------------
                    var nextStep = db.DocumentApprovalSteps
                        .Where(s => s.LId == document.LId && s.Step > currentStep.Step)
                        .OrderBy(s => s.Step)
                        .FirstOrDefault();

                    if (nextStep != null)
                    {
                        var nextApprover = db.UserDetails.FirstOrDefault(u => u.USE_Usercode == nextStep.Approver_id);
                        if (!string.IsNullOrEmpty(nextApprover?.USE_Email))
                        {
                            string nextApproverBody = $@"
                            <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.7; padding-left:20px;'>
                                <p><b>Dear {nextApprover.USE_FName} {nextApprover.USE_LName},</b></p>

                                <p style='text-indent: 2em;'>
                                    กรุณาตรวจสอบและดำเนินการอนุมัติคำร้องเอกสาร WS No: <b>{wsNo}</b><br/>
                                </p>
                                <p style='text-indent: 2em;'>
                                    วันที่ลงทะเบียน: <b>{updatedAt}</b>
                                </p>

                                <p style='text-indent: 2em;'>
                                    🔗 <a href='{docLink}' target='_blank'>คลิกเพื่อเปิดเอกสาร</a>
                                </p>

                                <br/>
                                <p style='color:gray; font-size:12px;'>**อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ**</p>
                            </div>";

                            NotificationService.SendSimpleEmail(
                                to: new List<string> { nextApprover.USE_Email },
                                subject: $"มีคำขอดำเนินการเอกสาร รออนุมัติ WS No: {wsNo}",
                                body: nextApproverBody
                            );
                        }
                    }

                    // ----------------------------
                    // 3. แจ้ง DCC เมื่ออนุมัติ Step สุดท้าย (Step 3)
                    // ----------------------------
                    if (currentStep.Step == 3)
                    {

                        if (!string.IsNullOrEmpty(step2Approver?.USE_Email))
                        {
                            string qmrBody = $@"
                            <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.7; padding-left:20px;'>
                                <p><b>Dear {step2Approver.USE_FName} {step2Approver.USE_LName},</b></p>
                                
                                <p style='text-indent: 2em;'>
                                    เอกสาร WS No: <b>{wsNo}</b> ได้รับการอนุมัติโดย {StepName} เรียบร้อยแล้ว
                                </p>
                                
                                <p style='text-indent: 2em;'>
                                    🔗 <a href='{docLink}' target='_blank'>ดูรายละเอียดเอกสาร</a>
                                </p>

                                <br/>
                                <p style='color:gray; font-size:12px;'>**อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ**</p>
                            </div>";

                            NotificationService.SendSimpleEmail(
                                to: new List<string> { step2Approver.USE_Email },
                                subject: $"คำขอดำเนินการเอกสาร WS No: {wsNo} ผ่านการอนุมัติแล้ว",
                                body: qmrBody
                            );
                        }
                    }
                }

            }

            public static void NotifyAfterReject(DocumentList document, DocumentDetail documentDetail, DocumentApprovalStep currentStep, string requesterEmail, UrlHelper urlHelper, HttpRequestBase request)
            {
                using (var db = new OCTWEBTESTEntities())
                {
                    string darNo = document.DarNumber ?? "N/A";
                    string wsNo = documentDetail?.WS_number ?? $"WS-{document.LId}";
                    string wsName = documentDetail?.WS_name ?? "(ไม่ระบุชื่อเอกสาร)";
                    string updatedAt = document.Updated_at?.ToString("yyyy-MM-dd") ?? "(ไม่ระบุวันที่)";
                    string docLink = urlHelper.Action("Details", "Document", new { id = document.LId }, request.Url.Scheme);
                    var step2 = db.DocumentApprovalSteps.FirstOrDefault(s => s.LId == document.LId && s.Step == 2);
                    var step2Approver = db.UserDetails.FirstOrDefault(u => u.USE_Usercode == step2.Approver_id);

                    string StepName;
                    switch (currentStep.Step)
                    {
                        case 1:
                            StepName = "Department Head";
                            break;
                        case 2:
                            StepName = "DCC";
                            break;
                        case 3:
                            StepName = "QMR";
                            break;
                        default:
                            StepName = currentStep.Step.ToString();
                            break;
                    }

                  
                    if (!string.IsNullOrEmpty(requesterEmail))
                    {
                        string registrationDate = document.Updated_at?.ToString("yyyy-MM-dd") ?? "(ไม่ระบุวันที่)";
                        string subject = $"คำขอดำเนินการเอกสาร WS No: {wsNo} ถูกปฏิเสธ";
                        string body = $@"
                            <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.7; padding-left:20px;'>
                                <p>เรียน ผู้ร้องขอ,</p>
                                <p style='text-indent:2em;'>
                                    คำขอดำเนินการเอกสาร ws no: <b>{wsNo}</b>
                                </p>
                                <p style='text-indent:2em;'>
                                    สถานะ: <b><span style='color:red'><b>ปฏิเสธ</b></span></b>
                                </p>
                                <p style='text-indent: 2em;'>
                                    อนุมัติโดย: <b>{StepName}</b>
                                </p>
                                <p style='text-indent: 2em;'>
                                    เหตุผล: <b>{currentStep.Comment}</b>
                                </p>
                                <p style='text-indent:2em;'>
                                    🔗 <a href='{docLink}' target='_blank'>คลิกที่นี่เพื่อดูรายละเอียดเอกสาร</a>
                                </p>
                                <br/>
                                <p style='color:gray; font-size:12px;'>**อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ**</p>
                            </div>";

                        NotificationService.SendSimpleEmail(
                            to: new List<string> { requesterEmail },
                            subject: subject,
                            body: body
                        );
                    }
                }

            }

            public static void NotifyProcessReviewTeamsIfNeeded(DocumentList document, DocumentDetail documentDetail, UrlHelper urlHelper, HttpRequestBase request)
            {
                // ตรวจสอบว่าเอกสารนี้ต้องการการ review หรือไม่
                bool requireFMEA = document.FMEAReview == true;
                bool requireControlPlan = document.ControlPlanReview == true;
                bool requireProcessFlow = document.ProcessFlowReview == true;

                if (!(requireFMEA || requireControlPlan || requireProcessFlow))
                    return;

                using (var db = new OCTWEBTESTEntities())
                {
                    var requiredRightIds = new[] { 77 };

                    var notifyUsers = db.UserRights
                        .Where(r => requiredRightIds.Contains(r.RIH_Id))
                        .Join(db.UserDetails, r => r.USE_Id, u => u.USE_Id, (r, u) => u)
                        .Select(u => u.USE_Email)
                        .Where(email => !string.IsNullOrEmpty(email))
                        .Distinct()
                        .ToList();

                    if (notifyUsers.Any())
                    {
                        var reviewItems = new List<string>();
                        if (requireFMEA) reviewItems.Add("FMEA");
                        if (requireControlPlan) reviewItems.Add("Control Plan");
                        if (requireProcessFlow) reviewItems.Add("Process Flow");

                        string wsNo = !string.IsNullOrEmpty(documentDetail.WS_number)
                            ? documentDetail.WS_number
                            : "N/A";

                        string wsName = documentDetail?.WS_name ?? "(ไม่ระบุชื่อเอกสาร)";
                        string darNo = document?.DarNumber ?? "N/A";
                        string registrationDate = document.Updated_at?.ToString("yyyy-MM-dd") ?? "(ไม่ระบุวันที่)";
                        string documentLink = urlHelper.Action("Details", "Document", new { id = document.LId }, request.Url.Scheme);

                        string subject = $"OCT - DocumentControl | WS No: {wsNo} ต้องการการ Review";

                        string body = $@"
                        <div style='font-family:Tahoma, sans-serif; font-size:14px; line-height:1.7; padding-left:20px;'>

                            <p><b>เรียน ผู้เกี่ยวที่ข้อง </b></p>

                            <p style='text-indent: 2em;'>
                                เอกสาร Dar No:  <b>{darNo}</b>
                            </p>
                            <p style='text-indent: 2em;'>
                                เอกสาร WS No:  <b>{wsNo}</b>
                            </p>

                            <p style='text-indent: 2em;'>                               
                                ต้องการ Review ในหัวข้อ:  <b>{string.Join(", ", reviewItems)}</b>
                            </p>

                            <p style='text-indent: 2em;'>
                                🔗 <a href='{documentLink}' target='_blank'>คลิกที่นี่เพื่อเปิดเอกสาร</a>
                            </p>

                            <br/>
                            <p style='color:gray; font-size:12px;'>**อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ**</p>
                        </div>";

                        NotificationService.SendSimpleEmail(
                            to: notifyUsers,
                            subject: subject,
                            body: body
                        );
                    }
                }
            }
        }
        #endregion
        #region Document Update Logic
        private void UpdateDocumentRecord(DocumentList document, DocumentFormViewModel model)
        {
            document.Status = DocumentStatus.PendingApproval;
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
        private void UpdateApprovalWorkflow(int documentId, int requesterId)
        {
            if (requesterId == 0)
                throw new ApproverConfigurationException("Requester ID is missing or invalid.", "ERR_INVALID_REQUESTER");

            for (int step = 1; step <= 3; step++)
            {
                var approverId = GetApproverIdForStep(step, requesterId);
                var existingStep = db.DocumentApprovalSteps.FirstOrDefault(s => s.LId == documentId && s.Step == step);

                if (existingStep != null)
                {
                    existingStep.Approver_id = approverId;
                    existingStep.Status = (step == 1) ? StepStatus.Pending : StepStatus.Waiting;
                    existingStep.Comment = "";
                }
            }

            db.SaveChanges();
        }

        private void UpdateSelectedAreas(DocumentFormViewModel model, int documentId)
        {
            var existingAreas = db.DocumentFormAreas.Where(a => a.LId == documentId).ToList();
            db.DocumentFormAreas.RemoveRange(existingAreas);
            ProcessSelectedAreas(model, documentId);
        }

        #endregion
        #region ExportFileHelpers
        private dynamic GetRawData(int id)
        {
            return (from doc in db.DocumentLists
                    where doc.LId == id
                    join reqUser in db.EmpLists on doc.Requester_id equals reqUser.EmpID into reqGroup
                    from req in reqGroup.DefaultIfEmpty()
                    join reqSig in db.EmployeeSignatures on req.EmpID equals reqSig.EmpId into reqSigGroup
                    from reqSigData in reqSigGroup.DefaultIfEmpty()
                    select new
                    {
                        doc.Effective_date,
                        doc.Updated_at,
                        doc.Request_type,
                        doc.Document_type,
                        doc.FMEAReview,
                        doc.ControlPlanReview,
                        doc.ProcessFlowReview,
                        doc.DarNumber,
                        RequesterFName = req.FName_EN,
                        RequesterLName = req.LName_EN,
                        RequesterSignature = reqSigData.SignatureImage,

                        Approvers = (from step in db.DocumentApprovalSteps
                                     where step.LId == doc.LId
                                     join appUser in db.EmpLists on step.Approver_id equals appUser.EmpID into appGroup
                                     from app in appGroup.DefaultIfEmpty()
                                     join sig in db.EmployeeSignatures on app.EmpID equals sig.EmpId into sigGroup
                                     from sigData in sigGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         Step = step.Step ?? 0,
                                         ApproverFName = app.FName_EN,
                                         ApproverLName = app.LName_EN,
                                         step.Approved_at,
                                         SignatureImage = sigData.SignatureImage
                                     }).ToList()
                    }).FirstOrDefault();
        }
        private void FillStaticData(IXLWorksheet ws, DocumentFormViewModel vm)
        {
            ws.Cell("S2").Value = $"Issued Date    {vm.Updated_at?.ToString("dd/MM/yyyy") ?? "N/A"}";
            ws.Cell("AG2").Value = $"DAR NO  {vm.DarNo}";
            ws.Cell("A28").Value = $"Date         {vm.Effective_date.ToString("dd/MM/yyyy") ?? "N/A"}";
            
            ws.Cell("J32").Value = $"Date   {vm.Updated_at?.ToString("dd/MM/yyyy") ?? "N/A"}";

            byte[] transparentImage = MakeBackgroundTransparent(vm.RequesterSignature);
            using (var stream = new MemoryStream(transparentImage))
            {

                var picture = ws.AddPicture(stream)
                                .WithPlacement(XLPicturePlacement.Move);

                // จำกัดขนาด
                const double maxWidthPx = 70.08;
                const double maxHeightPx = 66.24;

                var originalImage = System.Drawing.Image.FromStream(new MemoryStream(vm.RequesterSignature));
                var width = originalImage.Width;
                var height = originalImage.Height;

                double scale = Math.Min(maxWidthPx / width, maxHeightPx / height);
                picture.Scale(scale);

                // Offset ด้านบนเล็กน้อย
                const int yOffsetUpPx = 5;
                ws.Cell("A32").Value = $"Requested By";
                picture.MoveTo(ws.Cell("E32"), 0, yOffsetUpPx);
            }

        }

        private List<string> GetSectionCodes(int id)
        {
            return db.DocumentFormAreas
                     .Join(db.DocumentSections,
                           a => a.WS_TS_Id,
                           s => s.Id,
                           (a, s) => new { a.LId, s.SectionCode })
                     .Where(a => a.LId == id)
                     .Select(a => a.SectionCode)
                     .ToList();
        }

        //Export file to PDF OR EXCEL
        public byte[] MakeBackgroundTransparent(byte[] imageBytes)
        {
            using (var inputStream = new MemoryStream(imageBytes))
            using (var originalImage = new Bitmap(inputStream))
            {
                var transparentImage = new Bitmap(originalImage.Width, originalImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var transparentColor = Color.White;

                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        var pixelColor = originalImage.GetPixel(x, y);

                        if (pixelColor.R > 240 && pixelColor.G > 240 && pixelColor.B > 240)
                            transparentImage.SetPixel(x, y, Color.FromArgb(0, 255, 255, 255)); // โปร่งใส
                        else
                            transparentImage.SetPixel(x, y, pixelColor);
                    }
                }

                // หา Bounding Box ของพิกเซลที่ไม่โปร่งใส
                int minX = transparentImage.Width;
                int minY = transparentImage.Height;
                int maxX = 0;
                int maxY = 0;

                for (int y = 0; y < transparentImage.Height; y++)
                {
                    for (int x = 0; x < transparentImage.Width; x++)
                    {
                        var pixel = transparentImage.GetPixel(x, y);
                        if (pixel.A != 0) // ไม่โปร่งใส
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                // ตรวจสอบว่าเจอพิกเซลที่ไม่โปร่งใสหรือไม่
                if (minX >= maxX || minY >= maxY)
                {
                    // ไม่มีอะไรให้ crop (รูปว่าง)
                    using (var outputStream = new MemoryStream())
                    {
                        transparentImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
                        return outputStream.ToArray();
                    }
                }

                // Crop เฉพาะส่วนที่มีพิกเซลไม่โปร่งใส
                Rectangle cropRect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
                Bitmap croppedImage = transparentImage.Clone(cropRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var outputStream = new MemoryStream())
                {
                    croppedImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
                    return outputStream.ToArray();
                }
            }
        }

        private void FillRequestType(IXLWorksheet ws, string requestType)
        {
            var map = new Dictionary<string, string>
            {
                ["New Issue"] = "A5",
                ["Revised"] = "A6",
                ["External"] = "A7",
                ["Obsolete"] = "A8",
                ["Copy"] = "A9"
            };

            if (map.TryGetValue(requestType, out string cell))
                ws.Cell(cell).Value = "✔";
        }

        private void FillDocumentType(IXLWorksheet ws, string type)
        {
            if (type == "controlled") ws.Cell("A22").Value = "✔";
            else if (type == "uncontrolled") ws.Cell("A24").Value = "✔";
        }

        private void FillFileExistChecks(IXLWorksheet ws, List<DocumentDetail> details)
        {
            ws.Cell("A12").Value = details.Any(d => !string.IsNullOrEmpty(d.File_excel)) ? "✔" : "✖";
            ws.Cell("H12").Value = details.Any(d => !string.IsNullOrEmpty(d.File_pdf)) ? "✔" : "✖";
        }

        private void FillSectionCodes(IXLWorksheet ws, List<string> codes)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "IS", "S16" }, { "HR", "S17" }, { "AD", "S18" }, { "PU", "S19" },
                { "IT", "U16" }, { "PE", "U17" }, { "EN", "U18" }, { "PC", "U19" },
                { "QA", "W16" }, { "QC", "W17" }, { "PD1", "W18" }, { "L1", "W19" },
                { "PP", "Y16" }, { "L2", "Y17" }, { "L3", "Y18" }, { "RA", "Y19" },
                { "GS", "AA16" }, { "BR", "AA17" }, { "AB1", "AA18" }, { "AT", "AA19" },
                { "FC", "AD16" }, { "CA", "AD17" }, { "AF", "AD18" }, { "GN", "AD19" },
                { "AB2", "AF16" }, { "AS", "AF17" }, { "AC", "AF18" }, { "PD2", "AF19" },
                { "SE", "AI16" }, { "FR", "AI17" }, { "PT", "AI18" }, { "ACC", "AI19" },
            };

            foreach (var code in codes)
                if (map.TryGetValue(code, out string cell))
                    ws.Cell(cell).Value = "✔";
        }

        private void FillReviewChecks(IXLWorksheet ws, DocumentFormViewModel vm)
        {
            ws.Cell(vm.FMEA_Review == true ? "AB24" : "X24").Value = "✔";
            ws.Cell(vm.ControlPlan_Review == true ? "AB25" : "X25").Value = "✔";
            ws.Cell(vm.ProcessFlow_Review == true ? "AB26" : "X26").Value = "✔";
        }

        private void FillApprovers(IXLWorksheet ws, List<ApproverInfo> approvers, DocumentFormViewModel vm)
        {
            foreach (var approver in approvers)
            {
                if (approver.SignatureImage == null || approver.SignatureImage.Length == 0)
                    continue;

                byte[] transparentImage = MakeBackgroundTransparent(approver.SignatureImage);
                using (var stream = new MemoryStream(transparentImage))
                {

                    var picture = ws.AddPicture(stream)
                                    .WithPlacement(XLPicturePlacement.Move);

                    // จำกัดขนาด
                    const double maxWidthPx = 70.08;
                    const double maxHeightPx = 66.24;

                    var originalImage = System.Drawing.Image.FromStream(new MemoryStream(approver.SignatureImage));
                    var width = originalImage.Width;
                    var height = originalImage.Height;

                    double scale = Math.Min(maxWidthPx / width, maxHeightPx / height);
                    picture.Scale(scale);

                    // Offset ด้านบนเล็กน้อย
                    const int yOffsetUpPx = 5;


                    switch (approver.Step)
                    {
                        case 1:
                            ws.Cell("A34").Value = "Received By";
                            picture.MoveTo(ws.Cell("E34"), 0, yOffsetUpPx);
                            ws.Cell("J34").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;

                        case 2:
                            picture.MoveTo(ws.Cell("E38"), 0, yOffsetUpPx);
                            ws.Cell("J38").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;

                        case 3:
                            picture.MoveTo(ws.Cell("w31"), 0, yOffsetUpPx);
                            ws.Cell("AB31").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;
                    }
                }
            }
        }
        private void FillDetails(IXLWorksheet ws, List<DocumentDetail> details)
        {
            int maxRow = 5;
            for (int i = 0; i < Math.Min(details.Count, maxRow); i++)
            {
                var d = details[i];
                ws.Cell($"L{7 + i}").Value = i + 1;
                ws.Cell($"M{7 + i}").Value = d.WS_number;
                ws.Cell($"R{7 + i}").Value = d.WS_name;
                ws.Cell($"AF{7 + i}").Value = d.Revision;
                ws.Cell($"AH{7 + i}").Value = $"{d.Num_pages}/1";
                ws.Cell($"AJ{7 + i}").Value = d.Num_copies;
            }

            var changeDetails = details
                .Where(d => !string.IsNullOrWhiteSpace(d.Change_detail))
                .Select(d => d.Change_detail.Trim())
                .ToList();

            for (int i = 0; i < Math.Min(changeDetails.Count, 4); i++)
                ws.Cell($"A{16 + i}").Value = changeDetails[i];
        }

        #endregion
        #region View file upload
        public ActionResult Previewfile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return new HttpStatusCodeResult(400, "Invalid file request.");
            }

            try
            {
                // ป้องกัน directory traversal
                string sanitizedFileName = Path.GetFileName(file);
                if (sanitizedFileName != file)
                {
                    return new HttpStatusCodeResult(400, "Invalid file name.");
                }

                // ตรวจสอบ extension และเลือก path ให้เหมาะสม
                string extension = Path.GetExtension(sanitizedFileName)?.ToLowerInvariant();
                string basePath = null;

                if (extension == ".pdf")
                {
                    basePath = ConfigurationManager.AppSettings["path_Document_Pdf"];
                }
                else if (extension == ".xls" || extension == ".xlsx")
                {
                    basePath = ConfigurationManager.AppSettings["path_Document_Excel"];
                }
                else
                {
                    return new HttpStatusCodeResult(415, "Unsupported file type.");
                }

                // รวม path ไฟล์
                string fullPath = Path.Combine(basePath, sanitizedFileName);

                if (!System.IO.File.Exists(fullPath))
                {
                    return HttpNotFound("File not found.");
                }

                // ตรวจสอบ MIME type
                string contentType = MimeMapping.GetMimeMapping(fullPath);

                // คืนค่าเป็นไฟล์แสดงผลแบบ inline
                return File(fullPath, contentType, sanitizedFileName);
            }
            catch (Exception ex)
            {
                // สามารถเพิ่ม Logging ได้ที่นี่
                return new HttpStatusCodeResult(500, "An error occurred while processing the file.");
            }
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CanEdit(int id)
        {
            try
            {
                var document = db.DocumentLists.FirstOrDefault(d => d.LId == id);
                if (document == null)
                    return Json(new { success = false, message = "ไม่พบคำร้องที่คุณต้องการ กรุณาติดต่อเจ้าหน้าที่" });

                if (document.Status == DocumentStatus.Complete)
                    return Json(new { success = false, message = "คำร้องนี้ดำเนินการเสร็จสิ้นแล้ว ไม่สามารถแก้ไขได้" });

                if (!CanEditDocument(document))
                    return Json(new { success = false, message = "คุณไม่ได้รับสิทธิ์ในการแก้ไขคำร้องนี้ กรุณาติดต่อผู้ดูแลระบบ" });

                string warningMessage = null;
                if (document.Status == DocumentStatus.PendingApproval )
                {
                    warningMessage = "เอกสารรออนุมัติอยู่ หากแก้ไขระบบจะเริ่มอนุมัติใหม่ทั้งหมด คุณต้องการดำเนินการต่อหรือไม่?";
                }

                var redirectUrl = Url.Action("Edit", "Document", new { id = document.LId });

                return Json(new
                {
                    success = true,
                    warningMessage,
                    redirectUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
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

    }
}