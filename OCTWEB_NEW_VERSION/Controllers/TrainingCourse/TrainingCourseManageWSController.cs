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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Collections;

namespace OCTWEB_NET45.Controllers.TrainingCourse
{
    [Authorize]
    public class TrainingCourseManageWSController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_wstrain = ConfigurationManager.AppSettings["path_wstrain"];

        // GET: /TrainingCourseManageWS/ManageWSList
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult ManageWSList(ManageWSListModel model)
        {

            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            int pageSize = 30;
            int pageIndex = 1;
            //bool isAdmin = false;
            //model.IsAdminAccess = isAdmin;
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
                    area = x.t.Trai_NameArea,
                    Train_WS_Temp = x.h.Training_Ws_Temp_id,
                    status = (from twsd in db.Emp_TrainingWsDate
                              where twsd.E_TrainWsHeader_Id == x.h.Id
                              select twsd.Id).FirstOrDefault(),
                    downloadfile = (from twsd in db.Emp_TrainingWsDate
                                    where twsd.E_TrainWsHeader_Id == x.h.Id
                                    select twsd.Train_WsName).FirstOrDefault(),
                    department_id = (from user in db.UserDetails
                                     where user.USE_Usercode.ToString() == x.h.Train_EmpId
                                     select user.Dep_Id).FirstOrDefault(),

                }).ToList();



            // ---------------------------------------------------------------------------------- [01] UPDATE 21/8/24 ---------------------------------------------------------------------------------- // 
            var trainingDates = db.Emp_TrainingWsDate.ToList();
            model.TrainingDate = trainingDates;


            //---------------------------------------------------------------------------------- [02] UPDATE 21/8/24 ---------------------------------------------------------------------------------- //

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


            // ตรวจสอบสิทธิ์ผู้ใช้
            //Get permission
            if (Session["USE_Id"] != null)
            {
                int use_id = Convert.ToInt32(Session["USE_Id"]);
                //Training courses management (33)
                var rights_33 = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 33).FirstOrDefault();

                if (!model.showall || rights_33 == null)//From Course WS Setup 
                {
                    string user_dep_id = Session["Dep_Id"] != null ? Session["Dep_Id"].ToString() : " ";
                    train_list = train_list.Where(x => x.department_id.ToString() == user_dep_id).ToList();
                }
            }
            IPagedList<Emp_TrainingWsHeaderModel> trainPageList = train_list.ToPagedList(pageIndex, pageSize);
            List<SelectListItem> SelectStatus_list = new List<SelectListItem>();
            SelectStatus_list.Add(new SelectListItem() { Value = "0", Text = "Upload Complete" });
            SelectStatus_list.Add(new SelectListItem() { Value = "1", Text = "Waiting Upload" });

            model.Emp_TrainingWsHeaderModelPagedList = trainPageList;
            model.SelectStatusId = SelectStatus_list;
            return View(model);
        }

        //
        // GET: /TrainingCourseManageWS/ManageWSTrainCourse
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult ManageWSTrainCourse(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                var query = db.Emp_TrainingWsHeader.Where(s => s.Id == id);
                Emp_TrainingWsHeaderModel wstrainheader = query
                    .Join(db.Emp_TrainingWS_temporary, h => h.Trai_Temporary_Id, t => t.Id, (h, t) => new { h = h, t = t })
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
                    }).FirstOrDefault();

                if (wstrainheader == null)
                {
                    TempData["shortMessage"] = String.Format("This course has been deleted or this link is invalid. [Emp_TrainingWsHeader_id : {0}] ", id);
                    return RedirectToAction("ManageWSList");
                }

                var date_wstrain = db.Emp_TrainingWsDate.Where(x => x.E_TrainWsHeader_Id == wstrainheader.Id).FirstOrDefault();
                var query_detail = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsHeader_Id == wstrainheader.Id);
                var emp_id_list = query_detail.Select(s => s.Emp_Id).ToList();
                var skill_list = query_detail.Select(s => s.Trian_SkillKnow).ToList();
                IEnumerable<TraineeModel> trainee_list = db.EmpLists.Where(x => emp_id_list.Contains(x.EmpID.ToString()))
                    .Select(s => new TraineeModel()
                    {
                        emp_id = s.EmpID,
                        fname = s.FName_EN,
                        lname = s.LName_EN,
                        nation = s.Nation,
                        dept = s.DeptDesc,
                        position = s.Position,
                        assessment = (from wstd in db.Emp_TrainingWsDetail
                                      where wstd.E_TrainWsHeader_Id == wstrainheader.Id
                                      && wstd.Emp_Id == s.EmpID.ToString()
                                      select wstd.Trian_SkillKnow.Value).FirstOrDefault(),
                    }).ToList();

                var calass = 0.00;
                if (skill_list.Any())
                {
                    calass = (skill_list.Sum(s => Convert.ToInt32(s)) * 100) / (skill_list.Count() * 3);
                }

                ManageWSTrainCourseModel model = new ManageWSTrainCourseModel();
                model.WSTrainHeaderModel = wstrainheader;
                model.TraineeList = trainee_list;
                model.CalAssessment = String.Concat(calass.ToString(), "%");
                if (date_wstrain != null)
                {
                    model.DateTrainStart = date_wstrain.Training_dateSt;
                    model.DateTrainEnd = date_wstrain.Training_dateEnd;
                    model.TimeTrainStart = String.Concat(date_wstrain.Trai_Times, ":", date_wstrain.Trai_Timess);
                    model.TimeTrainEnd = String.Concat(date_wstrain.Trai_TimendT, ":", date_wstrain.Trai_TimendTs);
                    model.TrainDays = date_wstrain.Train_Date != "0" ? String.Concat(date_wstrain.Train_Date, " D ") : null;
                    model.TrainHours = String.Concat(date_wstrain.Train_Hour, " H ");
                    model.TrainMinutes = String.Concat(date_wstrain.Train_Min, " M ");
                    //Set Status
                    model.WSTrainHeaderModel.status = 1;
                    //Set downloadfile
                    model.WSTrainHeaderModel.downloadfile = date_wstrain.Train_WsName;
                }
                return View(model);
            }
            catch
            {
                ViewBag.errorMessage = String.Format("Error: Get ManageWSTrainCourse Id {0}", id);
                return View("Error");
            }
        }

        //
        // POST: /TrainingCourseManageWS/ManageWSTrainCourse
        [HttpPost]
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult ManageWSTrainCourse(FormCollection form, ManageWSTrainCourseModel model)
        {
            try
            {
                var heder_id = model.WSTrainHeaderModel.Id;

                List<string> emp_id_no_pass_list = new List<string>();
                List<string> emp_id_pass_list = new List<string>();
                foreach (var key in form.AllKeys)
                {
                    string assessment = form[key];
                    if (assessment == "NoPass")
                    {
                        emp_id_no_pass_list.Add(key);
                    }
                    else if (assessment == "Pass")
                    {
                        emp_id_pass_list.Add(key);
                    }
                }
                //NoPass
                if (emp_id_no_pass_list.Any())
                {
                    foreach (var emp_id in emp_id_no_pass_list)
                    {
                        var wstdetail = db.Emp_TrainingWsDetail
                            .Where(x => x.E_TrainWsHeader_Id == heder_id && x.Emp_Id == emp_id).FirstOrDefault();
                        wstdetail.Trian_SkillKnow = 0;
                        db.Entry(wstdetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Pass
                if (emp_id_pass_list.Any())
                {
                    foreach (var emp_id in emp_id_pass_list)
                    {
                        var wstdetail = db.Emp_TrainingWsDetail
                            .Where(x => x.E_TrainWsHeader_Id == heder_id && x.Emp_Id == emp_id).FirstOrDefault();
                        wstdetail.Trian_SkillKnow = 3;
                        db.Entry(wstdetail).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                //Save edit skill 
                db.SaveChanges();

                var query_detail = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsHeader_Id == heder_id);
                var emp_id_list = query_detail.Select(s => s.Emp_Id).ToList();
                var skill_list = query_detail.Select(s => s.Trian_SkillKnow).ToList();
                IEnumerable<TraineeModel> trainee_list = db.EmpLists.Where(x => emp_id_list.Contains(x.EmpID.ToString()))
                    .Select(s => new TraineeModel()
                    {
                        emp_id = s.EmpID,
                        fname = s.FName_EN,
                        lname = s.LName_EN,
                        nation = s.Nation,
                        dept = s.DeptDesc,
                        position = s.Position,
                        assessment = (from wstd in db.Emp_TrainingWsDetail
                                      where wstd.E_TrainWsHeader_Id == heder_id
                                      && wstd.Emp_Id == s.EmpID.ToString()
                                      select wstd.Trian_SkillKnow.Value).FirstOrDefault(),
                    }).ToList();

                var calass = 0.00;
                if (skill_list.Any())
                {
                    calass = (skill_list.Sum(s => Convert.ToInt32(s)) * 100) / (skill_list.Count() * 3);
                }

                model.TraineeList = trainee_list;
                model.CalAssessment = String.Concat(calass.ToString(), "%");

                ViewBag.Message = String.Format("Update assessment successfully, Pass : {0}, No Pass : {1} ", emp_id_pass_list.Count(), emp_id_no_pass_list.Count());
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("/TrainingCourseManageWS/ManageWSTrainCourse {0}", ex.InnerException.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseManageWS/UpdateAssessment
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult UpdateAssessment(int id)
        {
            try
            {
                ManageWSTrainCourseModel model = new ManageWSTrainCourseModel();
                var emp_id_list = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsDate_Id == id).Select(s => s.Emp_Id).ToList();
                if (!emp_id_list.Any())
                {
                    TempData["shortMessage"] = String.Format("No employee found in this course. ");
                    return RedirectToAction("ManageWSList");
                }

                IEnumerable<TraineeModel> trainee_list = db.EmpLists.Where(x => emp_id_list.Contains(x.EmpID.ToString()))
                    .Select(s => new TraineeModel()
                    {
                        emp_id = s.EmpID,
                        fname = s.FName_EN,
                        lname = s.LName_EN,
                        nation = s.Nation,
                        dept = s.DeptDesc,
                        position = s.Position,
                        assessment = s.EmpID,
                        detail_id = db.Emp_TrainingWsDetail
                                            .Where(w => w.Emp_Id == s.EmpID.ToString() && w.E_TrainWsDate_Id == id)
                                            .Select(w => w.Trian_SkillKnow.Value)
                                            .FirstOrDefault()
                    }).ToList();


                model.TraineeList = trainee_list;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Get /TrainingCourseManageWS/UpdateAssessment: {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: /TrainingCourseManageWS/UpdateAssessment
        [HttpPost]
        [CustomAuthorize(57, 33)]
        public ActionResult UpdateAssessment(FormCollection form, ManageWSTrainCourseModel model, int id)
        {
            try
            {
                List<string> emp_id_no_pass_list = new List<string>();
                List<string> emp_id_pass_list = new List<string>();

                // ดึงค่าการประเมินจาก Form
                foreach (var key in form.AllKeys)
                {
                    string assessment = form[key];
                    if (assessment == "NoPass")
                    {
                        emp_id_no_pass_list.Add(key);
                    }
                    else if (assessment == "Pass")
                    {
                        emp_id_pass_list.Add(key);
                    }
                }

                // Update สำหรับ NoPass
                if (emp_id_no_pass_list.Any())
                {
                    foreach (var emp_id in emp_id_no_pass_list)
                    {
                        var wstdetail = db.Emp_TrainingWsDetail
                            .Where(x => x.Emp_Id == emp_id && x.E_TrainWsDate_Id == id) // เพิ่มเงื่อนไข `id`
                            .FirstOrDefault();

                        if (wstdetail != null)
                        {
                            wstdetail.Trian_SkillKnow = 0;
                            db.Entry(wstdetail).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }

                // Update สำหรับ Pass
                if (emp_id_pass_list.Any())
                {
                    foreach (var emp_id in emp_id_pass_list)
                    {
                        var wstdetail = db.Emp_TrainingWsDetail
                            .Where(x => x.Emp_Id == emp_id && x.E_TrainWsDate_Id == id) // เพิ่มเงื่อนไข `id`
                            .FirstOrDefault();

                        if (wstdetail != null)
                        {
                            wstdetail.Trian_SkillKnow = 3;
                            db.Entry(wstdetail).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }

                // บันทึกการเปลี่ยนแปลง
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Update assessment successfully, Pass : {0}, No Pass : {1} ", emp_id_pass_list.Count(), emp_id_no_pass_list.Count());
                return RedirectToAction("ManageWSList");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error Post /TrainingCourseManageWS/UpdateAssessment: {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseManageWS/AddDateTimeTrainee
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult AddDateTimeTrainee(int id)
        {
            try
            {
                AddTraineeModel model = new AddTraineeModel();
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                Emp_TrainingWsHeaderModel wstrainheader = db.Emp_TrainingWsHeader.Where(s => s.Id == id)
                    .Select(x => new Emp_TrainingWsHeaderModel
                    {
                        Id = x.Id,
                        CourseHeader = x.CourseHeader,
                        Trai_Temporary_Id = x.Trai_Temporary_Id,
                        Train_DateWS = x.Train_DateWS,
                        Train_Name = x.Train_Name,
                        area = x.Training_Ws_Temp_id

                    }).FirstOrDefault();
                //Get All Employee
                List<SelectListItem> emp_list = (from u in db.EmpLists
                                                 select new SelectListItem
                                                 {
                                                     Text = String.Concat(u.EmpID.ToString()
                                                         , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                         , u.DeptDesc, " ]"),
                                                     Value = u.EmpID.ToString(),
                                                 }).ToList();

                model.EmployeeList = emp_list;
                model.TWSHeaderModel = wstrainheader;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }

        }

        // 
        // POST /TrainingCourseManageWS/AddDateTimeTrainee
        [HttpPost]
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult AddDateTimeTrainee(HttpPostedFileBase file, AddTraineeModel model)
        {
            try
            {   //Get select employee
                List<SelectListItem> emp_list = (from u in db.EmpLists
                                                 select new SelectListItem
                                                 {
                                                     Text = String.Concat(u.EmpID.ToString()
                                                          , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                          , u.DeptDesc, " ]"),
                                                     Value = u.EmpID.ToString(),
                                                 }).ToList();
                model.EmployeeList = emp_list;
                //Check Model is valid
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
                Emp_TrainingWsDate edit_wstraindate = db.Emp_TrainingWsDate.Where(x => x.E_TrainWsHeader_Id == model.TWSHeaderModel.Id).FirstOrDefault();
                edit_wstraindate = edit_wstraindate == null ? new Emp_TrainingWsDate() : edit_wstraindate;
                //Set wst header
                edit_wstraindate.E_TrainWsHeader_Id = model.TWSHeaderModel.Id;
                //Set Date, Time Start
                edit_wstraindate.Training_dateSt = model.DateOfTrainingStart.ToString("yyyy-MM-dd");
                edit_wstraindate.Trai_Times = model.StartTime.Hour.ToString();
                edit_wstraindate.Trai_Timess = model.StartTime.Minute.ToString();
                //Set Date, Time End
                edit_wstraindate.Training_dateEnd = model.DateOfTrainingEnd.ToString("yyyy-MM-dd");
                edit_wstraindate.Trai_TimendT = model.EndTime.Hour.ToString();
                edit_wstraindate.Trai_TimendTs = model.EndTime.Minute.ToString();
                //Set Diff time
                edit_wstraindate.Train_Date = diff_time.Days.ToString();
                edit_wstraindate.Train_Hour = diff_time.Hours.ToString();
                edit_wstraindate.Train_Min = diff_time.Minutes.ToString();
                //Set Break time
                edit_wstraindate.Trai_House = model.BreakTime.ToString();
                //Don't know
                edit_wstraindate.Trai_Minutices = diff_time.Minutes.ToString();


                var result = db.Emp_TrainingWsDate.Add(edit_wstraindate);
                var wsttemp = db.Emp_TrainingWS_temporary.Where(x => x.Id == model.TWSHeaderModel.Trai_Temporary_Id).FirstOrDefault();
                if (wsttemp != null)
                {
                    wsttemp.Train_Status = 2; //status complete
                    db.Entry(wsttemp).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();

                if (result != null)
                {
                    //Remove all
                    var wstdetailmodel = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsHeader_Id == model.TWSHeaderModel.Id).ToList();
                    //db.Emp_TrainingWsDetail.RemoveRange(wstdetailmodel);

                    //Save trainee
                    if (model.selectedEmp != null && model.selectedEmp.Any())
                    {
                        foreach (var i in model.selectedEmp)
                        {
                            Emp_TrainingWsDetail insertEmployee = new Emp_TrainingWsDetail()
                            {
                                E_TrainWsDate_Id = result.Id,
                                E_TrainWsHeader_Id = model.TWSHeaderModel.Id,
                                Emp_Id = i.ToString(),
                                Trian_SkillKnow = 3,
                            };
                            db.Emp_TrainingWsDetail.Add(insertEmployee);
                        }
                        TempData["shortMessage"] = String.Format("Insert employee successfully, {0} items. ", model.selectedEmp.Count());
                    }
                    //Save File
                    string id_emp_trainwsdate = result.Id.ToString();
                    string train_date_start = result.Training_dateSt;
                    string filename = String.Concat("OCT-", id_emp_trainwsdate, train_date_start, "EMPFILE");
                    if (file != null)
                    {
                        string _FileName = String.Concat(filename, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                        string _path = Path.Combine(path_wstrain, _FileName);
                        result.Train_WsName = filename;
                        db.Entry(result).State = System.Data.Entity.EntityState.Modified;
                        file.SaveAs(_path);
                    }
                }

                db.SaveChanges();

                return RedirectToAction("ManageWSList");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }
        //  
        // GET: /TrainingCourseManageWS/EditDateTimeTrainee
        [CustomAuthorize(57, 33)]//Training courses WS setup , //Training courses management
        public ActionResult EditDateTimeTrainee(int id)
        {
            AddTraineeModel model = new AddTraineeModel();
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            List<SelectListItem> emp_list = (from u in db.EmpLists
                                             select new SelectListItem
                                             {
                                                 Text = String.Concat(u.EmpID.ToString()
                                                     , " : ", u.FName_EN, " ", u.LName_EN, " ["
                                                     , u.DeptDesc, " ]"),
                                                 Value = u.EmpID.ToString(),
                                             }).ToList();
            model.EmployeeList = emp_list;
            try
            {
                //Get Date Time Course
                var wstraindate = db.Emp_TrainingWsDate.Where(s => s.Id == id).FirstOrDefault();
                WstrainDateModel wstrainDateModel = new WstrainDateModel
                {
                    Id = wstraindate.Id,
                    E_TrainWsHeader_Id = wstraindate.E_TrainWsHeader_Id,
                    Training_dateSt = wstraindate.Training_dateSt,
                    Training_dateEnd = wstraindate.Training_dateEnd,
                    Trai_Times = wstraindate.Trai_Times,
                    Trai_Timess = wstraindate.Trai_Timess,
                    Trai_TimendT = wstraindate.Trai_TimendT,
                    Trai_TimendTs = wstraindate.Trai_TimendTs,
                    Trai_House = wstraindate.Trai_House,
                    Train_WsName = wstraindate.Train_WsName
                };

                model.WstrainDateModel = wstrainDateModel;
                if (wstraindate != null)
                {
                    DateTime date_train_start = !String.IsNullOrEmpty(wstraindate.Training_dateSt) ? DateTime.Parse(wstraindate.Training_dateSt) : DateTime.Today;
                    DateTime date_train_end = !String.IsNullOrEmpty(wstraindate.Training_dateEnd) ? DateTime.Parse(wstraindate.Training_dateEnd) : DateTime.Today;
                    DateTime time_start = new DateTime(date_train_start.Year
                            , date_train_start.Month, date_train_start.Day
                            , !String.IsNullOrEmpty(wstraindate.Trai_Times) ? Convert.ToInt16(wstraindate.Trai_Times) : 0
                            , !String.IsNullOrEmpty(wstraindate.Trai_Timess) ? Convert.ToInt16(wstraindate.Trai_Timess) : 0
                            , 0, 0);
                    DateTime time_end = new DateTime(date_train_end.Year
                            , date_train_end.Month, date_train_end.Day
                            , !String.IsNullOrEmpty(wstraindate.Trai_TimendT) ? Convert.ToInt16(wstraindate.Trai_TimendT) : 0
                            , !String.IsNullOrEmpty(wstraindate.Trai_TimendTs) ? Convert.ToInt16(wstraindate.Trai_TimendTs) : 0
                            , 0, 0);
                    model.DateOfTrainingStart = date_train_start;
                    model.DateOfTrainingEnd = date_train_end;
                    model.StartTime = time_start;
                    model.EndTime = time_end;
                    model.BreakTime = Convert.ToInt16(wstraindate.Trai_House);
                    model.ImportFilePdf = wstraindate.Train_WsName;
                }
                //Get Detail Employee List
                var wstraindetail = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsDate_Id == id)
                    .Select(s => s.Emp_Id).ToList();
                if (wstraindetail.Any())
                {
                    model.selectedEmp = wstraindetail.Select(int.Parse).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Message = String.Format("Error: {0}", ex.InnerException.ToString());
                return View(model);
            }

        }

        // 
        // POST /TrainingCourseManageWS/EditDateTimeTrainee
        [HttpPost]
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult EditDateTimeTrainee(HttpPostedFileBase file, AddTraineeModel model, int id)
        {
            try
            {   //Get select employee
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
                Emp_TrainingWsDate edit_wstraindate = db.Emp_TrainingWsDate.Where(x => x.Id == id).FirstOrDefault();
                edit_wstraindate = edit_wstraindate == null ? new Emp_TrainingWsDate() : edit_wstraindate;
                //Set Date, Time Start
                edit_wstraindate.Training_dateSt = model.DateOfTrainingStart.ToString("yyyy-MM-dd");
                edit_wstraindate.Trai_Times = model.StartTime.Hour.ToString();
                edit_wstraindate.Trai_Timess = model.StartTime.Minute.ToString();
                //Set Date, Time End
                edit_wstraindate.Training_dateEnd = model.DateOfTrainingEnd.ToString("yyyy-MM-dd");
                edit_wstraindate.Trai_TimendT = model.EndTime.Hour.ToString();
                edit_wstraindate.Trai_TimendTs = model.EndTime.Minute.ToString();
                //Set Diff time
                edit_wstraindate.Train_Date = diff_time.Days.ToString();
                edit_wstraindate.Train_Hour = diff_time.Hours.ToString();
                edit_wstraindate.Train_Min = diff_time.Minutes.ToString();
                //Set Break time
                edit_wstraindate.Trai_House = model.BreakTime.ToString();
                //Don't know
                edit_wstraindate.Trai_Minutices = diff_time.Minutes.ToString();

                if (edit_wstraindate.Id > 0)
                {
                    //Save File
                    string filename = String.Concat("OCT-", edit_wstraindate.Id.ToString(), edit_wstraindate.Training_dateSt, "EMPFILE");
                    if (file != null)
                    {
                        string _FileName = String.Concat(filename, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                        string _path = Path.Combine(path_wstrain, _FileName);
                        edit_wstraindate.Train_WsName = filename;
                        file.SaveAs(_path);
                    }
                    //Save edit Date time record
                    db.Entry(edit_wstraindate).State = System.Data.Entity.EntityState.Modified;
                    TempData["shortMessage"] = String.Format("Edited date record successfully ");

                    var emp_id_list = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsDate_Id == id)
                        .Select(s => s.Emp_Id).ToList();
                    var oldEmpId = emp_id_list.Any() ? emp_id_list.Select(int.Parse).ToList() : new List<int>();
                    var newEmpId = model.selectedEmp != null && model.selectedEmp.Any() ? model.selectedEmp : new List<int>();

                    var oldEmpId_Not_selectedEmp = oldEmpId.Any() ? oldEmpId.Except(newEmpId).ToList() : new List<int>();

                    var selectedEmp_Not_oldEmpId = newEmpId.Any() ? newEmpId.Except(oldEmpId).ToList() : new List<int>();


                    foreach (var i in selectedEmp_Not_oldEmpId)
                    {
                        Emp_TrainingWsDetail insertEmployee = new Emp_TrainingWsDetail()
                        {
                            E_TrainWsDate_Id = edit_wstraindate.Id,
                            Emp_Id = i.ToString(),
                            Trian_SkillKnow = 3,
                        };
                        db.Emp_TrainingWsDetail.Add(insertEmployee);
                        TempData["shortMessage"] = String.Format("Insert employee successfully, {0} items. ", selectedEmp_Not_oldEmpId.Count());
                    }
                }

                db.SaveChanges();

                return RedirectToAction("ManageWSList");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        //
        // GET: /TrainingCourseManageWS/DeleteCourse
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult DeleteCourse(int id)
        {
            try
            {
                string filename = null;
                string coursname = " ";
                var wstheader = db.Emp_TrainingWsHeader.Where(x => x.Id == id).FirstOrDefault();
                var wstdate = db.Emp_TrainingWsDate.Where(x => x.E_TrainWsHeader_Id == id).FirstOrDefault();
                var wstdetail_list = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsHeader_Id == id).ToList();

                if (wstheader != null)
                {
                    coursname = String.Concat(wstheader.CourseHeader, " [", wstheader.Training_Ws_Temp_id, "]");
                    var wsttemp = db.Emp_TrainingWS_temporary.Where(x => x.Id == wstheader.Trai_Temporary_Id).FirstOrDefault();
                    if (wsttemp != null)
                    {
                        db.Emp_TrainingWS_temporary.Remove(wsttemp);
                    }
                    db.Emp_TrainingWsHeader.Remove(wstheader);
                }
                if (wstdate != null)
                {
                    filename = wstdate.Train_WsName;
                    db.Emp_TrainingWsDate.Remove(wstdate);
                }
                if (wstdetail_list.Any())
                {
                    db.Emp_TrainingWsDetail.RemoveRange(wstdetail_list);
                }


                db.SaveChanges();
                //Delete file
                if (!String.IsNullOrEmpty(filename))
                {
                    string _FileName = String.Concat(filename, ".pdf");
                    string _path = Path.Combine(path_wstrain, _FileName);
                    if (System.IO.File.Exists(_path))
                    {
                        System.IO.File.Delete(_path);
                    }
                }
                TempData["shortMessage"] = String.Format("Deleted successfully, {0}. ", coursname);
                return RedirectToAction("ManageWSList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        //Update---> 9 Dec 24
        // GET: /TrainingCourseManageWS/DeleteDateTrainee
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult DeleteDateTrainee(int id)
        {
            try
            {
                string filename = null;
                string coursname = " ";
                //var wstheader = db.Emp_TrainingWsHeader.Where(x => x.Id == id).FirstOrDefault();
                var wstdate = db.Emp_TrainingWsDate.Where(x => x.Id == id).FirstOrDefault();
                var wstdetail_list = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsDate_Id == id).ToList();


                if (wstdate != null)
                {
                    filename = wstdate.Train_WsName;
                    db.Emp_TrainingWsDate.Remove(wstdate);
                }
                if (wstdetail_list.Any())
                {
                    db.Emp_TrainingWsDetail.RemoveRange(wstdetail_list);
                }


                db.SaveChanges();
                //Delete file
                if (!String.IsNullOrEmpty(filename))
                {
                    string _FileName = String.Concat(filename, ".pdf");
                    string _path = Path.Combine(path_wstrain, _FileName);
                    if (System.IO.File.Exists(_path))
                    {
                        System.IO.File.Delete(_path);
                    }
                }
                TempData["shortMessage"] = String.Format("Deleted successfully, {0}. ", coursname);
                return RedirectToAction("ManageWSList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        //Update---> 5 Dec 24
        // GET: /TrainingCourseManageWS/DeleteTrainee
        [CustomAuthorize(57, 33)]//Training courses WS setup, //Training courses management
        public ActionResult DeleteTrainee(int id, int emp_id)
        {
            try
            {
                var wstdetail = db.Emp_TrainingWsDetail.Where(x => x.E_TrainWsDate_Id == id && x.Emp_Id == emp_id.ToString()).FirstOrDefault();
                // Delete Data Emp_TrainingWsDate
                var wstdateconnt = db.Emp_TrainingWsDetail.Count(x => x.E_TrainWsDate_Id == id);
                var wstdate =  db.Emp_TrainingWsDate.Where(x => x.Id == id).FirstOrDefault();
                if (wstdateconnt == 1)
                {
                    db.Emp_TrainingWsDate.Remove(wstdate);  
                }
                if (wstdetail != null)
                {
                    db.Emp_TrainingWsDetail.Remove(wstdetail);
                }
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Deleted successfully, Employee Id: {0} . ", wstdetail.Emp_Id);
                //return RedirectToAction("ManageWSTrainCourse", new { id = wstdetail.E_TrainWsHeader_Id });
                return RedirectToAction("ManageWSList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        public void ExportToCsv(List<Emp_TrainingWsHeaderModel> model)
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
                response.AddHeader("content-disposition", "attachment;filename=ManageWSTrainingCourse.CSV ");
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

        //
        // POST: /TrainingCourseManageWS/GetWSTrainName/
        [HttpPost]
        public JsonResult GetWSTrainName(string Prefix)
        {
            var wstrainname = (from ws in db.Emp_TrainingWsHeader
                               where ws.Train_Name.StartsWith(Prefix)
                               select new { label = ws.Train_Name, val = ws.Train_Name }).Distinct().Take(10).ToList();
            return Json(wstrainname);
        }

        //
        // POST: /TrainingCourseManageWS/GetWSTrainArea/
        [HttpPost]
        public JsonResult GetWSTrainArea(string Prefix)
        {
            var wstrainarea = (from ws in db.Emp_TrainingWsHeader
                               where ws.Training_Ws_Temp_id.StartsWith(Prefix)
                               select new { label = ws.Training_Ws_Temp_id, val = ws.Training_Ws_Temp_id }).Distinct().Take(10).ToList();
            return Json(wstrainarea);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}

