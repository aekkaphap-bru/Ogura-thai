using OCTWEB_NET45.Context;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Web.UI;
using OCTWEB_NET45.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.IO;
using System.Web.Security;
using Newtonsoft.Json;
using System.Collections;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using OCTWEB_NET45.Infrastructure;
using PagedList.Mvc;
using System.Text;

namespace OCTWEB_NET45.Controllers.Benefit
{
    [Authorize]
    [CustomAuthorize(71)]
    public class BenefitController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        private string path_pic = ConfigurationManager.AppSettings["path_pic"];
        private string path_ben = ConfigurationManager.AppSettings["path_ben"];

        public ActionResult BenList(SearchBenefitRequestViewModel searchModel, int? page)
        {
            try
            {
                var departments = db.EmpLists
                     .OrderBy(d => d.DeptDesc)
                     .Select(d => new SelectListItem
                     {
                         Value = d.DeptDesc,
                         Text = d.DeptDesc
                     })
                     .Distinct()
                     .ToList();

                // ดึงข้อมูล RequestType
                List<SelectListItem> requestTypes = db.Emp_Benefite.Select(b => new SelectListItem
                {
                    Value = b.TypeBenef,
                    Text = b.TypeBenef
                }).Distinct().ToList();

                List<SelectListItem> yearList = db.Emp_Benefite.Select(b => new SelectListItem
                {
                    Value = b.Years,
                    Text = b.Years
                }).Distinct().ToList();

                // Query
                var benefitsQuery = db.Emp_Benefite
                    .AsNoTracking()
                    .Join(db.EmpLists,
                        b => b.EmpId,
                        e => e.EmpID.ToString(),
                        (b, e) => new BenefitRequestModel
                        {
                            Id = b.Id,
                            EmpId = b.EmpId,
                            Dep = b.Dep,
                            TypeBenef = b.TypeBenef,
                            Relation = b.Relation,
                            EmpName = e.FName_TH,
                            EmpLastname = e.LName_TH,
                            SalaryCycle = b.Months + " " + b.Years,
                            Document = b.Document,
                            CreateTimeFormatted = b.CreateTime.ToString(),
                            CreateTime = b.CreateTime // เพิ่ม field นี้เพื่อใช้ sort
                        });

                // Filter
                if (!string.IsNullOrEmpty(searchModel.EmpId))
                    benefitsQuery = benefitsQuery.Where(b => b.EmpId.Contains(searchModel.EmpId));

                if (!string.IsNullOrEmpty(searchModel.Department))
                    benefitsQuery = benefitsQuery.Where(b => b.Dep.Contains(searchModel.Department));

                if (!string.IsNullOrEmpty(searchModel.RequestType))
                    benefitsQuery = benefitsQuery.Where(b => b.TypeBenef.Contains(searchModel.RequestType));

                if (!string.IsNullOrEmpty(searchModel.Months))
                    benefitsQuery = benefitsQuery.Where(b => b.SalaryCycle.Contains(searchModel.Months));

                if (!string.IsNullOrEmpty(searchModel.Years))
                    benefitsQuery = benefitsQuery.Where(b => b.SalaryCycle.Contains(searchModel.Years));

                int pageSize = 20;
                int currentPage = page ?? 1;

                var filteredBenefits = benefitsQuery
                    .OrderByDescending(b => b.CreateTime)
                    .ToList(); // ยังคงต้อง materialize ก่อน เพื่อใช้ LINQ To Objects

                // Add RowNumber
                var pagedBenefits = filteredBenefits
                    .Select((b, index) => {
                        b.RowNumber = index + 1;
                        return b;
                    })
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new BenefitRequestViewModel
                {
                    BenefitRequestList = pagedBenefits, // after skip/take
                    DepartmentsList = departments,
                    RequestTypesList = requestTypes,
                    YearsList = yearList,
                    CurrentPage = currentPage,
                    TotalPages = (int)Math.Ceiling((double)filteredBenefits.Count / pageSize),
                    SearchModel = searchModel,
                    PathDefaultWS = path_ben
                };

                return View(model);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }


        public ActionResult Create()
        {
            BenefitRequestModel model = new BenefitRequestModel();
            // กำหนดตัวเลือกสำหรับประเภทสวัสดิการต่างๆ
            ViewBag.BenefitTypeOptions = new List<string> { "คลอดบุตร", "แต่งงาน", "งานศพ" };

            // ตัวเลือกความสัมพันธ์สำหรับแต่ละประเภทสวัสดิการ
            ViewBag.BirthRelationOptions = new List<SelectListItem> { new SelectListItem { Value = "บุตร", Text = "บุตร" } };
            ViewBag.MarriageRelationOptions = new List<SelectListItem> {
                new SelectListItem { Value = "สามี", Text = "สามี" },
                new SelectListItem { Value = "ภรรยา", Text = "ภรรยา" }
            };
            ViewBag.FuneralRelationOptions = new List<SelectListItem> {
                new SelectListItem { Value = "บิดา", Text = "บิดา" },
                new SelectListItem { Value = "มารดา", Text = "มารดา" },
                new SelectListItem { Value = "สามี", Text = "สามี" },
                new SelectListItem { Value = "ภรรยา", Text = "ภรรยา" },
                new SelectListItem { Value = "อื่นๆ", Text = "อื่นๆ" }
            };

            // ตัวเลือกคำนำหน้าสำหรับแต่ละประเภทสวัสดิการ
            ViewBag.BirthTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "เด็กหญิง", Text = "เด็กหญิง" },
                new SelectListItem { Value = "เด็กชาย", Text = "เด็กชาย" }
            };
            ViewBag.MarriageTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "นาย", Text = "นาย" },
                new SelectListItem { Value = "นางสาว", Text = "นางสาว" },
                new SelectListItem { Value = "นาง", Text = "นาง" }
            };
            ViewBag.FuneralTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "เด็กหญิง", Text = "เด็กหญิง" },
                new SelectListItem { Value = "เด็กชาย", Text = "เด็กชาย" },
                new SelectListItem { Value = "นางสาว", Text = "นางสาว" },
                new SelectListItem { Value = "นาง", Text = "นาง" },
                new SelectListItem { Value = "นาย", Text = "นาย" }
            };
            return PartialView("Create", model);
        }

        [HttpPost]
        public ActionResult Create(BenefitRequestModel model, IEnumerable<HttpPostedFileBase> UploadedFiles)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();
                    return Json(new { success = false, message = string.Join("\n", errors) });
                }

                using (OCTWEBTESTEntities db = new OCTWEBTESTEntities())
                {
                    bool isDuplicate = db.Emp_Benefite.Any(e =>
                        e.EmpId == model.EmpId &&
                        e.Dep == model.Dep &&
                        e.TypeBenef == model.TypeBenef &&
                        e.TRName == model.TRName &&
                        e.FRName == model.FRName &&
                        e.LRName == model.LRName
                    );

                    if (isDuplicate)
                    {
                        return Json(new { success = false, message = "This benefit record already exists!" }, JsonRequestBehavior.AllowGet);
                    }

                    // ตรวจสอบว่ามีการอัปโหลดไฟล์หรือไม่
                    string fileNamesString = null;
                    List<string> savedFileNames = new List<string>();
                    string path_ben = ConfigurationManager.AppSettings["path_ben"];

                    if (UploadedFiles != null && UploadedFiles.Any())
                    {
                        foreach (var file in UploadedFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string fileExtension = Path.GetExtension(file.FileName);
                                string shortId = Guid.NewGuid().ToString("N").Substring(0, 8); // สร้าง GUID แบบไม่มีขีด
                                string dateForFilename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                                string fileName = $"Benefit_{model.EmpId}_{shortId}_{dateForFilename}{fileExtension}";
                                string fullPath = Path.Combine(path_ben, fileName);

                                // ตรวจสอบว่ามีไฟล์ซ้ำหรือไม่
                                if (System.IO.File.Exists(fullPath))
                                {
                                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                                    fullPath = Path.Combine(path_ben, fileName);
                                }

                                // บันทึกไฟล์ลง Path
                                file.SaveAs(fullPath);
                                savedFileNames.Add(fileName);
                            }
                        }

                        fileNamesString = string.Join(separator: ",", savedFileNames);
                    }

                    // สร้าง Emp_Benefite object
                    Emp_Benefite empBenefit = new Emp_Benefite
                    {
                        EmpId = model.EmpId,
                        Dep = model.Dep,
                        TypeBenef = model.TypeBenef,
                        Relation = model.Relation,
                        TRName = model.TRName,
                        FRName = model.FRName,
                        LRName = model.LRName,
                        Months = model.Months,
                        Years = model.Years,
                        Document = fileNamesString, // บันทึกชื่อไฟล์ลง DB
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };

                    db.Emp_Benefite.Add(empBenefit);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Create Item has been successful" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(71)]
        public ActionResult Edit(int id)
        {
            var model = db.Emp_Benefite
                 .Where(b => b.Id == id)
                 .Join(db.EmpLists,
                     b => b.EmpId,
                     e => e.EmpID.ToString(),
                     (b, e) => new BenefitRequestModel
                     {
                         Id = b.Id,
                         EmpId = b.EmpId,
                         Dep = b.Dep,
                         TypeBenef = b.TypeBenef,
                         Relation = b.Relation,
                         TRName = b.TRName,
                         FRName = b.FRName,
                         LRName = b.LRName,
                         Months = b.Months,
                         Years = b.Years,
                         Document = b.Document,
                         EmpName = e.FName_TH,
                         EmpLastname = e.LName_TH,
                     }).FirstOrDefault();

            // กำหนดตัวเลือกสำหรับประเภทสวัสดิการต่างๆ
            ViewBag.BenefitTypeOptions = new List<string> { "คลอดบุตร", "แต่งงาน", "งานศพ" };

            // ตัวเลือกความสัมพันธ์สำหรับแต่ละประเภทสวัสดิการ
            ViewBag.BirthRelationOptions = new List<SelectListItem> { new SelectListItem { Value = "บุตร", Text = "บุตร" } };
            ViewBag.MarriageRelationOptions = new List<SelectListItem> {
                new SelectListItem { Value = "สามี", Text = "สามี" },
                new SelectListItem { Value = "ภรรยา", Text = "ภรรยา" }
            };
            ViewBag.FuneralRelationOptions = new List<SelectListItem> {
                new SelectListItem { Value = "บิดา", Text = "บิดา" },
                new SelectListItem { Value = "มารดา", Text = "มารดา" },
                new SelectListItem { Value = "สามี", Text = "สามี" },
                new SelectListItem { Value = "ภรรยา", Text = "ภรรยา" },
                new SelectListItem { Value = "อื่นๆ", Text = "อื่นๆ" }
            };

            // ตัวเลือกคำนำหน้าสำหรับแต่ละประเภทสวัสดิการ
            ViewBag.BirthTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "เด็กหญิง", Text = "เด็กหญิง" },
                new SelectListItem { Value = "เด็กชาย", Text = "เด็กชาย" }
            };
            ViewBag.MarriageTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "นาย", Text = "นาย" },
                new SelectListItem { Value = "นางสาว", Text = "นางสาว" },
                new SelectListItem { Value = "นาง", Text = "นาง" }
            };
            ViewBag.FuneralTitleOptions = new List<SelectListItem> {
                new SelectListItem { Value = "เด็กหญิง", Text = "เด็กหญิง" },
                new SelectListItem { Value = "เด็กชาย", Text = "เด็กชาย" },
                new SelectListItem { Value = "นางสาว", Text = "นางสาว" },
                new SelectListItem { Value = "นาง", Text = "นาง" },
                new SelectListItem { Value = "นาย", Text = "นาย" }
            };

            return PartialView(viewName: "Edit", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BenefitRequestModel model, HttpPostedFileBase UploadedFile)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Edit", model);
            }

            var benefit = db.Emp_Benefite.Find(model.Id);
            if (benefit == null)
            {
                return HttpNotFound();
            }

            // ป้องกันข้อมูลซ้ำ (ยกเว้น record ปัจจุบัน)
            bool isDuplicate = db.Emp_Benefite.Any(e =>
                e.Id != model.Id &&
                e.EmpId == model.EmpId &&
                e.Dep == model.Dep &&
                e.TypeBenef == model.TypeBenef &&
                e.TRName == model.TRName &&
                e.FRName == model.FRName &&
                e.LRName == model.LRName);

            if (isDuplicate)
            {
                return Json(new { success = false, message = "ข้อมูลซ้ำในระบบ!" });
            }

            // อัปเดตข้อมูลทั่วไป
            benefit.TypeBenef = model.TypeBenef;
            benefit.Relation = model.Relation;
            benefit.TRName = model.TRName;
            benefit.FRName = model.FRName;
            benefit.LRName = model.LRName;
            benefit.Months = model.Months;
            benefit.Years = model.Years;
            benefit.UpdateTime = DateTime.Now;

            string path_ben = ConfigurationManager.AppSettings["path_ben"];

            // ถ้ามีการอัปโหลดไฟล์ใหม่
            if (UploadedFile != null && UploadedFile.ContentLength > 0)
            {
                // ลบไฟล์เก่า (ถ้ามี)
                if (!string.IsNullOrEmpty(benefit.Document))
                {
                    var oldFiles = benefit.Document.Split(',');
                    foreach (var oldFile in oldFiles)
                    {
                        var oldFilePath = Path.Combine(path_ben, oldFile);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                }

                // อัปโหลดไฟล์ใหม่
                string fileExtension = Path.GetExtension(UploadedFile.FileName);
                string shortId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string dateForFilename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Benefit_{model.EmpId}_{shortId}_{dateForFilename}{fileExtension}";
                string fullPath = Path.Combine(path_ben, fileName);

                // ป้องกันชื่อไฟล์ซ้ำ
                if (System.IO.File.Exists(fullPath))
                {
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{fileExtension}";
                    fullPath = Path.Combine(path_ben, fileName);
                }

                UploadedFile.SaveAs(fullPath);
                benefit.Document = fileName;
            }

            db.SaveChanges();
            return Json(new { success = true, message = "แก้ไขข้อมูลสำเร็จ", JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var benefitRequest = db.Emp_Benefite.FirstOrDefault(b => b.Id == id);
                if (benefitRequest == null)
                {
                    return Json(new { success = false, message = "Record not found for deletion" });
                }

                // Delete document file (if any)
                bool fileDeleted = true;
                if (!string.IsNullOrWhiteSpace(benefitRequest.Document))
                {
                    fileDeleted = DeleteOldFileSafely(benefitRequest.Document);
                }

                db.Emp_Benefite.Remove(benefitRequest);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Record deleted successfully." + (fileDeleted ? "" : " (Note: Unable to delete attached file.)")
                });
            }
            catch (Exception ex)
            {
                LogException(ex); // ระบบ Log ภายใน
                return Json(new
                {
                    success = false,
                    message = "An error occurred while deleting the record: " + ex.Message
                });
            }
        }

        private bool DeleteOldFileSafely(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName)) return true;

                string fullPath = Path.Combine(path_ben, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return true;
                }
                return false; // File ไม่พบ
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        public ActionResult GetEmployeeImage(string empId)
        {
            try
            {
                // สร้างเส้นทางรูปภาพ 
                string fullPath = Path.Combine(path_pic, empId + ".png");

                // ตรวจสอบว่าไฟล์มีอยู่จริง
                if (System.IO.File.Exists(fullPath))
                {
                    // อ่านไฟล์และส่งกลับเป็น FileResult
                    return File(fullPath, "image/jpeg");
                }

                // ถ้าไม่พบไฟล์ ให้ส่งรูปภาพเริ่มต้น
                return File(Server.MapPath("~/static/img/undraw_profile.svg"), "image/svg+xml");
            }
            catch (Exception ex)
            {
                // กรณีเกิดข้อผิดพลาด ส่งรูปภาพเริ่มต้น
                return File(Server.MapPath("~/static/img/undraw_profile.svg"), "image/svg+xml");
            }
        }

        public JsonResult GetEmployeeInfo(string empId)
        {
            try
            {
                // หาข้อมูลพนักงาน
                var employee = db.EmpLists
                    .Where(e => e.EmpID.ToString() == empId)
                    .Select(e => new
                    {
                        EmpName = e.FName_TH,
                        EmpLastname = e.LName_TH,
                        Dep = e.DeptDesc
                    }).FirstOrDefault();

                // ตรวจสอบว่าข้อมูลพนักงานมีอยู่จริง
                if (employee != null)
                {
                    // สร้างเส้นทางรูปภาพ
                    string fullPath = Path.Combine(path_pic, empId + ".png");
                    bool imageExists = System.IO.File.Exists(fullPath);

                    // ส่งข้อมูลกลับไปยัง View
                    return Json(new
                    {
                        EmpName = employee.EmpName,
                        EmpLastname = employee.EmpLastname,
                        Dep = employee.Dep,
                        ImagePath = Url.Action("GetEmployeeImage", new { empId = empId }),
                        ImageExists = imageExists
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        private static readonly Dictionary<string, BenefitDropdownConfig> BenefitConfig = new Dictionary<string, BenefitDropdownConfig>
        {
            { "คลอดบุตร", new BenefitDropdownConfig
                {
                    Relations = new List<string> { "บุตร" },
                    Titles = new List<string> { "เด็กหญิง", "เด็กชาย" },
                    AllowCustom = false
                }
            },
            { "แต่งงาน", new BenefitDropdownConfig
                {
                    Relations = new List<string> { "สามี", "ภรรยา" },
                    Titles = new List<string> { "นาย", "นาง", "นางสาว" },
                    AllowCustom = false
                }
            },
            { "งานศพ", new BenefitDropdownConfig
                {
                    Relations = new List<string> { "บิดา", "มารดา", "สามี", "ภรรยา", "อื่นๆ" },
                    Titles = new List<string> { "เด็กหญิง", "เด็กชาย", "นางสาว", "นาง", "นาย" },
                    AllowCustom = true
                }
            }
        };

        [HttpGet]
        public JsonResult GetBenefitConfig(string type)
        {
            if (BenefitConfig.ContainsKey(type))
            {
                var config = BenefitConfig[type];
                return Json(new
                {
                    relations = config.Relations,
                    titles = config.Titles,
                    allowCustom = config.AllowCustom
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Unknown benefit type." }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Viewfile(string PDF)
        {
            try
            {
                // ตรวจสอบว่าได้รับชื่อไฟล์มาหรือไม่
                if (string.IsNullOrEmpty(PDF))
                {
                    return new HttpStatusCodeResult(400, "Invalid file request");
                }

                // ป้องกัน Directory Traversal (../)
                string fileName = Path.GetFileName(PDF);

                // ระบุพาธไฟล์ (อ่านจาก Web.config หรือกำหนดค่าตรงนี้)
                string filePath = Path.Combine(ConfigurationManager.AppSettings["path_ben"], fileName);

                // ตรวจสอบว่าไฟล์มีอยู่หรือไม่
                if (!System.IO.File.Exists(filePath))
                {
                    return HttpNotFound("File not found");
                }

                // อ่านไฟล์เป็น byte[]
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // ระบุ Content-Type สำหรับ PDF
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error: " + ex.Message);
            }
        }

        private void LogException(Exception ex)
        {
            // Implement proper logging 
            // You can use a logging framework like log4net, NLog, or create a custom logging mechanism
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            // Consider adding more detailed logging
        }

        [HttpPost]
        public ActionResult ExportToCsv(SearchBenefitRequestViewModel searchModel)
        {
            var benefitsQuery = db.Emp_Benefite
                .Join(db.EmpLists,
                    b => b.EmpId,
                    e => e.EmpID.ToString(),
                    (b, e) => new
                    {
                        b.EmpId,
                        e.Title_TH,
                        e.FName_TH,
                        e.LName_TH,
                        b.Dep,
                        b.TypeBenef,
                        b.Relation,
                        b.TRName,
                        b.LRName,
                        b.Months,
                        b.Years,
                        b.CreateTime,
                        b.UpdateTime
                    });

            // Apply filters
            if (!string.IsNullOrEmpty(searchModel.EmpId))
                benefitsQuery = benefitsQuery.Where(b => b.EmpId.Contains(searchModel.EmpId));

            if (!string.IsNullOrEmpty(searchModel.Department))
                benefitsQuery = benefitsQuery.Where(b => b.Dep.Contains(searchModel.Department));

            if (!string.IsNullOrEmpty(searchModel.RequestType))
                benefitsQuery = benefitsQuery.Where(b => b.TypeBenef.Contains(searchModel.RequestType));

            if (!string.IsNullOrEmpty(searchModel.Months))
                benefitsQuery = benefitsQuery.Where(b => b.Months == searchModel.Months);

            if (!string.IsNullOrEmpty(searchModel.Years))
                benefitsQuery = benefitsQuery.Where(b => b.Years == searchModel.Years);

            var data = benefitsQuery.ToList();

            var csv = new StringBuilder();

            // Header
            csv.AppendLine("รหัสพนักงาน,คำนำหน้า,ชื่อ,นามสกุล,แผนก,ประเภทสวัสดิการ,ความสัมพันธ์,ชื่อผู้เกี่ยวข้อง,นามสกุลผู้เกี่ยวข้อง,เดือน,ปี,วันที่สร้าง,วันที่อัปเดต");

            // Rows
            foreach (var item in data)
            {
                csv.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11:yyyy-MM-dd HH:mm:ss}\",\"{12:yyyy-MM-dd HH:mm:ss}\"",
                    item.EmpId,
                    item.Title_TH,
                    item.FName_TH,
                    item.LName_TH,
                    item.Dep,
                    item.TypeBenef,
                    item.Relation,
                    item.TRName,
                    item.LRName,
                    item.Months,
                    item.Years,
                    item.CreateTime ?? DateTime.MinValue,
                    item.UpdateTime ?? DateTime.MinValue
                ));
            }

            //byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
            byte[] buffer = Encoding.GetEncoding("utf-8").GetBytes('\uFEFF' + csv.ToString()); 

            return File(buffer, "text/csv", "Benefit_Export.csv");
        }


        // Add this method to your BenefitController class
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


