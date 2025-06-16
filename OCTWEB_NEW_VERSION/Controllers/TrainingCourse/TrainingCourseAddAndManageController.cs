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
    public class TrainingCourseAddAndManageController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_wstrain_hr = ConfigurationManager.AppSettings["path_wstrain_hr"];

        //
        // GET: /TrainingCourseAddAndManage/CourseList
        [CustomAuthorize(61,33)]//Training courses setup , //Training courses management
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
                    DateOfTrainingStart = s.Training_dateSt.HasValue ? s.Training_dateSt.Value: new DateTime(),
                    DateOfTrainingEnd = s.Training_dateEnd.HasValue ? s.Training_dateEnd.Value: new DateTime(),
                    ImportFilePdf = s.Train_WsFileName,
                    time_str = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                    difftime_str = String.Concat(s.Train_Hour, " Hours ", s.Train_Min, " Minutes"),//s.Train_Date, " Days ",
                    cognitive = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Select(x => x.Trian_SkillKnow).ToList().Any() ?
                                 db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Average(a => a.Trian_SkillKnow) * 20 : 0.0,

                }).ToList();
                //Round to 2 decimal
                course_list.ForEach(f => f.cognitive = System.Math.Round(Convert.ToDouble(f.cognitive), 2));
                IPagedList<CourseModel> coursePagedList = course_list.ToPagedList(pageIndex, pageSize);
                model.CourseModelPagedList = coursePagedList;

                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get TrainingCourseAddAndManage CourseList:  ", ex.InnerException.ToString());
                return View("Error");
            }
            
        }

        //
        // POST: /TrainingCourseAddAndManage/CourseList
        [HttpPost]
        [CustomAuthorize(61,33)]//Training courses setup, //Training courses management
        public ActionResult CourseList(FormCollection form,CourseListModel model)
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
                    TotalPrice = s.Train_Price,
                    DateOfTrainingStart = s.Training_dateSt.HasValue ? s.Training_dateSt.Value : new DateTime(),
                    DateOfTrainingEnd = s.Training_dateEnd.HasValue ? s.Training_dateEnd.Value : new DateTime(),
                    ImportFilePdf = s.Train_WsFileName,
                    time_str = String.Concat(s.Trai_Times, ":", s.Trai_Timess, " - ", s.Trai_TimendT, ":", s.Trai_TimendTs),
                    difftime_str = String.Concat(s.Train_Hour, " Hours ", s.Train_Min, " Minutes"),//s.Train_Date, " Days ",
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
                ViewBag.errorMessage = String.Format("Error Post TrainingCourseAddAndManage CourseList:  ", ex.InnerException.ToString());
                return View("Error");
            }                   
        }

        //
        // GET: /TrainingCourseAddAndManage/ManageCourse
        [CustomAuthorize(61,33)]//Training courses setup, //Training courses management
        public ActionResult ManageCourse(int id)
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
                    difftime_str = String.Concat(s.Train_Hour, " Hours ", s.Train_Min, " Minutes"), //s.Train_Date, " Days ",
                    cognitive = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Select(x => x.Trian_SkillKnow).ToList().Any() ?
                                 db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == s.Id).Average(a => a.Trian_SkillKnow) * 20 : 0.0,

                }).FirstOrDefault();
                if(course == null)
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
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /TrainingCourseAddAndManage/ManageCourse Id {0}: {1}", id,ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseAddAndManage/AddCourse
        [CustomAuthorize(61,33)]//Training courses setup, //Training courses management
        public ActionResult AddCourse()
        {
            try
            {
                CourseModel model = new CourseModel();
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                //Get select employee
                List<SelectListItem> emp_list = (from u in db.EmpLists
                                                 select new SelectListItem
                                                 {
                                                     Text = String.Concat(u.EmpID.ToString()
                                                          , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                          , u.DeptDesc, " ]"),
                                                     Value = u.EmpID.ToString(),
                                                 }).ToList();
                model.EmployeeList = emp_list;
                model.Location = "Ogura Clutch (Thailand) Co.,Ltd.";

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get TrainingCourseAddAndManage AddCourse:  ", ex.InnerException.ToString());
                return View("Error");
            }

        }

        //
        // POST: /TrainingCourseAddAndManage/AddCourse
        [HttpPost]
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult AddCourse(HttpPostedFileBase file, CourseModel model)
        {
            try
            {
                //Get select employee
                List<SelectListItem> emp_list = (from u in db.EmpLists
                                                 select new SelectListItem
                                                 {
                                                     Text = String.Concat(u.EmpID.ToString()
                                                          , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                          , u.DeptDesc, " ]"),
                                                     Value = u.EmpID.ToString(),
                                                 }).ToList();
                model.EmployeeList = emp_list;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                DateTime datetime_start = new DateTime(model.DateOfTrainingStart.Year
                    , model.DateOfTrainingStart.Month, model.DateOfTrainingStart.Day
                    , model.StartTime.Hour, model.StartTime.Minute
                    , model.StartTime.Second, model.StartTime.Millisecond);

                DateTime datetime_end = new DateTime(model.DateOfTrainingEnd.Year
                   , model.DateOfTrainingEnd.Month, model.DateOfTrainingEnd.Day
                   , model.EndTime.Hour, model.EndTime.Minute
                   , model.EndTime.Second, model.EndTime.Millisecond);

                TimeSpan diff_time = datetime_end.AddHours(-model.BreakTime) - datetime_start;
                //Check Invalid Date Time Input
                if (diff_time.Minutes < 0)
                {
                    ViewBag.Message = "Invalid date or time, 'Date of Training (End)' and 'End Time' should be greater than 'Date of Training (Start)' and 'Start Time' ";
                    return View(model);
                }
                //Check file extension
                var supportedTypes = new[] { "pdf" };
                if (file != null)
                {
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.Message = "Invalid file extension, Only PDF file.";
                        return View(model);
                    }
                }
                Emp_TrainingHeader add_course = new Emp_TrainingHeader()
                {
                    CourseHeader = model.CourseHeader,
                    Train_Location = model.Location,
                    Train_Price = model.CoursePrice,
                    Train_Name = model.TrainerName,
                    //Set Date, Time Start
                    Training_dateSt = model.DateOfTrainingStart,
                    Trai_Times = model.StartTime.Hour.ToString(),
                    Trai_Timess = model.StartTime.Minute.ToString(),
                    //Set Date, Time End
                    Training_dateEnd = model.DateOfTrainingEnd,
                    Trai_TimendT = model.EndTime.Hour.ToString(),
                    Trai_TimendTs = model.EndTime.Minute.ToString(),
                    //Set Diff time
                    Train_Date = diff_time.Days.ToString(),
                    Train_Hour = diff_time.Hours.ToString(),
                    Train_Min = diff_time.Minutes.ToString(),
                    //Set Break time
                    Trai_House = model.BreakTime.ToString(),
                    //Don't know
                    Trai_Minutices = diff_time.Minutes.ToString(),
                };
                //Add New Course
                var result = db.Emp_TrainingHeader.Add(add_course);
                db.SaveChanges();
                TempData["shortMessage"] = String.Format("Created successfully, {0} items. ", result.CourseHeader);

                //Save trainee
                if (model.selectedEmp != null && model.selectedEmp.Any())
                {
                    foreach (var i in model.selectedEmp)
                    {
                        Emp_TrainingDetail insertEmp = new Emp_TrainingDetail()
                        {
                            E_TrainHeader = result.Id,
                            Emp_Id = i.ToString(),
                            Trian_SkillKnow = 5,
                        };
                        db.Emp_TrainingDetail.Add(insertEmp);
                    }
                    TempData["shortMessage"] = String.Format("Insert employee successfully, {0} items. ", model.selectedEmp.Count());
                    db.SaveChanges();
                }
                //Save File
                if (file != null)
                {
                    string train_id = result.Id.ToString();
                    //DateTime dt_start = result.Training_dateSt != null ? (DateTime)result.Training_dateSt : DateTime.Today;
                    string train_date_start = result.Training_dateSt.HasValue ? 
                        result.Training_dateSt.Value.ToString("yyyy-MM-dd") : null;
                    string filename = String.Concat("OCT-DC", train_id, "_", train_date_start, "FILE");

                    string _FileName = String.Concat(filename, Path.GetExtension(file.FileName));
                    string _path = Path.Combine(path_wstrain_hr, _FileName);
                    result.Train_WsFileName = filename;
                    db.Entry(result).State = System.Data.Entity.EntityState.Modified;
                    file.SaveAs(_path);
                    db.SaveChanges();

                }
                return RedirectToAction("ManageCourse", new { id = result.Id});
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Post TrainingCourseAddAndManage AddCourse:  ", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseAddAndManage/EditCourse
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult EditCourse(int id)
        {
            CourseModel model = new CourseModel();

            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }

            // Get select employee (current)
            var emp_list = (from u in db.EmpLists
                            select new SelectListItem
                            {
                                Text = string.Concat(u.EmpID.ToString(), " : ", u.FName_EN, " ", u.LName_EN, " [", u.DeptDesc, " ]"),
                                Value = u.EmpID.ToString()
                            }).ToList();

            // Get select employee (former)
            var emp_old = (from u in db.Former_EmpList
                           select new SelectListItem
                           {
                               Text = string.Concat(u.EmpID.ToString(), " : ", u.FName_EN, " ", u.LName_EN, " [", u.DeptDesc, " ]"),
                               Value = u.EmpID.ToString()
                           }).ToList();

            model.EmployeeList = emp_list.Concat(emp_old).ToList();

            try
            {
                var courseheader = db.Emp_TrainingHeader
                    .Where(x => x.Id == id)
                    .Select(s => new Emp_TrainingHeaderModel()
                    {
                        Id = s.Id,
                        CourseHeader = s.CourseHeader,
                        Train_Name = s.Train_Name,
                        Train_Location = s.Train_Location,
                        Train_Price = s.Train_Price ?? 0,
                        Train_WsFileName = s.Train_WsFileName,

                        Training_dateSt = s.Training_dateSt ?? DateTime.MinValue,
                        Trai_Times = s.Trai_Times,
                        Trai_Timess = s.Trai_Timess,

                        Training_dateEnd = s.Training_dateEnd ?? DateTime.MinValue,
                        Trai_TimendT = s.Trai_TimendT,
                        Trai_TimendTs = s.Trai_TimendTs,

                        Train_Date = s.Train_Date,
                        Train_Hour = s.Train_Hour,
                        Train_Min = s.Train_Min,

                        Trai_House = s.Trai_House,
                        Trai_Minutices = s.Trai_Minutices
                    }).FirstOrDefault();

                if (courseheader != null)
                {
                    model.THeaderModel = courseheader;
                    model.CourseHeader = courseheader.CourseHeader;
                    model.TrainerName = courseheader.Train_Name;
                    model.CoursePrice = (int)(courseheader.Train_Price);
                    model.DateOfTrainingStart = courseheader.Training_dateSt;
                    model.DateOfTrainingEnd = courseheader.Training_dateEnd;

                    // Convert time safely
                    int start_hours = int.TryParse(courseheader.Trai_Times, out var sh) ? sh : 0;
                    int start_minutes = int.TryParse(courseheader.Trai_Timess, out var sm) ? sm : 0;
                    int end_hours = int.TryParse(courseheader.Trai_TimendT, out var eh) ? eh : 0;
                    int end_minutes = int.TryParse(courseheader.Trai_TimendTs, out var em) ? em : 0;
                    int breaktime = int.TryParse(courseheader.Trai_House, out var bt) ? bt : 0;

                    // Check valid date before creating DateTime
                    if (courseheader.Training_dateSt != DateTime.MinValue)
                    {
                        model.StartTime = new DateTime(
                            courseheader.Training_dateSt.Year,
                            courseheader.Training_dateSt.Month,
                            courseheader.Training_dateSt.Day,
                            start_hours,
                            start_minutes,
                            0);
                    }

                    if (courseheader.Training_dateEnd != DateTime.MinValue)
                    {
                        model.EndTime = new DateTime(
                            courseheader.Training_dateEnd.Year,
                            courseheader.Training_dateEnd.Month,
                            courseheader.Training_dateEnd.Day,
                            end_hours,
                            end_minutes,
                            0);
                    }

                    model.BreakTime = breaktime;
                    model.ImportFilePdf = courseheader.Train_WsFileName;
                    model.Location = courseheader.Train_Location;
                    model.course_header_id = courseheader.Id;
                }

                // Get employee list who registered this course
                var emp_id = db.Emp_TrainingDetail
                    .Where(x => x.E_TrainHeader == id)
                    .Select(s => s.Emp_Id)
                    .ToList();

                if (emp_id != null && emp_id.Any())
                {
                    model.selectedEmp = emp_id.Select(e => int.TryParse(e, out var val) ? val : 0).Where(x => x > 0).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = $"Error EditCourse: {ex.Message}";
                return View("Error");
            }
        }


        //
        // POST: /TrainingCourseAddAndManage/EditCourse
        [HttpPost]
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult EditCourse(HttpPostedFileBase file, CourseModel model)
        {
            try
            {
                //Get select employee
                List<SelectListItem> emp_list = (from u in db.EmpLists
                                                 select new SelectListItem
                                                 {
                                                     Text = String.Concat(u.EmpID.ToString()
                                                          , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                          , u.DeptDesc, " ]"),
                                                     Value = u.EmpID.ToString(),
                                                 }).ToList();
                model.EmployeeList = emp_list;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                DateTime datetime_start = new DateTime(model.DateOfTrainingStart.Year
                    , model.DateOfTrainingStart.Month, model.DateOfTrainingStart.Day
                    , model.StartTime.Hour, model.StartTime.Minute
                    , model.StartTime.Second, model.StartTime.Millisecond);

                DateTime datetime_end = new DateTime(model.DateOfTrainingEnd.Year
                   , model.DateOfTrainingEnd.Month, model.DateOfTrainingEnd.Day
                   , model.EndTime.Hour, model.EndTime.Minute
                   , model.EndTime.Second, model.EndTime.Millisecond);

                TimeSpan diff_time = datetime_end.AddHours(-model.BreakTime) - datetime_start;
                //Check Invalid Date Time Input
                if (diff_time.Minutes < 0)
                {
                    ViewBag.Message = "Invalid date or time, 'Date of Training (End)' and 'End Time' should be greater than 'Date of Training (Start)' and 'Start Time' ";
                    return View(model);
                }
                //Check file extension
                var supportedTypes = new[] { "pdf" };
                if (file != null)
                {
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.Message = "Invalid file extension, Only PDF file.";
                        return View(model);
                    }
                }
                Emp_TrainingHeader edit_course = db.Emp_TrainingHeader.Where(x => x.Id == model.THeaderModel.Id).FirstOrDefault();
                edit_course.CourseHeader = model.CourseHeader;
                edit_course.Train_Location = model.Location;
                edit_course.Train_Price = model.CoursePrice;
                edit_course.Train_Name = model.TrainerName;
                //Set Date, Time Start
                edit_course.Training_dateSt = model.DateOfTrainingStart;
                edit_course.Trai_Times = model.StartTime.Hour.ToString();
                edit_course.Trai_Timess = model.StartTime.Minute.ToString();
                //Set Date, Time End
                edit_course.Training_dateEnd = model.DateOfTrainingEnd;
                edit_course.Trai_TimendT = model.EndTime.Hour.ToString();
                edit_course.Trai_TimendTs = model.EndTime.Minute.ToString();
                //Set Diff time
                edit_course.Train_Date = diff_time.Days.ToString();
                edit_course.Train_Hour = diff_time.Hours.ToString();
                edit_course.Train_Min = diff_time.Minutes.ToString();
                //Set Break time
                edit_course.Trai_House = model.BreakTime.ToString();
                //Don't know
                edit_course.Trai_Minutices = diff_time.Minutes.ToString();

                //Save File
                if (file != null)
                {
                    //Delete old file
                    if (!String.IsNullOrEmpty(edit_course.Train_WsFileName))
                    {
                        string _FileName_old = String.Concat(edit_course.Train_WsFileName, ".pdf");
                        string _path_old = Path.Combine(path_wstrain_hr, _FileName_old);
                        if (System.IO.File.Exists(_path_old))
                        {
                            System.IO.File.Delete(_path_old);
                        }
                    }
                    //Save New file
                    string train_id = model.THeaderModel.Id.ToString();
                    string train_date_start = model.DateOfTrainingStart.ToString("yyyy-MM-dd");
                    string filename = String.Concat("OCT-DC", train_id, "_", train_date_start, "FILE");

                    string _FileName = String.Concat(filename, Path.GetExtension(file.FileName));
                    string _path = Path.Combine(path_wstrain_hr, _FileName);

                    edit_course.Train_WsFileName = filename;

                    file.SaveAs(_path);
                }
                //Edit Course modified
                db.Entry(edit_course).State = System.Data.Entity.EntityState.Modified;
                TempData["shortMessage"] = String.Format("Edited successfully, {0} . ", model.CourseHeader);

                //Get Employee
                var emp_id_list = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == model.THeaderModel.Id).Select(s => s.Emp_Id).ToList();
                var oldEmpId = emp_id_list.Any() ? emp_id_list.Select(int.Parse).ToList() : new List<int>();
                var newEmpId = model.selectedEmp != null && model.selectedEmp.Any() ? model.selectedEmp : new List<int>();

                //For Delete employee in Emp_TrainingDetail
                var oldEmpId_Not_selectedEmp = oldEmpId.Any() ? oldEmpId.Except(newEmpId).ToList() : new List<int>();
                //For Add New employee in Emp_trainingDetail
                var selectedEmp_Not_oldEmpId = newEmpId.Any() ? newEmpId.Except(oldEmpId).ToList() : new List<int>();

                //Delete Employee
                if (oldEmpId_Not_selectedEmp.Any())
                {
                    var old = oldEmpId_Not_selectedEmp.Select(s => s.ToString()).ToList();
                    var coursedetail = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == model.THeaderModel.Id
                        && old.Contains(x.Emp_Id)).ToList();
                    //Remove all
                    db.Emp_TrainingDetail.RemoveRange(coursedetail);
                }
                //Add New 
                foreach (var i in selectedEmp_Not_oldEmpId)
                {
                    Emp_TrainingDetail insertEmp = new Emp_TrainingDetail()
                    {
                        E_TrainHeader = edit_course.Id,
                        Emp_Id = i.ToString(),
                        Trian_SkillKnow = 5,
                    };
                    db.Emp_TrainingDetail.Add(insertEmp);
                    TempData["shortMessage"] = String.Format("Insert employee successfully, {0} items. ", selectedEmp_Not_oldEmpId.Count());
                }
                db.SaveChanges();

                return RedirectToAction("ManageCourse", new { id = model.course_header_id });
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Post TrainingCourseAddAndManage/EditCourse: ", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseAddAndManage/UpdateAssessment
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult UpdateAssessment(int id)
        {
            try
            {
                ManageCourseModel model = new ManageCourseModel();
                var emp_id_list = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == id).Select(s => s.Emp_Id).ToList();
                if (!emp_id_list.Any())
                {
                    TempData["shortMessage"] = String.Format("No employee found in this course. ");
                    return RedirectToAction("ManageCourse", new { id = id });
                }
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
                                      where td.E_TrainHeader == id
                                      && td.Emp_Id == s.EmpID.ToString()
                                      select td.Trian_SkillKnow.Value).FirstOrDefault(),
                    }).ToList();
                CourseModel coursemodel = new CourseModel();
                coursemodel.course_header_id = id;

                model.CourseModel = coursemodel;
                model.TraineeList = trainee_list;

                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get TrainingCourseAddAndManage/UpdateAssessment: {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: /TrainingCourseAddAndManage/UpdateAssessment
        [HttpPost]
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult UpdateAssessment(FormCollection form, ManageCourseModel model)
        {
            try 
            {
                var header_id = model.CourseModel.course_header_id;
                List<string> notgood_list = new List<string>();
                List<string> fair_list = new List<string>();
                List<string> good_list = new List<string>();
                List<string> excellent_list = new List<string>();
                foreach (var key in form.AllKeys)
                {
                    string am_value = form[key];
                    if(am_value == "NotGood")
                    {
                        notgood_list.Add(key);
                    }
                    else if (am_value == "Fair")
                    {
                        fair_list.Add(key);
                    }
                    else if (am_value == "Good")
                    {
                        good_list.Add(key);
                    }
                    else if (am_value == "Excellent")
                    {
                        excellent_list.Add(key);
                    }
                }
                //NotGood
                if (notgood_list.Any())
                { 
                    foreach (var emp_id in notgood_list)
                    {
                        var coursedetail = db.Emp_TrainingDetail
                           .Where(x => x.E_TrainHeader == header_id && x.Emp_Id == emp_id).FirstOrDefault();
                        coursedetail.Trian_SkillKnow = 1;
                        db.Entry(coursedetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Fair
                if (fair_list.Any())
                {
                    foreach (var emp_id in fair_list)
                    {
                        var coursedetail = db.Emp_TrainingDetail
                           .Where(x => x.E_TrainHeader == header_id && x.Emp_Id == emp_id).FirstOrDefault();
                        coursedetail.Trian_SkillKnow = 2;
                        db.Entry(coursedetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Good
                if (good_list.Any())
                {
                    foreach (var emp_id in good_list)
                    {
                        var coursedetail = db.Emp_TrainingDetail
                           .Where(x => x.E_TrainHeader == header_id && x.Emp_Id == emp_id).FirstOrDefault();
                        coursedetail.Trian_SkillKnow = 3;
                        db.Entry(coursedetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Excellent
                if (excellent_list.Any())
                {
                    foreach (var emp_id in excellent_list)
                    {
                        var coursedetail = db.Emp_TrainingDetail
                           .Where(x => x.E_TrainHeader == header_id && x.Emp_Id == emp_id).FirstOrDefault();
                        coursedetail.Trian_SkillKnow = 5;
                        db.Entry(coursedetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Save edit skill
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Update assessment successfully, Excellent: {0}, Good: {1}, Fair: {2}, Not Good: {3}"
                    , excellent_list.Count(),good_list.Count(),fair_list.Count(),notgood_list.Count());
                return RedirectToAction("ManageCourse", new { id = model.CourseModel.course_header_id });
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get TrainingCourseAddAndManage/UpdateAssessment: {0}", ex.ToString());
                return View("Error");
            }

        }


        //
        // GET: /TrainingCourseAddAndManage/DeleteCourse
        [CustomAuthorize(61, 33)]//Training courses setup ,//Training courses management
        public ActionResult DeleteCourse(int id)
        {
            try
            {
                string filename = null;
                string coursname = " ";
                var courseheader = db.Emp_TrainingHeader.Where(x => x.Id == id).FirstOrDefault();
                var coursedetail = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == id).ToList();

                if (courseheader != null)
                {
                    filename = courseheader.Train_WsFileName;
                    coursname = courseheader.CourseHeader;
                    db.Emp_TrainingHeader.Remove(courseheader);
                }
                if (coursedetail.Any())
                {
                    db.Emp_TrainingDetail.RemoveRange(coursedetail);
                }

                db.SaveChanges();
                //Delete file
                if (!String.IsNullOrEmpty(filename))
                {
                    string _FileName = String.Concat(filename, ".pdf");
                    string _path = Path.Combine(path_wstrain_hr, _FileName);
                    if (System.IO.File.Exists(_path))
                    {
                        System.IO.File.Delete(_path);
                    }
                }
                TempData["shortMessage"] = String.Format("Deleted successfully, {0}. ", coursname);
                return RedirectToAction("CourseList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseAddAndManage/DeleteTrainee
        [CustomAuthorize(61, 33)]//Training courses setup, //Training courses management
        public ActionResult DeleteTrainee(int id, int emp_id)//id:header_id, emp_id: employee_id
        {
            try
            {
                var coursedetail = db.Emp_TrainingDetail.Where(x => x.E_TrainHeader == id && x.Emp_Id == emp_id.ToString()).FirstOrDefault();
                if (coursedetail != null)
                {
                    db.Emp_TrainingDetail.Remove(coursedetail);
                }
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Deleted successfully, Employee Id: {0} . ", emp_id);
                return RedirectToAction("ManageCourse", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        public void ExportToCsv(List<CourseModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    courseheader = "\"" + v.CourseHeader + "\"" ,
                    datestart = v.DateOfTrainingStart,
                    dateend = v.DateOfTrainingEnd,
                    loaction = "\"" + v.Location +"\"" ,
                    trainer = "\"" + v.TrainerName + "\"" ,
                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5}",
                    "Item", "Course Name", "Date", "Loaction","Trainer"
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
                response.AddHeader("content-disposition", "attachment;filename=ManageCourse.CSV ");
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


        //For Convert string
        public ActionResult EncodingString()
        {
            try
            {
                var courselist = db.Emp_TrainingHeader.ToList();
                foreach (var c in courselist)
                {
                    //Convert Set
                    var _courseheader = c.CourseHeader;
                    var _trainername = c.Train_Name;
                    var _location = c.Train_Location;

                    var ascii = Encoding.Default.GetBytes(_courseheader);
                    var _new = Encoding.UTF8.GetString(ascii);
                    var ascii_2 = Encoding.Default.GetBytes(_trainername);
                    var _new_2 = Encoding.UTF8.GetString(ascii_2);
                    var ascii_3 = Encoding.Default.GetBytes(_location);
                    var _new_3 = Encoding.UTF8.GetString(ascii_3);

                    c.CourseHeader = _new;
                    c.Train_Name = _new_2;
                    c.Train_Location = _new_3;

                    db.Entry(c).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();

                ViewBag.Message = String.Format("Successfully encoded string {0} item.", courselist.Count);
                return View("Info");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }          
        }
    }
}
