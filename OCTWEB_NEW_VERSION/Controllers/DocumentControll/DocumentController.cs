using ClosedXML.Excel;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static OCTWEB_NET45.Controllers.DocumentControll.DocumentNotification;
using static OCTWEB_NET45.Controllers.DocumentControll.DocumentService;


namespace OCTWEB_NET45.Controllers.DocumentControll
{

    //[Authorize]
    [CustomAuthorize(73)]
    public class DocumentController : Controller
    {
        #region Fields

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        private DocumentService service = new DocumentService();

        private DocumentNotification notification = new DocumentNotification();

        #endregion

        #region Document List - Get
        /// <summary>
        /// Displays a paginated list of documents with optional search and status filters.
        /// </summary>
        /// <param name="searchString">Optional keyword for filtering document name, ID, or requester</param>
        /// <param name="statusFilter">Optional status filter (e.g., "COMPLETE", "EDITING")</param>
        /// <param name="page">Page number for pagination</param>
        public ActionResult List(int? page, string searchString = null, string statusFilter = null)
        {
            try
            {

                ViewBag.CurrentFilter = searchString;
                ViewBag.CurrentStatus = statusFilter;

                IQueryable<DocumentList> documentsQuery = db.DocumentLists.AsQueryable();

                if (Session["UserCode"] != null)
                {
                    int usercode = Convert.ToInt32(Session["UserCode"]);
                    int usercodeId = db.UserDetails
                        .Where(u => u.USE_Usercode == usercode)
                        .Select(u => u.USE_Id)
                        .FirstOrDefault();

                    bool isAdmin = db.UserRights.Any(ur => ur.USE_Id == usercodeId && ur.RIH_Id == 73);

                    if (!isAdmin)
                    {
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
                    else
                    {
                        // Admin sees all documents
                        documentsQuery = documentsQuery.Where(d => d.Request_from == DocumentTypes.Common);
                    }
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] List action failed: {ex}");
                ViewBag.ErrorMessage = "An error occurred while loading the document list. Please try again later.";
                return View(new PagedList<DocumentListDisplayViewModel>(new List<DocumentListDisplayViewModel>(), 1, 10));

            }
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
                    Requester_id = service.GetCurrentUserId(Session),
                    Effective_date = DateTime.Now.AddDays(3),
                    DocumentDetails = new List<DocumentDetailViewModel> { new DocumentDetailViewModel() },
                    AvailableAreas = service.LoadAvailableAreas(),
                    RequestTypes = service.GetRequestTypes()
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
                if (!ModelState.IsValid)
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
                        var document = service.CreateDocumentRecord(model);
                        db.DocumentLists.Add(document);
                        db.SaveChanges();

                        service.ProcessFileUploads(model, document.LId);
                        service.ProcessDocumentDetails(model, document.LId);
                        service.ProcessSelectedAreas(model, document.LId);
                        service.CreateApprovalWorkflow(document, document.LId, document.Requester_id);

                        // Notify approvers of the new request
                        var redirectUrl = Url.Action("Details", "Document", new { id = document.LId }, Request.Url.Scheme);

                        NotificationService.SendApprovalRequestEmail(
                            to: null,
                            subject: "New Request Document Control Pending Approval",
                            cc: new List<string>(),
                            callbackUrl: redirectUrl,
                            session: Session
                        );


                        db.SaveChanges();

                        transaction.Commit();

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

                if (!service.CanEditDocument(document,service.GetCurrentUserId(Session)))
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
                var existingSteps = db.DocumentApprovalSteps.Where(s => s.LId == id).ToList();
                if (existingSteps.Any())
                {
                    foreach (var step in existingSteps)
                    {
                        step.Status = StepStatus.Waiting;
                    }

                    document.Status = DocumentStatus.Editing;
                    db.SaveChanges();
                }




                var availableAreas = service.LoadAvailableAreas();
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
                    RequestTypes = service.GetRequestTypes(),

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
                
                var document = db.DocumentLists.FirstOrDefault(d => d.LId == model.Id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลคำร้อง";
                    return RedirectToAction("List");
                }

                if (!service.CanEditDocument(document, service.GetCurrentUserId(Session)))
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขคำร้องนี้";
                    return RedirectToAction("List");
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        service.UpdateDocumentRecord(document, model);
                        service.ProcessFileUploads(model, document.LId);
                        service.UpdateDocumentDetails(model, document.LId);
                        service.UpdateSelectedAreas(model, document.LId);
                        service.UpdateApprovalWorkflow(document, document.LId, document.Requester_id);

                        db.SaveChanges();
                        transaction.Commit();

                        // Notify approvers of the new request
                        var redirectUrl = Url.Action("Details", "Document", new { id = document.LId }, Request.Url.Scheme);

                        NotificationService.SendApprovalRequestEmail(
                            to: null,
                            subject: "Updated ISO 14001 Request Pending Review",
                            cc: new List<string>(),
                            callbackUrl: redirectUrl,
                            session: Session
                        );

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
                model.AvailableAreas = service.LoadAvailableAreas();
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
                        return Json(new { success = false, message = "The request number to be delete was not found" });
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
                        // Remove files from disk if they exist
                        foreach (var detail in details)
                        {
                            if (!string.IsNullOrEmpty(detail.File_excel))
                            {
                                var excelPath = Path.Combine(ConfigurationManager.AppSettings["path_Document_Excel"], detail.File_excel);
                                if (System.IO.File.Exists(excelPath))
                                {
                                    System.IO.File.Delete(excelPath);
                                }
                            }
                            if (!string.IsNullOrEmpty(detail.File_pdf))
                            {
                                var pdfPath = Path.Combine(ConfigurationManager.AppSettings["path_Document_Pdf"], detail.File_pdf);
                                if (System.IO.File.Exists(pdfPath))
                                {
                                    System.IO.File.Delete(pdfPath);
                                }
                            }
                        }

                        db.DocumentDetails.RemoveRange(details);
                    }

                    // 3. ลบข้อมูลจากตารางหลัก (Parent table)
                    db.DocumentLists.Remove(document);

                    // 4. บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูล
                    db.SaveChanges();

                    // 5. ยืนยันการทำรายการ (Commit Transaction)
                    transaction.Commit();

                    // ส่งผลลัพธ์ว่าการลบสำเร็จ
                    return Json(new { success = true, message = "The request has been deleted successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Response.StatusCode = 500;
                    return Json(new { success = false, message = "An unexpected error occurred while deleting the document : " + ex.Message });
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

            var service = new DocumentService();
            foreach (var step in approvalSteps)
                step.StepName = StepNameHelper.GetStepName(document,step.Step);

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
                CurrentUserId = service.GetCurrentUserId(Session),
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
                    var currentStep = db.DocumentApprovalSteps.FirstOrDefault(s => s.AId == model.StepAId && s.LId == model.DocumentId);
                    if (currentStep == null)
                        return Json(new { success = false, message = "Approval step not found." });

                    var document = db.DocumentLists.Find(model.DocumentId);
                    var documentdetail = db.DocumentDetails.FirstOrDefault(d => d.LId == model.DocumentId);

                    if (document == null)
                        return Json(new { success = false, message = "Document not found." });

                    var requester = db.UserDetails.FirstOrDefault(u => u.USE_Usercode == document.Requester_id);
                    var requesterEmail = requester?.USE_Email;

                    if (!IsUserAuthorizedToApprove(currentStep, service.GetCurrentUserId(Session), document.Requester_id))
                    {
                        return Json(new { success = false, message = "You are not authorized to approve this step." });
                    }

                    if (currentStep.Status != StepStatus.Pending)
                    {
                        return Json(new { success = false, message = "This step is not in a pending state." });
                    }

                    var detailsUrl = Url.Action("Details", "Document", new { id = document.LId }, Request.Url.Scheme);
                    if (model.Action.ToLower() == "reject")
                    {
                        if (string.IsNullOrWhiteSpace(model.Comment))
                            return Json(new { success = false, message = "Please provide a comment for rejection." });

                        RejectDocument(document, currentStep, model.Comment);
                        db.SaveChanges();
                        transaction.Commit();

                        NotificationService.NotifyAfterReject(document, documentdetail, currentStep, requesterEmail: requesterEmail,
                        callbackUrl: detailsUrl);

                        return Json(new { success = true, message = "The request has been rejected and sent back for revision." });
                    }


                    // Handle Approval
                    ApproveStep(document, documentdetail, currentStep, model, service.GetCurrentUserId(Session));
                    db.SaveChanges();
                    transaction.Commit();

                    // แจ้งหลังอนุมัติ
                    NotificationService.NotifyAfterApproval(
                        document: document,
                        documentDetail: documentdetail,
                        currentStep: currentStep,
                        requesterEmail: requesterEmail,
                        callbackUrl: detailsUrl // ส่งเป็น string
                    );

                    NotificationService.NotifyProcessReviewTeamsIfNeeded(
                        document: document,
                        documentDetail: documentdetail,
                        callbackUrl: detailsUrl // ส่งเป็น string
                    );

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
            var currentUserId = service.GetCurrentUserId(Session);
            foreach (var step in allSteps)
            {
                if (step.AId == currentStep.AId)
                {
                    step.Approver_id = currentUserId;
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
                .Where(d => d.DarNumber.StartsWith(currentYear + "-") && d.Request_from == DocumentTypes.Common)
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

        /// Handles a cancellation action initiated by the requester for a document that has been approved at step 1.
        /// </summary>
        /// <param name="id">The ID of the document to cancel.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelApproval(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var document = db.DocumentLists.Find(id);

                    if (document == null)
                    {
                        return Json(new { success = false, message = "Document not found." });
                    }

                    // 1. Validate that the current user is the requester
                    if (document.Requester_id != service.GetCurrentUserId(Session))
                    {
                        return Json(new { success = false, message = "You are not authorized to cancel this request." });
                    }

                    // 2. Validate the document status
                    if (document.Status != DocumentStatus.PendingApproval)
                    {
                        return Json(new { success = false, message = "This request cannot be canceled as it's not in a pending state." });
                    }

                    var allSteps = db.DocumentApprovalSteps.Where(s => s.LId == id).OrderBy(s => s.Step).ToList();
                    var step1 = allSteps.FirstOrDefault(s => s.Step == 1);

                    // 3. Validate that step 1 has been approved and it's not yet complete
                    if (step1 == null || step1.Status != StepStatus.Approved)
                    {
                        return Json(new { success = false, message = "Cancellation is only possible after the Department Head has approved." });
                    }

                    // Logic: Reset the entire workflow
                    document.Status = DocumentStatus.Rejected; // Set status to Rejected so the user can edit it again.

                    foreach (var step in allSteps)
                    {
                        if (step.Step == 1)
                        {
                            step.Status = StepStatus.Rejected; // Mark step 1 as 'Rejected' to signify cancellation
                            step.Comment = "Request was canceled by the originator after approval.";
                            step.Approved_at = DateTime.Now;
                        }
                        else
                        {
                            step.Status = StepStatus.Waiting; // Reset subsequent steps
                            step.Comment = null;
                            step.Approved_at = null;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, message = "The approval request has been successfully canceled and returned for editing." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log the exception ex
                    return Json(new { success = false, message = "An error occurred during cancellation: " + ex.Message });
                }
            }
        }

        #endregion

        #region Document Export (Excel)

        /// <summary>
        /// Generates and exports a filled Excel file based on the document data.
        /// </summary>
        /// <param name="id">Document ID</param>
        public ActionResult ExportExcel(int id)
        {
            try
            {
                string tempDirectory = ConfigurationManager.AppSettings["path_Document_Templat"];
                Directory.CreateDirectory(tempDirectory);

                string uniqueFileName = Guid.NewGuid().ToString();
                string templatePath = Path.Combine(tempDirectory, "OCT-IS-FM028.xlsx");
                string outputExcelPath = Path.Combine(tempDirectory, $"DocumentForm_{uniqueFileName}.xlsx");
                if (!System.IO.File.Exists(templatePath))
                    return HttpNotFound("ไม่พบเทมเพลตเอกสารสำหรับการส่งออก");   

                var rawData = service.GetRawData(id);
                if (rawData == null)
                    return HttpNotFound($"ไม่พบข้อมูลเอกสารที่ระบุ (ID: {id})");

                var viewModel = MapToViewModel(rawData);

                var details = db.DocumentDetails.Where(d => d.LId == id).ToList();
                var sectionCodes = service.GetSectionCodes(id);

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    service.FillStaticData(worksheet, viewModel);
                    service.FillRequestType(worksheet, viewModel.Request_type);
                    service.FillDocumentType(worksheet, viewModel.Document_type);
                    service.FillReviewChecks(worksheet, viewModel);
                    service.FillFileExistChecks(worksheet, details);
                    service.FillSectionCodes(worksheet, sectionCodes);
                    service.FillApprovers(worksheet, viewModel.Approvers, viewModel);
                    service.FillDetails(worksheet, details);

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
       
        #region Mapping

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

        #region Document Check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CanEdit(int id)
        {
            try
            {
                var document = db.DocumentLists.FirstOrDefault(d => d.LId == id);
                if (document == null)
                    return Json(new { success = false, message = "The requested document was not found. Please contact support." });

                if (document.Status == DocumentStatus.Complete)
                    return Json(new { success = false, message = "This document has already been completed and cannot be edited." });

                if (!service.CanEditDocument(document, service.GetCurrentUserId(Session)))
                    return Json(new { success = false, message = "You do not have permission to edit this document. Please contact the system administrator." });

                string warningMessage = null;
                if (document.Status == DocumentStatus.PendingApproval)
                {
                    warningMessage = "This document is currently pending approval. Editing it will reset the approval process. Do you want to proceed?";
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
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        #endregion

        #region View Attach Files

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

        [HttpGet]
        public JsonResult GetWSData()
        {
            try
            {
                var wsData = db.WSR_WorkingStandardEdit
                    .AsEnumerable() // ดึงออกจาก SQL ก่อน
                    .Select(ws => new
                    {
                        ws.WS_Id,
                        ws.WS_Name,
                        ws.WS_Number,
                        WS_Rev = ws.WS_Rev == null
                            ? "00"                       
                            : ws.WS_Rev.ToString().PadLeft(2, '0') 
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