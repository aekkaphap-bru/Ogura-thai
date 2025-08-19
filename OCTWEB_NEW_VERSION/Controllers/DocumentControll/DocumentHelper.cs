using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using System.Web.Mvc;


namespace OCTWEB_NET45.Controllers.DocumentControll
{
    public class DocumentService
    {
        #region connection
        private readonly OCTWEBTESTEntities db;

        public DocumentService()
        {
            db = new OCTWEBTESTEntities();
        }
        #endregion

        #region Exception class
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
        #endregion

        #region Status Helper
        public static class DocumentTypes
        {
            public const string Common = "IATF16949";
            public const string ISO = "ISO14001";
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

        #region StepApproveNameHelper

        public static class StepNameHelper 
        {
            public static string GetInitialsName(DocumentList document, DocumentApprovalStep currentStep)
            {
                // Check DocumentType
                var DocumentType = document.Document_type ?? "N/A";
                
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
                        if(DocumentType == DocumentTypes.Common)
                        {
                            StepName = "QMR";
                        }
                        else if(DocumentType == DocumentTypes.ISO)
                        {
                            StepName = "EMR";
                        }
                        else
                        {
                            StepName = "N/A";
                        }
                        
                        break;
                    default:
                        StepName = currentStep.Step.ToString();
                        break;
                }

                return StepName;


            }

            public static string GetStepName(DocumentList document,int step)
            {
                var DocumentType = document.Request_from ?? "N/A";

                switch (step)
                {
                    case 1:
                        return "Department Head (Asst manager up)";
                    case 2:
                        return "Document Control Center (DCC)";
                    case 3:
                        if (DocumentType == DocumentTypes.Common)
                        {
                            return "Quality Management Representative (QMR)";
                        }
                        else if (DocumentType == DocumentTypes.ISO)
                        {
                            return "Environmental Management Representative (EMR)";
                        }
                        else
                        {
                            return "N/A";
                        }     
                    default:
                        return "Unknown Approval Step";
                }
            }

            public static int GetApproverIdForStep(DocumentList document, OCTWEBTESTEntities db, int step, int requesterId)
            {
                int? approverId;
                var DocumentType = document.Request_from ?? "N/A";

                switch (step)
                {
                    case 1:
                        approverId = 0;
                        break;
                    case 2:
                        
                        approverId = db.UserRights
                            .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
                            .Where(x => x.ur.RIH_Id == 75)
                            .Select(x => (int?)x.ud.USE_Usercode)
                            .FirstOrDefault();
                        break;
                    case 3:
                        if (DocumentType == DocumentTypes.Common)
                        {
                            approverId = db.UserRights
                                .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
                                .Where(x => x.ur.RIH_Id == 76)
                                .Select(x => (int?)x.ud.USE_Usercode)
                                .FirstOrDefault();
                        }
                        else if (DocumentType == DocumentTypes.ISO)
                        {
                            approverId = db.UserRights
                            .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
                            .Where(x => x.ur.RIH_Id == 79)
                            .Select(x => (int?)x.ud.USE_Usercode)
                            .FirstOrDefault();
                        } 
                        else
                        {
                            approverId = null; // Handle unknown document types
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(step), $"Invalid approval step: {step}");
                }

                if (step > 1 && !approverId.HasValue)
                    throw new ApproverConfigurationException($"No approver configured for step {step}.", $"ERR_APPROVER_{step}");

                return approverId.Value;
            }

        }

        #endregion

        #region Create Document
        public DocumentList CreateDocumentRecord(DocumentFormViewModel model)
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

        public DocumentList CreateDocumentIsoRecord(DocumentMasterModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Model cannot be null");

            var document = new DocumentList
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

            db.DocumentLists.Add(document);
            db.SaveChanges();

            return document;
        }


        public void ProcessDocumentDetails(DocumentFormViewModel model, int documentId)
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

