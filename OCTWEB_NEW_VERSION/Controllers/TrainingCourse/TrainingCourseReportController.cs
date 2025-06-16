using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.TrainingCourse
{
    [Authorize]
    public class TrainingCourseReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_wstrain_hr = ConfigurationManager.AppSettings["path_wstrain_hr"];
        private string path_wstrain = ConfigurationManager.AppSettings["path_wstrain"];

        //
        // GET: /TrainingCourseReport/CourseWSList
        [CustomAuthorize(56)]//Training courses report
        public ActionResult CourseWSList(ManageWSListModel model)
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            int pageSize = 30;
            int pageIndex = 1;
            pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

            int? selectedStatusId = model.selected_status.HasValue ? model.selected_status : null;
            string search_wsnumber = !String.IsNullOrEmpty(model.search_wsnumber) ? model.search_wsnumber : null;
            string search_area = !String.IsNullOrEmpty(model.search_area) ? model.search_area : null;

            IEnumerable<Emp_TrainingWsHeader> query = db.Emp_TrainingWsHeader;
            if (selectedStatusId.HasValue)
            {
                var wstheader_id = db.Emp_TrainingWsDate.Select(x => x.E_TrainWsHeader_Id).ToList();
                if (selectedStatusId == 0)// 0 Upload Complete
                {
                    query = query.Where(x => wstheader_id.Contains(x.Id));
                }
                else//1 Waiting Upload
                {
                    query = query.Where(x => !wstheader_id.Contains(x.Id));
                }
            }
            if (!String.IsNullOrEmpty(search_wsnumber))
            {
                query = query.Where(x => !String.IsNullOrEmpty(x.Train_Name) && x.Train_Name.ToLowerInvariant().Contains(search_wsnumber.ToLowerInvariant()));
            }
            if (!String.IsNullOrEmpty(search_area))
            {
                query = query.Where(x => !String.IsNullOrEmpty(x.Training_Ws_Temp_id) && x.Training_Ws_Temp_id.ToLowerInvariant().Contains(search_area.ToLowerInvariant()));
            }

            var train_list = query.Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new { h = h, t = t })
                .OrderByDescending(o => o.h.Id).Select(x => new Emp_TrainingWsHeaderModel
                {
                    Id = x.h.Id,
                    CourseHeader = String.Concat(x.t.Train_Header, " ", x.t.Train_HeaderThai),
                    Trai_Temporary_Id = x.h.Trai_Temporary_Id,
                    Train_Date = x.h.Train_Date,
                    Train_DateWS = x.h.Train_DateWS,
                    Train_EmpId = x.h.Train_EmpId,
                    Train_Hour = x.h.Train_Hour,
                    Train_Location = x.h.Train_Location,
                    Train_Min = x.t.Trai_Rev,
                    Train_Name = x.t.Train_NumberWS,
                    Train_Price = x.h.Train_Price,
                    Train_WS_Temp = x.h.Training_Ws_Temp_id,
                    area = x.t.Trai_NameArea,
                    status = (from twsd in db.Emp_TrainingWsDate
                              where twsd.E_TrainWsHeader_Id == x.h.Id
                              select twsd.Id).FirstOrDefault(),
                    downloadfile = (from twsd in db.Emp_TrainingWsDate
                                    where twsd.E_TrainWsHeader_Id == x.h.Id
                                    select twsd.Train_WsName).FirstOrDefault(),

                }).ToList();

            //---------------------------------------- ADD trainingDate Data---------------------------------

            var trainingDates = db.Emp_TrainingWsDate.ToList();
            model.TrainingDate = trainingDates;

            //----------------------------------------------------------------------------------------------
            // ค้นหาข้อมูลของ wstrainheader
            var trainingDateIds = trainingDates.Select(td => td.E_TrainWsHeader_Id).ToList();

            var queryList = db.Emp_TrainingWsHeader
                .Where(s => trainingDateIds.Contains(s.Id))
                .ToList();

            var wstrainheader = queryList.Join(db.Emp_TrainingWS_temporary,
                                   h => h.Trai_Temporary_Id,
                                   t => t.Id,
                                   (h, t) => new { h, t })
                             .Select(x => new Emp_TrainingWsHeaderModel
                             {
                                 Id = x.h.Id,
                                 CourseHeader = String.Concat(x.t.Train_Header, " ", x.t.Train_HeaderThai),
                                 Trai_Temporary_Id = x.h.Trai_Temporary_Id,
                                 Train_Date = x.h.Train_Date,
                                 Train_DateWS = x.h.Train_DateWS,
                                 Train_EmpId = x.h.Train_EmpId,
                                 Train_Hour = x.h.Train_Hour,
                                 Train_Location = x.h.Train_Location,
                                 Train_Min = x.t.Trai_Rev,
                                 Train_Name = x.t.Train_NumberWS,
                                 Train_Price = x.h.Train_Price,
                                 area = x.t.Trai_NameArea
                             })
                             .FirstOrDefault();



            if (wstrainheader == null)
            {
                TempData["shortMessage"] = String.Format("This course has been deleted or this link is invalid. [Emp_TrainingWsHeader_id : {0}] ");
                return RedirectToAction("ManageWSList");
            }

            model.WSTrainHeaderModel = wstrainheader;

            // ดึงข้อมูลวันที่ฝึกอบรม
            var date_wstrain = db.Emp_TrainingWsDate.Where(x => x.E_TrainWsHeader_Id == wstrainheader.Id).FirstOrDefault();


            // ดึงรายชื่อพนักงานที่เข้าร่วม
            var traineeLists = db.Emp_TrainingWsDetail
                .Join(db.EmpLists,
                      detail => detail.Emp_Id,
                      emp => emp.EmpID.ToString(),
                      (detail, emp) => new { detail, emp })
                .Select(x => new TraineeModel
                {
                    emp_id = x.emp.EmpID,
                    fname = x.emp.FName_EN,
                    lname = x.emp.LName_EN,
                    nation = x.emp.Nation,
                    dept = x.emp.DeptDesc,
                    position = x.emp.Position,
                    assessment = x.detail.Trian_SkillKnow ?? 0,
                    e_trainWsdate_id = x.detail.E_TrainWsDate_Id
                }).ToList();

            // ดึงรายชื่อพนักงานที่เข้าร่วม (FormerLists)
            var traineeFormerLists = db.Emp_TrainingWsDetail
               .Join(db.Former_EmpList,
                     detail => detail.Emp_Id,
                     fmp => fmp.EmpID.ToString(),
                     (detail, fmp) => new { detail, fmp })
               .Select(x => new TraineeModel
               {
                   emp_id = x.fmp.EmpID,
                   fname = x.fmp.FName_EN,
                   lname = x.fmp.LName_EN,
                   nation = x.fmp.Nation,
                   dept = x.fmp.DeptDesc,
                   position = x.fmp.Position,
                   assessment = x.detail.Trian_SkillKnow ?? 0,
                   e_trainWsdate_id = x.detail.E_TrainWsDate_Id
               }).ToList();

            // รวมสอง List โดยใช้ Concat
            model.TraineeLists = traineeLists.Concat(traineeFormerLists).ToList();

            IPagedList<Emp_TrainingWsHeaderModel> trainPageList = train_list.ToPagedList(pageIndex, pageSize);

            List<SelectListItem> SelectStatus_list = new List<SelectListItem>();
            SelectStatus_list.Add(new SelectListItem() { Value = "0", Text = "Upload Complete" });
            SelectStatus_list.Add(new SelectListItem() { Value = "1", Text = "Waiting Upload" });

            model.Emp_TrainingWsHeaderModelPagedList = trainPageList;
            model.SelectStatusId = SelectStatus_list;

            return View(model);
        }
        //
        // POST: /TrainingCourseReport/CourseWSList
        [HttpPost]
        [CustomAuthorize(56)]//Training courses report
        public ActionResult CourseWSList(FormCollection form, ManageWSListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? selectedStatusId = model.selected_status.HasValue ? model.selected_status : null;
                string search_wsnumber = !String.IsNullOrEmpty(model.search_wsnumber) ? model.search_wsnumber : null;
                string search_area = !String.IsNullOrEmpty(model.search_area) ? model.search_area : null;

                IEnumerable<Emp_TrainingWsHeader> query = db.Emp_TrainingWsHeader;
                if (selectedStatusId.HasValue)
                {
                    var wstheader_id = db.Emp_TrainingWsDate.Select(x => x.E_TrainWsHeader_Id).ToList();
                    if (selectedStatusId == 0)// 0 Upload Complete
                    {
                        query = query.Where(x => wstheader_id.Contains(x.Id));
                    }
                    else//1 Waiting Upload
                    {
                        query = query.Where(x => !wstheader_id.Contains(x.Id));
                    }
                }
                if (!String.IsNullOrEmpty(search_wsnumber))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Train_Name) && x.Train_Name.ToLowerInvariant().Contains(search_wsnumber.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(search_area))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Training_Ws_Temp_id) && x.Training_Ws_Temp_id.ToLowerInvariant().Contains(search_area.ToLowerInvariant()));
                }

                var train_list = query.Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new { h = h, t = t })
                    .OrderByDescending(o => o.h.Id).Select(x => new Emp_TrainingWsHeaderModel
                    {
                        Id = x.h.Id,
                        CourseHeader = String.Concat(x.t.Train_Header, " ", x.t.Train_HeaderThai),
                        Trai_Temporary_Id = x.h.Trai_Temporary_Id,
                        Train_Date = x.h.Train_Date,
                        Train_DateWS = x.h.Train_DateWS,
                        Train_EmpId = x.h.Train_EmpId,
                        Train_Hour = x.h.Train_Hour,
                        Train_Location = x.h.Train_Location,
                        Train_Min = x.t.Trai_Rev,
                        Train_Name = x.t.Train_NumberWS,
                        Train_Price = x.h.Train_Price,
                        area = x.t.Trai_NameArea,
                        status = (from twsd in db.Emp_TrainingWsDate
                                  where twsd.E_TrainWsHeader_Id == x.h.Id
                                  select twsd.Id).FirstOrDefault(),
                        downloadfile = (from twsd in db.Emp_TrainingWsDate
                                        where twsd.E_TrainWsHeader_Id == x.h.Id
                                        select twsd.Train_WsName).FirstOrDefault(),
                    }).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsvWS(train_list);
                }

                //---------------------------------------- ADD trainingDate Data---------------------------------

                var trainingDates = db.Emp_TrainingWsDate.ToList();
                model.TrainingDate = trainingDates;

                //----------------------------------------------------------------------------------------------

                // ค้นหาข้อมูลของ wstrainheader
                var trainingDateIds = trainingDates.Select(td => td.E_TrainWsHeader_Id).ToList();

                var queryList = db.Emp_TrainingWsHeader
                    .Where(s => trainingDateIds.Contains(s.Id))
                    .ToList();

                var wstrainheader = queryList.Join(db.Emp_TrainingWS_temporary,
                                       h => h.Trai_Temporary_Id,
                                       t => t.Id,
                                       (h, t) => new { h, t })
                                 .Select(x => new Emp_TrainingWsHeaderModel
                                 {
                                     Id = x.h.Id,
                                     CourseHeader = String.Concat(x.t.Train_Header, " ", x.t.Train_HeaderThai),
                                     Trai_Temporary_Id = x.h.Trai_Temporary_Id,
                                     Train_Date = x.h.Train_Date,
                                     Train_DateWS = x.h.Train_DateWS,
                                     Train_EmpId = x.h.Train_EmpId,
                                     Train_Hour = x.h.Train_Hour,
                                     Train_Location = x.h.Train_Location,
                                     Train_Min = x.t.Trai_Rev,
                                     Train_Name = x.t.Train_NumberWS,
                                     Train_Price = x.h.Train_Price,
                                     area = x.t.Trai_NameArea
                                 })
                                 .FirstOrDefault();



                if (wstrainheader == null)
                {
                    TempData["shortMessage"] = String.Format("This course has been deleted or this link is invalid. [Emp_TrainingWsHeader_id : {0}] ");
                    return RedirectToAction("ManageWSList");
                }

                model.WSTrainHeaderModel = wstrainheader;

                // ดึงข้อมูลวันที่ฝึกอบรม
                var date_wstrain = db.Emp_TrainingWsDate.Where(x => x.E_TrainWsHeader_Id == wstrainheader.Id).FirstOrDefault();


                // ดึงรายชื่อพนักงานที่เข้าร่วม
                var traineeLists = db.Emp_TrainingWsDetail
                    .Join(db.EmpLists,
                          detail => detail.Emp_Id,
                          emp => emp.EmpID.ToString(),
                          (detail, emp) => new { detail, emp })
                    .Select(x => new TraineeModel
                    {
                        emp_id = x.emp.EmpID,
                        fname = x.emp.FName_EN,
                        lname = x.emp.LName_EN,
                        nation = x.emp.Nation,
                        dept = x.emp.DeptDesc,
                        position = x.emp.Position,
                        assessment = x.detail.Trian_SkillKnow ?? 0,
                        e_trainWsdate_id = x.detail.E_TrainWsDate_Id
                    }).ToList();

                // ดึงรายชื่อพนักงานที่เข้าร่วม (FormerLists)
                var traineeFormerLists = db.Emp_TrainingWsDetail
                   .Join(db.Former_EmpList,
                         detail => detail.Emp_Id,
                         fmp => fmp.EmpID.ToString(),
                         (detail, fmp) => new { detail, fmp })
                   .Select(x => new TraineeModel
                   {
                       emp_id = x.fmp.EmpID,
                       fname = x.fmp.FName_EN,
                       lname = x.fmp.LName_EN,
                       nation = x.fmp.Nation,
                       dept = x.fmp.DeptDesc,
                       position = x.fmp.Position,
                       assessment = x.detail.Trian_SkillKnow ?? 0,
                       e_trainWsdate_id = x.detail.E_TrainWsDate_Id
                   }).ToList();

                // รวมสอง List โดยใช้ Concat
                model.TraineeLists = traineeLists.Concat(traineeFormerLists).ToList();

                IPagedList<Emp_TrainingWsHeaderModel> trainPageList = train_list.ToPagedList(pageIndex, pageSize);

                List<SelectListItem> SelectStatus_list = new List<SelectListItem>();
                SelectStatus_list.Add(new SelectListItem() { Value = "0", Text = "Upload Complete" });
                SelectStatus_list.Add(new SelectListItem() { Value = "1", Text = "Waiting Upload" });


                model.Emp_TrainingWsHeaderModelPagedList = trainPageList;
                model.SelectStatusId = SelectStatus_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //For CourseWS
        public ActionResult DownloadFileWS(string fileName)
        {
            string fileName_pdf = String.Concat(fileName, ".pdf");
            string path = Path.Combine(path_wstrain, fileName_pdf);
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                return File(bytes, "application/octet-stream", fileName_pdf);
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                return View("Error");
            }
        }
        //For CourseWS
        public void ExportToCsvWS(List<Emp_TrainingWsHeaderModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    ws_no = v.Train_Name,
                    ws_date = v.Train_DateWS,
                    train_area = "\"" + v.area + "\""
                });

                sb.AppendFormat("{0},{1},{2},{3},{4}",
                    "Item", "Working Standard No.", "Registration WS Date", "Responsible Department"
                     , Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4}",
                        item.item, item.ws_no, item.ws_date, item.train_area
                        , Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CourseWSReport.CSV ");
                response.ContentType = "text/plain";
                response.Write(sb.ToString());
                response.End();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }


        //
        // POST: /TrainingCourseReport/GetWSTrainName/
        [HttpPost]
        public JsonResult GetWSTrainName(string Prefix)
        {
            var wstrainname = (from ws in db.Emp_TrainingWsHeader
                               where ws.Train_Name.StartsWith(Prefix)
                               select new { label = ws.Train_Name, val = ws.Train_Name }).Take(10).ToList();
            return Json(wstrainname);
        }

        //
        // POST: /TrainingCourseReport/GetWSTrainArea/
        [HttpPost]
        public JsonResult GetWSTrainArea(string Prefix)
        {
            var wstrainarea = (from ws in db.Emp_TrainingWsHeader
                               where ws.Training_Ws_Temp_id.StartsWith(Prefix)
                               select new { label = ws.Training_Ws_Temp_id, val = ws.Training_Ws_Temp_id }).Distinct().Take(10).ToList();
            return Json(wstrainarea);
        }



        /******************************************Course**********************************************************************/

        //
        // GET: /TrainingCourseReport/CourseList
        [CustomAuthorize(56)]//Training courses report
        public ActionResult CourseList(CourseListModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                string search_topic = !String.IsNullOrEmpty(model.search_topic) ? model.search_topic : null;
                string search_location = !String.IsNullOrEmpty(model.search_location) ? model.search_location : null;
                DateTime? search_startdate = model.search_startdate;
                DateTime? search_enddate = model.search_enddate;

                IEnumerable<Emp_TrainingHeader> query = db.Emp_TrainingHeader;
                if (search_startdate.HasValue)
                {
                    query = query.Where(x => x.Training_dateSt >= search_startdate.Value);
                }
                if (search_enddate.HasValue)
                {
                    query = query.Where(x => x.Training_dateEnd <= search_enddate.Value);
                }
                if (!String.IsNullOrEmpty(search_topic))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CourseHeader) && x.CourseHeader.ToLowerInvariant().Contains(search_topic.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(search_location))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Train_Location) && x.Train_Location.ToLowerInvariant().Contains(search_location.ToLowerInvariant()));
                }

                var course_list = query.OrderByDescending(o => o.Id).Select(s => new CourseModel
                {
                    course_header_id = s.Id,
                    CourseHeader = s.CourseHeader,
                    TrainerName = s.Train_Name,
                    Location = s.Train_Location,
                    TotalPrice = s.Train_Price,
                    DateOfTrainingStart = s.Training_dateSt.HasValue ? s.Training_dateSt.Value : new DateTime(),
                    DateOfTrainingEnd = s.Training_dateEnd.HasValue ? s.Training_dateEnd.Value : new DateTime(),
                    ImportFilePdf = s.Train_WsFileName,
                    time_str = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                    difftime_str = String.Concat(s.Train_Date, " Days ", s.Train_Hour, " Hours ", s.Train_Min, " Minutes"),
                    cognitive = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Select(x => x.Trian_SkillKnow).ToList().Any() ?
                                 db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Average(a => a.Trian_SkillKnow) * 20 : 0.0,

                }).ToList();
                //Round to 2 decimal
                course_list.ForEach(f => f.cognitive = System.Math.Round(Convert.ToDouble(f.cognitive), 2));
                IPagedList<CourseModel> coursePagedList = course_list.ToPagedList(pageIndex, pageSize);
                model.CourseModelPagedList = coursePagedList;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get TrainingCourseReport CourseList:  ", ex.InnerException.ToString());
                return View("Error");
            }

        }
        //
        // POST: /TrainingCourseReport/CourseList
        [HttpPost]
        [CustomAuthorize(56)]//Training courses report
        public ActionResult CourseList(FormCollection form, CourseListModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                int pageSize = 30;
                int pageIndex = 1;

                string search_topic = !String.IsNullOrEmpty(model.search_topic) ? model.search_topic : null;
                string search_location = !String.IsNullOrEmpty(model.search_location) ? model.search_location : null;
                DateTime? search_startdate = model.search_startdate;
                DateTime? search_enddate = model.search_enddate;

                IEnumerable<Emp_TrainingHeader> query = db.Emp_TrainingHeader;
                if (search_startdate.HasValue)
                {
                    query = query.Where(x => x.Training_dateSt >= search_startdate.Value);
                }
                if (search_enddate.HasValue)
                {
                    query = query.Where(x => x.Training_dateEnd <= search_enddate.Value);
                }
                if (!String.IsNullOrEmpty(search_topic))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CourseHeader) && x.CourseHeader.ToLowerInvariant().Contains(search_topic.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(search_location))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Train_Location) && x.Train_Location.ToLowerInvariant().Contains(search_location.ToLowerInvariant()));
                }

                var course_list = query.OrderByDescending(o => o.Id).Select(s => new CourseModel
                {
                    course_header_id = s.Id,
                    CourseHeader = s.CourseHeader,
                    TrainerName = s.Train_Name,
                    Location = s.Train_Location,
                    DateOfTrainingStart = s.Training_dateSt.HasValue ? s.Training_dateSt.Value : new DateTime(),
                    DateOfTrainingEnd = s.Training_dateEnd.HasValue ? s.Training_dateEnd.Value : new DateTime(),
                    ImportFilePdf = s.Train_WsFileName,
                    time_str = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                    difftime_str = String.Concat(s.Train_Date, " Days ", s.Train_Hour, " Hours ", s.Train_Min, " Minutes"),
                    cognitive = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Select(x => x.Trian_SkillKnow).ToList().Any() ?
                                 db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Average(a => a.Trian_SkillKnow) * 20 : 0.0,

                }).ToList();
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(course_list);
                }

                //Round to 2 decimal
                course_list.ForEach(f => f.cognitive = System.Math.Round(Convert.ToDouble(f.cognitive), 2));

                IPagedList<CourseModel> coursePagedList = course_list.ToPagedList(pageIndex, pageSize);
                model.CourseModelPagedList = coursePagedList;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Post TrainingCourseReport CourseList:  ", ex.InnerException.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseReport/Course
        [CustomAuthorize(56)]//Training courses report
        public ActionResult Course(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                var course = db.Emp_TrainingHeader.Where(x => x.Id == id).Select(s => new CourseModel
                {
                    course_header_id = s.Id,
                    CourseHeader = s.CourseHeader,
                    TrainerName = s.Train_Name,
                    Location = s.Train_Location,
                    TotalPrice = s.Train_Price,
                    DateOfTrainingStart = s.Training_dateSt.HasValue ? s.Training_dateSt.Value : new DateTime(),
                    DateOfTrainingEnd = s.Training_dateEnd.HasValue ? s.Training_dateEnd.Value : new DateTime(),
                    ImportFilePdf = s.Train_WsFileName,
                    time_str = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                    difftime_str = String.Concat(s.Train_Date, " Days ", s.Train_Hour, " Hours ", s.Train_Min, " Minutes"),
                    cognitive = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Select(x => x.Trian_SkillKnow).ToList().Any() ?
                                 db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Average(a => a.Trian_SkillKnow) * 20 : 0.0,

                }).FirstOrDefault();
                if (course == null)
                {
                    TempData["shortMessage"] = String.Format("This course has been deleted or this link is invalid. [Emp_TrainingHeader_id : {0}] ", id);
                    return RedirectToAction("CourseList");
                }
                var emp_id_list = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == course.course_header_id).Select(s => s.Emp_Id).ToList();
                IEnumerable<TraineeModel> trainee_list = db.EmpLists.Where(x => emp_id_list.Contains(x.EmpID.ToString()))
                    .Select(s => new TraineeModel
                    {
                        emp_id = s.EmpID,
                        fname = s.FName_EN,
                        lname = s.LName_EN,
                        nation = s.Nation,
                        dept = s.DeptDesc,
                        position = s.Position,
                        assessment = (from td in db.Emp_TrainingDetail
                                      where td.E_TrainHeader == course.course_header_id
                                      && td.Emp_Id == s.EmpID.ToString()
                                      select td.Trian_SkillKnow.Value).FirstOrDefault(),
                    }).ToList();

                ManageCourseModel model = new ManageCourseModel();
                double cognitive_edit = System.Math.Round(Convert.ToDouble(course.cognitive), 2);
                course.cognitive = cognitive_edit;
                model.CourseModel = course;
                model.TraineeList = trainee_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /TrainingCourseReport/Course Id {0}: {1}", id, ex.ToString());
                return View("Error");
            }
        }
        //For course
        public ActionResult DownloadFile(string fileName)
        {
            string fileName_pdf = String.Concat(fileName, ".pdf");
            string path = Path.Combine(path_wstrain_hr, fileName_pdf);
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                return File(bytes, "application/octet-stream", fileName_pdf);
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                return View("Error");
            }
        }
        //For course
        public void ExportToCsv(List<CourseModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    courseheader = "\"" + v.CourseHeader + "\"",
                    datestart = v.DateOfTrainingStart,
                    dateend = v.DateOfTrainingEnd,
                    loaction = "\"" + v.Location + "\"",
                    trainer = "\"" + v.TrainerName + "\"",
                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5}",
                    "Item", "Course Name", "Date", "Loaction", "Trainer"
                     , Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5}",
                        item.item, item.courseheader, item.datestart, item.loaction, item.trainer
                        , Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CourseReport.CSV ");
                response.ContentType = "text/plain";
                response.Write(sb.ToString());
                response.End();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }



        //public JsonResult GetTopic(string Prefix)
        //{
        //    var coursetopic = (from th in db.Emp_TrainingHeader
        //                       where th.CourseHeader.StartsWith(Prefix)
        //                       select th.CourseHeader)
        //                      .Distinct()
        //                      .OrderBy(x => x)
        //                      .Take(10)
        //                      .Select(x => new { label = x, val = x })
        //                      .ToList();

        //    return Json(coursetopic);
        //}

        //[HttpPost]
        //public JsonResult GetLocation(string Prefix)
        //{
        //    var courselocation = (from th in db.Emp_TrainingHeader
        //                          where th.Train_Location.StartsWith(Prefix)
        //                          select th.Train_Location)
        //                         .Distinct()
        //                         .OrderBy(x => x)
        //                         .Take(10)
        //                         .Select(x => new { label = x, val = x })
        //                         .ToList();

        //    return Json(courselocation);
        //}
        public JsonResult GetAllTopics()
        {
            var coursetopic = db.Emp_TrainingHeader
                                .Select(th => th.CourseHeader)
                                .Distinct()
                                .OrderBy(x => x)
                                .ToList();

            var list = coursetopic.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllLocations()
        {
            var courselocation = db.Emp_TrainingHeader
                                   .Select(th => th.Train_Location)
                                   .Distinct()
                                   .OrderBy(x => x)
                                   .ToList();

            var list = courselocation.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
