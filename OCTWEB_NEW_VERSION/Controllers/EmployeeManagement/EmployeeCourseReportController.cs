using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.EmployeeManagement
{
    [Authorize]
    public class EmployeeCourseReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        //
        // GET: /EmployeeCourseReport/CourseList
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult CourseList(EmployeeCourseReport model, int id)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;
                model.emp_id = id;
         
                int location_id = !String.IsNullOrEmpty(model.search_location) ? Convert.ToInt32(model.search_location): 0;
                int topic_id = !String.IsNullOrEmpty(model.search_topic) ? Convert.ToInt32(model.search_topic): 0;
                DateTime? start_date = model.search_datestart;
                DateTime? end_date = model.search_dateend;

                var course_list = db.Emp_TrainingDetail.Where(x => x.Emp_Id == id.ToString()).Select(s => s.E_TrainHeader).ToList();
                //Select topic
                //List<SelectListItem> selectTopic = db.Emp_TrainingHeader.Where(x => course_list.Contains(x.Id))
                //    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.CourseHeader }).ToList();
                ////Selection location
                //List<SelectListItem> selectLocation = db.Emp_TrainingHeader.Where(x => course_list.Contains(x.Id))
                //   .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Train_Location }).ToList();

                // ดึงข้อมูลมาทั้งหมดก่อน
                var trainingHeaders = db.Emp_TrainingHeader
                    .Where(x => course_list.Contains(x.Id))
                    .ToList(); // << สำคัญ!

                // selectTopic: Group by CourseHeader และ OrderBy ตาม CourseHeader
                List<SelectListItem> selectTopic = trainingHeaders
                    .GroupBy(x => x.CourseHeader)
                    .OrderBy(g => g.Key)
                    .Select(g => new SelectListItem
                    {
                        Value = g.First().Id.ToString(), // ใช้ First ได้ เพราะอยู่ใน memory แล้ว
                        Text = g.Key
                    })
                    .ToList();

                // selectLocation: Group by Train_Location และ OrderBy ตาม Train_Location
                List<SelectListItem> selectLocation = trainingHeaders
                    .GroupBy(x => x.Train_Location)
                    .OrderBy(g => g.Key)
                    .Select(g => new SelectListItem
                    {
                        Value = g.First().Id.ToString(),
                        Text = g.Key
                    })
                    .ToList();


                IEnumerable<Emp_TrainingHeader> query = db.Emp_TrainingHeader;
                if (start_date.HasValue)
                {
                    query = query.Where(x => x.Training_dateSt <= start_date);
                }
                if(end_date.HasValue)
                {
                    query = query.Where(x => x.Training_dateEnd >= end_date);
                }
                if(location_id > 0)
                {
                    query = query.Where(x => x.Id == location_id);
                }
                if (topic_id > 0)
                {
                    query = query.Where(x => x.Id == topic_id);
                }
                var course_model = query.Where(x => course_list.Contains(x.Id))
                    .Select(s => new CourseReportDetail
                    {
                        course_id = s.Id,
                        course_training_name = s.CourseHeader,
                        location = s.Train_Location,
                        start_date = s.Training_dateSt,
                        end_date = s.Training_dateEnd,
                        time = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                        hours = s.Train_Hour,
                        minutes = s.Train_Min,
                        price = s.Train_Price,
                        training_by = s.Train_Name,

                    }).OrderByDescending(o=>o.course_id).ToList();

               
                course_model.ForEach(f =>
                {
                    f.start_date_str = f.start_date.HasValue ? f.start_date.Value.ToString("yyyy-MM-dd") : "";
                    f.end_date_str = f.end_date.HasValue ? f.end_date.Value.ToString("yyyy-MM-dd") : "";
                });


                var sum_hours = course_model
                     .Select(s => double.TryParse(s.hours, out var h) ? h : 0)
                     .Sum();

                var sum_minutes = course_model
                    .Select(s => double.TryParse(s.minutes, out var m) ? m : 0)
                    .Sum();


                TimeSpan t = TimeSpan.FromMinutes(sum_minutes);
                
                model.sum_hours = sum_hours + t.Hours;
                model.sum_minutes = t.Minutes;

                IPagedList<CourseReportDetail> coursePagedList = course_model.ToPagedList(pageIndex, pageSize);

                model.CourseReportDetailPagedList = coursePagedList;
                model.SelectTopic = selectTopic;
                model.SelectLocation = selectLocation;

                return View(model);
            }
            catch(Exception ex){
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }          
        }

        //
        // POST: /EmployeeCourseReport/CourseList
        [HttpPost]
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult CourseList(EmployeeCourseReport model ,FormCollection form)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                
                int location_id = !String.IsNullOrEmpty(model.search_location) ? Convert.ToInt32(model.search_location) : 0;
                int topic_id = !String.IsNullOrEmpty(model.search_topic) ? Convert.ToInt32(model.search_topic) : 0;
                DateTime? start_date = model.search_datestart;
                DateTime? end_date = model.search_dateend;

                var course_list = db.Emp_TrainingDetail.Where(x => x.Emp_Id == model.emp_id.ToString()).Select(s => s.E_TrainHeader).ToList();
                //Select topic
                List<SelectListItem> selectTopic = db.Emp_TrainingHeader.Where(x => course_list.Contains(x.Id))
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.CourseHeader }).ToList();
                //Selection location
                List<SelectListItem> selectLocation = db.Emp_TrainingHeader.Where(x => course_list.Contains(x.Id))
                   .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Train_Location }).ToList();

                IEnumerable<Emp_TrainingHeader> query = db.Emp_TrainingHeader;
                if (start_date.HasValue)
                {
                    query = query.Where(x => x.Training_dateSt <= start_date);
                }
                if (end_date.HasValue)
                {
                    query = query.Where(x => x.Training_dateEnd >= end_date);
                }
                if (location_id > 0)
                {
                    query = query.Where(x => x.Id == location_id);
                }
                if (topic_id > 0)
                {
                    query = query.Where(x => x.Id == topic_id);
                }
                var course_model = query.Where(x => course_list.Contains(x.Id))
                    .Select(s => new CourseReportDetail
                    {
                        course_id = s.Id,
                        course_training_name = s.CourseHeader,
                        location = s.Train_Location,
                        start_date = s.Training_dateSt,
                        end_date = s.Training_dateEnd,
                        time = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                        hours = s.Train_Hour,
                        minutes = s.Train_Min,
                        price = s.Train_Price,
                        training_by = s.Train_Name,

                    }).OrderByDescending(o => o.course_id).ToList();

                //Export Csv
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(course_model);
                }

                //course_model.ForEach(f => f.start_date_str = f.start_date.Value.ToString("yyyy-MM-dd"));
                //course_model.ForEach(f => f.end_date_str = f.end_date.Value.ToString("yyyy-MM-dd"));
                ////Set sum hours minutes
                //var sum_hours = course_model.Select(s => Convert.ToDouble(s.hours)).Sum();
                //var sum_minutes = course_model.Select(s => Convert.ToDouble(s.minutes)).Sum();

                //TimeSpan t = TimeSpan.FromMinutes(sum_minutes);

                //model.sum_hours = sum_hours + t.Hours;
                //model.sum_minutes = t.Minutes;

                IPagedList<CourseReportDetail> coursePagedList = course_model.ToPagedList(pageIndex, pageSize);

                model.CourseReportDetailPagedList = coursePagedList;
                model.SelectTopic = selectTopic;
                model.SelectLocation = selectLocation;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }

        }

        //
        // GET: /EmployeeCourseReport/CourseWSList
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult CourseWSList(EmployeeCourseReport model, int id)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;
                model.emp_id = id;

                //int location_id = !String.IsNullOrEmpty(model.search_location) ? Convert.ToInt32(model.search_location) : 0;
                int topic_id = !String.IsNullOrEmpty(model.search_topic) ? Convert.ToInt32(model.search_topic) : 0;
                DateTime? start_date = model.search_datestart;
                DateTime? end_date = model.search_dateend;

                var course_list = db.Emp_TrainingWsDetail.Where(x => x.Emp_Id == id.ToString()).Select(s => s.E_TrainWsHeader_Id).ToList();
                //Select topic
                List<SelectListItem> selectTopic = db.Emp_TrainingWsHeader.Where(x => course_list.Contains(x.Id))
                    .Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new { h = h, t = t })
                    .Select(s => new SelectListItem { Value = s.h.Id.ToString(), Text = String.Concat(s.t.Train_Header," ",s.t.Train_HeaderThai) }).ToList();
                //Selection location
                List<SelectListItem> selectLocation = db.Emp_TrainingWsHeader
                    .Where(x => course_list.Contains(x.Id))
                    .Select(s => new SelectListItem
                    {
                        Value = s.Train_Location,
                        Text = s.Train_Location
                    })
                    .Distinct()
                    .ToList();

                IEnumerable<Emp_TrainingWsHeader> query = db.Emp_TrainingWsHeader;
                if (start_date.HasValue)
                {
                    query = query.Where(x => x.Train_DateWS == start_date.Value.ToString("yyyy-MM-dd"));
                    //query = query.ToList().Where(a => DateTime.ParseExact(a.Train_DateWS, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= start_date);
                }
                if (end_date.HasValue)
                {
                    query = query.Where(x => x.Train_DateWS == end_date.Value.ToString("yyyy-MM-dd"));
                    //query = query.ToList().Where(a => DateTime.ParseExact(a.Train_DateWS, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= end_date);
                }
                if (!string.IsNullOrEmpty(model.search_location))
                {
                    query = query.Where(x => x.Train_Location == model.search_location);
                }
                if (topic_id > 0)
                {
                    query = query.Where(x => x.Id == topic_id);
                }
                var course_model = query.Where(x => course_list.Contains(x.Id))
                    .Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new { h = h, t = t })
                    .Select(s => new CourseReportDetail
                    {
                        course_id = s.h.Id,
                        number_ws = s.h.Train_Name,
                        course_training_name = String.Concat(s.t.Train_Header, "", s.t.Train_HeaderThai),
                        location = s.h.Train_Location,
                        rev = s.t.Trai_Rev,
                        name_area = s.t.Trai_NameArea,
                        date_ws = s.t.Train_DateWS,

                    }).OrderByDescending(o => o.course_id).ToList();




                //Set sum hours minutes
                var course_h = db.Emp_TrainingWsDate
                .Where(x => course_list.Contains(x.E_TrainWsHeader_Id))
                .ToList();

                var sum_hours = course_h.Select(s => Convert.ToDouble(s.Train_Hour)).Sum();
                var sum_minutes = course_h.Select(s => Convert.ToDouble(s.Train_Min)).Sum();
                TimeSpan tw = TimeSpan.FromMinutes(sum_minutes);
                model.sum_hours = sum_hours + tw.Hours;
                model.sum_minutes = tw.Minutes;
                IPagedList<CourseReportDetail> coursePagedList = course_model.ToPagedList(pageIndex, pageSize);

                
                model.CourseReportDetailPagedList = coursePagedList;
                model.SelectTopic = selectTopic;
                model.SelectLocation = selectLocation;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }          
        }

        //
        // POST: /EmployeeCourseReport/CourseWSList
        [HttpPost]
        [CustomAuthorize(58)]//Can see Training Employee
        public ActionResult CourseWSList(EmployeeCourseReport model, FormCollection form)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
              
                int topic_id = !String.IsNullOrEmpty(model.search_topic) ? Convert.ToInt32(model.search_topic) : 0;
                DateTime? start_date = model.search_datestart;
                DateTime? end_date = model.search_dateend;

                var course_list = db.Emp_TrainingWsDetail.Where(x => x.Emp_Id == model.emp_id.ToString()).Select(s => s.E_TrainWsHeader_Id).ToList();
                //Select topic
                List<SelectListItem> selectTopic = db.Emp_TrainingWsHeader.Where(x => course_list.Contains(x.Id))
                    .Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new {h=h,t=t })
                    .Select(s => new SelectListItem 
                    { 
                        Value = s.h.Id.ToString(), 
                        Text = String.Concat(s.t.Train_Header," ",s.t.Train_HeaderThai) 
                    }).ToList();
                //Selection location
                List<SelectListItem> selectLocation = db.Emp_TrainingWsHeader
                    .Where(x => course_list.Contains(x.Id))
                    .Select(s => new SelectListItem
                    {
                        Value = s.Train_Location,
                        Text = s.Train_Location
                    })
                    .Distinct()
                    .ToList();

                IEnumerable<Emp_TrainingWsHeader> query = db.Emp_TrainingWsHeader;
                if (start_date.HasValue)
                {
                    query = query.Where(x => x.Train_DateWS == start_date.Value.ToString("yyyy-MM-dd"));
                    //query = query.ToList().Where(a => DateTime.ParseExact(a.Train_DateWS, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= start_date);
                }
                if (end_date.HasValue)
                {
                    query = query.Where(x => x.Train_DateWS == end_date.Value.ToString("yyyy-MM-dd"));
                    //query = query.ToList().Where(a => DateTime.ParseExact(a.Train_DateWS, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= end_date);
                }
                if (!string.IsNullOrEmpty(model.search_location))
                {
                    query = query.Where(x => x.Train_Location == model.search_location);
                }
                if (topic_id > 0)
                {
                    query = query.Where(x => x.Id == topic_id);
                }
                var course_model = query.Where(x => course_list.Contains(x.Id))
                    .Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new {h=h,t=t })
                    .Select(s => new CourseReportDetail
                    {
                        course_id = s.h.Id,
                        number_ws = s.h.Train_Name,
                        course_training_name = String.Concat(s.t.Train_Header," ",s.t.Train_HeaderThai),
                        location = s.h.Train_Location,
                        rev = s.t.Trai_Rev,
                        name_area = s.t.Trai_NameArea,
                        date_ws = s.t.Train_DateWS,

                    }).OrderByDescending(o => o.course_id).ToList();

                //Set sum hours minutes
                var course_h = db.Emp_TrainingWsDate
                .Where(x => course_list.Contains(x.E_TrainWsHeader_Id))
                .ToList();

                var sum_hours = course_h.Select(s => Convert.ToDouble(s.Train_Hour)).Sum();
                var sum_minutes = course_h.Select(s => Convert.ToDouble(s.Train_Min)).Sum();
                TimeSpan tw = TimeSpan.FromMinutes(sum_minutes);
                model.sum_hours = sum_hours + tw.Hours;
                model.sum_minutes = tw.Minutes;

                //Export Csv
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsvWS(course_model);
                }

                IPagedList<CourseReportDetail> coursePagedList = course_model.ToPagedList(pageIndex, pageSize);

                model.CourseReportDetailPagedList = coursePagedList;
                model.SelectTopic = selectTopic;
                model.SelectLocation = selectLocation;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //  /EmployeeCourseReport/ExportToCsv
        public void ExportToCsv(List<CourseReportDetail> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                     course_training_name = "\""+ v.course_training_name + "\"",
                     location = "\"" + v.location + "\"",
                     start_date  = v.start_date,
                     end_date  = v.end_date,
                     start_date_str  = v.start_date_str,
                     end_date_str = v.end_date_str,
                     time  = v.time,
                     hours  = v.hours,
                     minutes = v.minutes,
                     price = v.price,
                     training_by  = v.training_by,
                     number_ws  = v.number_ws,
                     rev = v.rev,
                     date_ws = v.date_ws,
                     name_area = "\"" +v.name_area + "\""
                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    "No", 	"Course Training Name", 	"Location", 	"Start Date", 	"End Date", 	
                    "Time", 	"Hours", 	"Minutes" ,	"Price" ,	"Training By "
                     , Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        item.item, item.course_training_name, item.location, item.start_date, item.end_date
                        ,item.time, item.hours, item.minutes, item.price, item.training_by
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
                response.AddHeader("content-disposition", "attachment;filename=EmployeeCourseReport.CSV ");
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


        //  /EmployeeCourseReport/ExportToCsvWS
        public void ExportToCsvWS(List<CourseReportDetail> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    course_training_name = "\"" + v.course_training_name + "\"",
                    location = "\"" + v.location + "\"",
                    start_date = v.start_date,
                    end_date = v.end_date,
                    start_date_str = v.start_date_str,
                    end_date_str = v.end_date_str,
                    time = v.time,
                    hours = v.hours,
                    minutes = v.minutes,
                    price = v.price,
                    training_by = v.training_by,
                    number_ws = v.number_ws,
                    rev = v.rev,
                    date_ws = v.date_ws,
                    name_area = "\"" + v.name_area + "\""
                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}",
                    "No", 	"Number WS", 	"Course Training Name", 	"Line/Process", 	
                    "Revision", 	"Location", 	"Date WS"
                     , Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}",
                        item.item, item.number_ws, item.course_training_name, item.name_area
                        ,item.rev, item.location, item.date_ws
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
                response.AddHeader("content-disposition", "attachment;filename=EmployeeCourseWSReport.CSV ");
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


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
      
    }
}