        public void ProcessDocumentDetail(DocumentMasterModel model, int documentId)
        {
            try
            {
                var detail = model.DocumentDetail;
                if (detail == null) return;

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
            catch (Exception ex)
            {
                throw new Exception("Error processing document detail", ex);
            }
        }

        public void ProcessSelectedAreas(DocumentFormViewModel model, int documentId)
        {
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

        public void ProcessSelectedArea(DocumentMasterModel model, int documentId)
        {
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

        public void ProcessDocumentDetailSingle(DocumentMasterModel model, int documentId)
        {
            var detail = new DocumentDetail
            {
                LId = documentId,
                WS_number = model.DocumentDetail.WS_number,
                WS_name = model.DocumentDetail.WS_name,
                Revision = model.DocumentDetail.Revision,
                Num_pages = model.DocumentDetail.Num_pages,
                Num_copies = model.DocumentDetail.Num_copies,
                Change_detail = model.DocumentDetail.Change_detail,
                File_excel = model.DocumentDetail.File_excel,
                File_pdf = model.DocumentDetail.File_pdf
            };

            db.DocumentDetails.Add(detail);
        }

        #endregion 

        #region Update Document
        public void UpdateDocumentRecord(DocumentList document, DocumentFormViewModel model)
        {
            document.Status = DocumentStatus.PendingApproval;
            document.Request_type = model.Request_type;
            document.Document_type = model.Document_type;
            document.Effective_date = model.Effective_date;
            document.Updated_at = DateTime.Now;
        }

        public void UpdateDocumentDetails(DocumentFormViewModel model, int documentId)
        {
            var existingDetails = db.DocumentDetails.Where(d => d.LId == documentId).ToList();
            db.DocumentDetails.RemoveRange(existingDetails);
            ProcessDocumentDetails(model, documentId);
        }

        public void UpdateSelectedAreas(DocumentFormViewModel model, int documentId)
        {
            var existingAreas = db.DocumentFormAreas.Where(a => a.LId == documentId).ToList();
            db.DocumentFormAreas.RemoveRange(existingAreas);
            ProcessSelectedAreas(model, documentId);
        }

        public void UpdateApprovalWorkflow(DocumentList document,int documentId, int requesterId)
        {
            if (requesterId == 0)
                throw new ApproverConfigurationException("Requester ID is missing or invalid.", "ERR_INVALID_REQUESTER");


            for (int step = 1; step <= 3; step++)
            {
                var approverId = StepNameHelper.GetApproverIdForStep(document, db, step, requesterId);
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

        #endregion 

        #region Create Approval
        public void CreateApprovalWorkflow(DocumentList document,int documentId, int requesterId)
        {
            if (requesterId == 0)
                throw new ApproverConfigurationException("Requester ID is missing or invalid.", "ERR_INVALID_REQUESTER");

            var steps = new List<DocumentApprovalStep>();

            for (int step = 1; step <= 3; step++)
            {
                //var approverId = GetApproverIdForStep(step, requesterId);
                var approverId = StepNameHelper.GetApproverIdForStep(document, db, step, requesterId);

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
            db.SaveChanges();
        }

        //public int GetApproverIdForStep(int step, int requesterId)
        //{
        //    int? approverId;

        //    switch (step)
        //    {
        //        case 1:
        //            approverId = 0;
        //            break;
        //        case 2:
        //            approverId = db.UserRights
        //                .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
        //                .Where(x => x.ur.RIH_Id == 75)
        //                .Select(x => (int?)x.ud.USE_Usercode)
        //                .FirstOrDefault();
        //            break;
        //        case 3:
        //            approverId = db.UserRights
        //                .Join(db.UserDetails, ur => ur.USE_Id, ud => ud.USE_Id, (ur, ud) => new { ur, ud })
        //                .Where(x => x.ur.RIH_Id == 76)
        //                .Select(x => (int?)x.ud.USE_Usercode)
        //                .FirstOrDefault();
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(step), $"Invalid approval step: {step}");
        //    }

        //    if (!approverId.HasValue)
        //        throw new ApproverConfigurationException($"No approver configured for step {step}.", $"ERR_APPROVER_{step}");

        //    return approverId.Value;
        //}

        //public string GetStepName(int step)
        //{
        //    switch (step)
        //    {
        //        case 1:
        //            return "Department Head (Asst manager up)";
        //        case 2:
        //            return "Document Control Center (DCC)";
        //        case 3:
        //            return "Quality Management Representative (QMR)";
        //        default:
        //            return "Unknown Approval Step";
        //    }
        //}
        #endregion

        #region Input data to excel Common

        public dynamic GetRawData(int id)
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
        public void FillStaticData(IXLWorksheet ws, DocumentFormViewModel vm)
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
        public List<string> GetSectionCodes(int id)
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
        public void FillRequestType(IXLWorksheet ws, string requestType)
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
        public void FillDocumentType(IXLWorksheet ws, string type)
        {
            if (type == "controlled") ws.Cell("A22").Value = "✔";
            else if (type == "uncontrolled") ws.Cell("A24").Value = "✔";
        }
        public void FillFileExistChecks(IXLWorksheet ws, List<DocumentDetail> details)
        {
            ws.Cell("A12").Value = details.Any(d => !string.IsNullOrEmpty(d.File_excel)) ? "✔" : "✖";
            ws.Cell("H12").Value = details.Any(d => !string.IsNullOrEmpty(d.File_pdf)) ? "✔" : "✖";
        }
        public void FillSectionCodes(IXLWorksheet ws, List<string> codes)
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
        public void FillReviewChecks(IXLWorksheet ws, DocumentFormViewModel vm)
        {
            ws.Cell(vm.FMEA_Review == true ? "AB24" : "X24").Value = "✔";
            ws.Cell(vm.ControlPlan_Review == true ? "AB25" : "X25").Value = "✔";
            ws.Cell(vm.ProcessFlow_Review == true ? "AB26" : "X26").Value = "✔";
        }
        public void FillApprovers(IXLWorksheet ws, List<ApproverInfo> approvers, DocumentFormViewModel vm)
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
        public void FillApproversEmr(IXLWorksheet ws, List<ApproverInfo> approvers, DocumentFormViewModel vm)
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

                            picture.MoveTo(ws.Cell("E34"), 0, yOffsetUpPx);
                            ws.Cell("J34").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;

                        case 2:
                            picture.MoveTo(ws.Cell("E38"), 0, yOffsetUpPx);
                            ws.Cell("J38").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;

                        case 3:
                            picture.MoveTo(ws.Cell("W28"), 0, yOffsetUpPx - 4);
                            ws.Cell("AB28").Value = $"Date   {approver.ApprovedAt?.ToString("dd/MM/yyyy") ?? "Pending"}";
                            break;
                    }
                }
            }
        }
        public void FillDetails(IXLWorksheet ws, List<DocumentDetail> details)
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

        #region File Upload Handling

        /// <summary>
        /// Handles file uploads for PDF and Excel files related to document details.
        /// </summary>
        /// <param name="model">Document form view model</param>
        /// <param name="documentId">Associated Document ID</param>
        public void ProcessFileUploads(DocumentFormViewModel model, int documentId)
        {
            string pathExcel = ConfigurationManager.AppSettings["path_Document_Excel"];
            string pathPdf = ConfigurationManager.AppSettings["path_Document_Pdf"];

            Directory.CreateDirectory(pathExcel);
            Directory.CreateDirectory(pathPdf);

            // And in the ProcessFileUploads method, change:
            // var files = Request.Files;

            // to:
            var files = HttpContext.Current.Request.Files;

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

        public void ProcessFileUpload(DocumentMasterModel model, int documentId)
        {
            
            string pathExcel = ConfigurationManager.AppSettings["path_Document_Excel"];
            string pathPdf = ConfigurationManager.AppSettings["path_Document_Pdf"];

            Directory.CreateDirectory(pathExcel);
            Directory.CreateDirectory(pathPdf);

            var files = HttpContext.Current.Request.Files;

            // สำหรับ PDF
            var filePdf = files["DocumentDetail.File_pdf"];
            if (filePdf != null && filePdf.ContentLength > 0)
            {
                string pdfFileName = GenerateUniqueFileName("PDF", documentId, filePdf.FileName);
                filePdf.SaveAs(Path.Combine(pathPdf, pdfFileName));
                model.DocumentDetail.File_pdf = pdfFileName;
            }

            // สำหรับ Excel
            var fileExcel = files["DocumentDetail.File_excel"];
            if (fileExcel != null && fileExcel.ContentLength > 0)
            {
                string excelFileName = GenerateUniqueFileName("EXCEL", documentId, fileExcel.FileName);
                fileExcel.SaveAs(Path.Combine(pathExcel, excelFileName));
                model.DocumentDetail.File_excel = excelFileName;
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

        #region Area & User Helpers

        /// <summary>
        /// Returns a list of available request types for dropdown.
        /// </summary>

        public List<CustomSelectListItem> GetRequestTypes()
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
        /// Loads all available document sections to be selected in the form.
        /// </summary>
        public List<AreaItemViewModel> LoadAvailableAreas()
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
        public int GetCurrentUserId(HttpSessionStateBase session)
        {
            if (session["UserCode"] != null)
                return Convert.ToInt32(session["UserCode"]);

            throw new UnauthorizedAccessException("ไม่พบข้อมูลผู้ใช้งาน");
        }

        /// <summary>
        /// Checks whether the user can edit a document based on ownership and status.
        /// </summary>
        public bool CanEditDocument(DocumentList document, int userId)
        {
            try
            {
                var finddepartment = db.UserDetails
                    .Where(u => u.USE_Usercode == userId)
                    .Select(u => u.Department)
                    .FirstOrDefault();

                var requesterdepartment = db.UserDetails
                    .Where(u => u.USE_Usercode == document.Requester_id)
                    .Select(u => u.Department)
                    .FirstOrDefault();

                return (document.Requester_id == userId || finddepartment == requesterdepartment) &&
                       document.Status != DocumentStatus.Complete;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }
    }
}
