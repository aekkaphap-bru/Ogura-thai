using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.TrainingCourse
{
    [Authorize]
    public class TrainingCourseAddWSController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        //
        // GET: /TrainingCourseAddWS/WSTrainingList
        [CustomAuthorize(33)]//Training courses management
        public ActionResult WSTrainingList(WSTrainingListModel model)
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            int pageSize = 30;
            int pageIndex = 1;
            pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

            int? selectedStatusId = model.selected_status.HasValue ? model.selected_status : null;

            IEnumerable<Emp_TrainingWS_temporary> query = db.Emp_TrainingWS_temporary;

            if (selectedStatusId.HasValue)
            {
                query = query.Where(x => x.Train_Status == selectedStatusId);
            }

            var train_list = query.OrderByDescending(o => o.Id).Select(x => new WSTrainingModel
            {
                Id = x.Id,
                Train_Status = x.Train_Status,
                Train_NumberWS = x.Train_NumberWS,
                Train_HeaderThai = x.Train_HeaderThai,
                Train_Header = x.Train_Header,
                Train_DateWS = x.Train_DateWS,
                Train_DateChkSendMail = x.Train_DateChkSendMail,
                Trai_Rev = x.Trai_Rev,
                Trai_NameArea = x.Trai_NameArea,

            }).ToList();

            IPagedList<WSTrainingModel> trainPageList = train_list.ToPagedList(pageIndex, pageSize);

            List<SelectListItem> SelectStatus_list = new List<SelectListItem>();
            SelectStatus_list.Add(new SelectListItem() { Value = "0", Text = "Pending" });
            SelectStatus_list.Add(new SelectListItem() { Value = "1", Text = "In progress" });
            SelectStatus_list.Add(new SelectListItem() { Value = "2", Text = "Complete" });

            model.WSTrainingModelPagedList = trainPageList;
            model.SelectStatusId = SelectStatus_list;

            return View(model);
        }

        //
        // POST: /TrainingCourseAddWS/WSTrainingList
        [HttpPost]
        [CustomAuthorize(33)]//Training courses management
        public ActionResult WSTrainingList(FormCollection form, WSTrainingListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? selectedStatusId = model.selected_status.HasValue ? model.selected_status : null;

                IEnumerable<Emp_TrainingWS_temporary> query = db.Emp_TrainingWS_temporary;

                if (selectedStatusId.HasValue)
                {
                    query = query.Where(x => x.Train_Status == selectedStatusId);
                }

                var train_list = query.OrderByDescending(o => o.Id).Select(x => new WSTrainingModel
                {
                    Id = x.Id,
                    Train_Status = x.Train_Status,
                    Train_NumberWS = x.Train_NumberWS,
                    Train_HeaderThai = x.Train_HeaderThai,
                    Train_Header = x.Train_Header,
                    Train_DateWS = x.Train_DateWS,
                    Train_DateChkSendMail = x.Train_DateChkSendMail,
                    Trai_Rev = x.Trai_Rev,
                    Trai_NameArea = x.Trai_NameArea,

                }).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(train_list);
                }

                IPagedList<WSTrainingModel> trainPageList = train_list.ToPagedList(pageIndex, pageSize);

                List<SelectListItem> SelectStatus_list = new List<SelectListItem>();
                SelectStatus_list.Add(new SelectListItem() { Value = "0", Text = "Pending" });
                SelectStatus_list.Add(new SelectListItem() { Value = "1", Text = "In progress" });
                SelectStatus_list.Add(new SelectListItem() { Value = "2", Text = "Complete" });

                model.WSTrainingModelPagedList = trainPageList;
                model.SelectStatusId = SelectStatus_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        //
        // GET: /TrainingCourseAddWS/WSTrainingSendMail
        [CustomAuthorize(33)]//Training courses management
        public ActionResult WSTrainingSendMail(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                WSTrainingModel wstrain = db.Emp_TrainingWS_temporary.Where(x => x.Id == id).Select(s => new WSTrainingModel
                {
                    Id = s.Id,
                    Trai_NameArea = s.Trai_NameArea,
                    Trai_Rev = s.Trai_Rev,
                    Train_DateChkSendMail = s.Train_DateChkSendMail,
                    Train_DateWS = s.Train_DateWS,
                    Train_Header = s.Train_Header,
                    Train_HeaderThai = s.Train_HeaderThai,
                    Train_NumberWS = s.Train_NumberWS,
                    Train_Status = s.Train_Status
                }).FirstOrDefault();

                IEnumerable<UserDetailModel> user_list = db.UserDetails.Select(s => new UserDetailModel
                {
                    USE_Id = s.USE_Id,
                    USE_FName = s.USE_FName,
                    USE_LName = s.USE_LName,
                    USE_Email = s.USE_Email,
                }).ToList();

                WSTrainingSendMailModel model = new WSTrainingSendMailModel();

                List<SelectListItem> dept_list = db.Departments.OrderBy(o => o.Dep_Name).Select(s => new SelectListItem()
                {
                    Value = s.Dep_Id.ToString(),
                    Text = s.Dep_Name
                }).ToList();

                model.WsTrain = wstrain;
                model.UserList = user_list;
                model.SelectDepartmentId = dept_list;
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
        // POST: /TrainingCourseAddWS/WSTrainingSendMail
        [HttpPost]
        [CustomAuthorize(33)]//Training courses management
        //public ActionResult WSTrainingSendMail(FormCollection form, WSTrainingSendMailModel model, int id)
        //{
        //    try
        //    {
        //        WSTrainingModel wstrain = db.Emp_TrainingWS_temporary.Where(x => x.Id == id).Select(s => new WSTrainingModel
        //        {
        //            Id = s.Id,
        //            Trai_NameArea = s.Trai_NameArea,
        //            Trai_Rev = s.Trai_Rev,
        //            Train_DateChkSendMail = s.Train_DateChkSendMail,
        //            Train_DateWS = s.Train_DateWS,
        //            Train_Header = s.Train_Header,
        //            Train_HeaderThai = s.Train_HeaderThai,
        //            Train_NumberWS = s.Train_NumberWS,
        //            Train_Status = s.Train_Status
        //        }).FirstOrDefault();

        //        var selected_user = form["selectedItem"];
        //        var submit_var = form["submit_var"];

        //        if (String.IsNullOrEmpty(submit_var))
        //        {
        //            if (!String.IsNullOrEmpty(selected_user))
        //            {
        //                int user_id = Convert.ToInt32(selected_user);
        //                bool result = SendEmailWSTrain(model.WsTrain, user_id);

        //                if (result)
        //                {
        //                    if (wstrain.Train_Status == 1)
        //                    {
        //                        return Json(new { success = true, message = "Email resent successfully.", redirectUrl = Url.Action("WSTrainingList") });
        //                    }
        //                    else
        //                    {
        //                        return Json(new { success = true, message = "Email sent successfully.", redirectUrl = Url.Action("WSTrainingList") });
        //                    }

        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "Failed to send email. Please try again." });
        //                }
        //            }

        //            return Json(new { success = false, message = "Please select a user before sending the email." });
        //        }

        //        //if (String.IsNullOrEmpty(submit_var))
        //        //{
        //        //    if (!String.IsNullOrEmpty(selected_user))
        //        //    {
        //        //        int user_id = Convert.ToInt32(selected_user);
        //        //        SendEmailWSTrain(model.WsTrain, user_id);
        //        //        TempData["shortMessage"] = String.Format("Send email successfully, To  ");

        //        //        return RedirectToAction("WSTrainingList");
        //        //    }

        //        //    TempData["shortMessage"] = String.Format("User is not selected for sending email.");
        //        //    return RedirectToAction("WSTrainingSendMail");
        //        //}

        //        int dept_id = model.searchDepartmentId;

        //        IEnumerable<UserDetail> query = db.UserDetails;
        //        query = dept_id > 0 ? query.Where(x => x.Dep_Id == dept_id) : query;

        //        IEnumerable<UserDetailModel> user_list = query.Select(s => new UserDetailModel
        //        {
        //            USE_Id = s.USE_Id,
        //            USE_FName = s.USE_FName,
        //            USE_LName = s.USE_LName,
        //            USE_Email = s.USE_Email,
        //        }).ToList();

        //        List<SelectListItem> dept_list = db.Departments.OrderBy(o => o.Dep_Name).Select(s => new SelectListItem()
        //        {
        //            Value = s.Dep_Id.ToString(),
        //            Text = s.Dep_Name
        //        }).ToList();

        //        model.UserList = user_list;
        //        model.SelectDepartmentId = dept_list;

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        ViewBag.errorMessage = ex.ToString();
        //        return View("Error");
        //    }
        //}

        // /TrainingCourseAddWS/SendEmailWSTrain
        //public void SendEmailWSTrain(WSTrainingModel wstrain, int userid)
        //{
        //    try
        //    {
        //        SendMailCenterModel model = new SendMailCenterModel();
        //        var user_to = db.UserDetails.Where(w => w.USE_Id == userid).Select(s => new
        //        {
        //            fname = s.USE_FName,
        //            lname = s.USE_LName,
        //            email = s.USE_Email,
        //            usercode = s.USE_Usercode,
        //        }).FirstOrDefault();

        //        //Edit status in Emp_trainingWS_temporary
        //        var ws_trian_temp = db.Emp_TrainingWS_temporary.Where(x => x.Id == wstrain.Id).FirstOrDefault();
        //        if (ws_trian_temp != null)
        //        {
        //            ws_trian_temp.Train_Status = 1;
        //            ws_trian_temp.Train_DateChkSendMail = DateTime.Now.ToString("yyyy-MM-dd");
        //            ws_trian_temp.SendMail = user_to.email;
        //        }
        //        db.Entry(ws_trian_temp).State = System.Data.Entity.EntityState.Modified;
        //        //Add new ws train header card
        //        Emp_TrainingWsHeader wstrainheader = new Emp_TrainingWsHeader() 
        //        {
        //            CourseHeader = string.Concat(wstrain.Train_Header, " ", wstrain.Train_HeaderThai),
        //            Trai_Temporary_Id = wstrain.Id,
        //            Train_Location = "Ogura Clutch (Thailand) Co.,Ltd.",
        //            Train_Price = 0,
        //            Train_Name = wstrain.Train_NumberWS,
        //            Train_Min = wstrain.Trai_Rev,
        //            Train_DateWS = wstrain.Train_DateWS,
        //            Train_EmpId = user_to.usercode.ToString(),
        //            Training_Ws_Temp_id = wstrain.Trai_NameArea,
        //        };
        //        var result = db.Emp_TrainingWsHeader.Add(wstrainheader);
        //        db.SaveChanges();

        //        //Send e-mail
        //        var callbackUrl = Url.Action("ManageWSTrainCourse", "TrainingCourseManageWS", new {id= result.Id }, protocol: Request.Url.Scheme);

        //        string html = "<strong style='font-size: 20px'> Dear K' " + user_to.fname + " " + user_to.lname + ", </strong>"
        //                     + "<p style='font-size: 18px'>Please update list name of employee into </p>"
        //                     + "<table style='font-size:18px'>"
        //                     + "<tr><td>WS Number : </td><td>" + wstrain.Train_NumberWS + "</td></tr>"
        //                     + "<tr><td>WS Name : </td><td>" + wstrain.Train_Header + " " + wstrain.Train_HeaderThai + "</td></tr>"
        //                     + "<tr><td>Line Name : </td><td>" + wstrain.Trai_NameArea + "</td></tr>"
        //                     + "<tr><td>Registration WS Date : </td><td>" + wstrain.Train_DateWS + "</td></tr></table>"
        //                     + "<p style='font-size:18px'><strong>*Please fill out the documents within 7 days. On <a href=\"" + callbackUrl + "\">OCT WEB SYSTEMS </a> </strong>"
        //                     + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/><br/><br/><br/>"
        //                     + "<p style='font-size: 18px'>Best Regards,<br/>"
        //                     + "OCT WEB</p>";

        //        List<string> emailList = new List<string>();
        //        List<string> emailList_cc = new List<string>();
        //        emailList.Add(user_to.email);
        //        model.To = emailList;
        //        model.Tocc = emailList_cc;
        //        model.Subject = "Update the data working standard";
        //        model.Body = html;
        //        SendMailCenterController.SendMail(model);
        //        //var status = model.WsTrain.Train_Status; // สมมติว่ามี field นี้

        //        //if (status == 0)
        //        //{

        //        //    TempData["Message"] = "ส่งเมลสำเร็จ!";
        //        //}
        //        //else
        //        //{
        //        //    TempData["Message"] = "ส่งเมลอีกครั้งสำเร็จแล้ว!";
        //        //}
        //        TempData["shortMessage"] = String.Format("Send email successfully, To {0} ", user_to.email);

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["shortMessage"] = String.Format("Error {0}",ex.ToString());
        //    }
        //}

        public ActionResult WSTrainingSendMail(FormCollection form, WSTrainingSendMailModel model)
        {
            try
            {
                var selected_user = form["selectedItem"];
                var submit_var = form["submit_var"];

                if (String.IsNullOrEmpty(submit_var))
                {
                    if (!String.IsNullOrEmpty(selected_user))
                    {
                        int user_id = Convert.ToInt32(selected_user);
                        var result = SendEmailWSTrain(model.WsTrain, user_id);

                        string message;
                        string icon;

                        switch (result)
                        {
                            case "resent":
                                message = "The email has been resent successfully.";
                                icon = "info";
                                break;
                            case "sent":
                                message = "The email was sent successfully.";
                                icon = "success";
                                break;
                            default:
                                message = "Failed to send the email. Please try again later.";
                                icon = "error";
                                break;
                        }

                        return Json(new
                        {
                            success = true,
                            message = message,
                            icon = icon,
                            redirectUrl = Url.Action("WSTrainingList")
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Please select a recipient before sending the email."
                        });
                    }
                }

                // กรณีเลือก department แต่ยังไม่ได้ส่งเมล
                int dept_id = model.searchDepartmentId;
                IEnumerable<UserDetail> query = db.UserDetails;
                query = dept_id > 0 ? query.Where(x => x.Dep_Id == dept_id) : query;

                IEnumerable<UserDetailModel> user_list = query.Select(s => new UserDetailModel
                {
                    USE_Id = s.USE_Id,
                    USE_FName = s.USE_FName,
                    USE_LName = s.USE_LName,
                    USE_Email = s.USE_Email,
                }).ToList();

                List<SelectListItem> dept_list = db.Departments.OrderBy(o => o.Dep_Name).Select(s => new SelectListItem()
                {
                    Value = s.Dep_Id.ToString(),
                    Text = s.Dep_Name
                }).ToList();

                model.UserList = user_list;
                model.SelectDepartmentId = dept_list;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = "An unexpected error has occurred. Please contact the system administrator.";
                return RedirectToAction("WSTrainingSendMail");
            }
        }

        public string SendEmailWSTrain(WSTrainingModel wstrain, int userid)
        {
            try
            {
                SendMailCenterModel model = new SendMailCenterModel();
                var user_to = db.UserDetails.Where(w => w.USE_Id == userid).Select(s => new
                {
                    fname = s.USE_FName,
                    lname = s.USE_LName,
                    email = s.USE_Email,
                    usercode = s.USE_Usercode,
                }).FirstOrDefault();

                var ws_trian_temp = db.Emp_TrainingWS_temporary.FirstOrDefault(x => x.Id == wstrain.Id);

                string resultStatus = "sent";

                if (ws_trian_temp != null)
                {
                    if (ws_trian_temp.Train_Status == 1)
                    {
                        resultStatus = "resent"; // ส่งซ้ำ
                    }
                    ws_trian_temp.Train_Status = 1;
                    ws_trian_temp.Train_DateChkSendMail = DateTime.Now.ToString("yyyy-MM-dd");
                    ws_trian_temp.SendMail = user_to.email;
                    db.Entry(ws_trian_temp).State = System.Data.Entity.EntityState.Modified;
                }

                Emp_TrainingWsHeader wstrainheader = new Emp_TrainingWsHeader()
                {
                    CourseHeader = $"{wstrain.Train_Header} {wstrain.Train_HeaderThai}",
                    Trai_Temporary_Id = wstrain.Id,
                    Train_Location = "Ogura Clutch (Thailand) Co.,Ltd.",
                    Train_Price = 0,
                    Train_Name = wstrain.Train_NumberWS,
                    Train_Min = wstrain.Trai_Rev,
                    Train_DateWS = wstrain.Train_DateWS,
                    Train_EmpId = user_to.usercode.ToString(),
                    Training_Ws_Temp_id = wstrain.Trai_NameArea,
                };

                var result = db.Emp_TrainingWsHeader.Add(wstrainheader);
                db.SaveChanges();

                var callbackUrl = Url.Action("ManageWSTrainCourse", "TrainingCourseManageWS", new { id = result.Id }, protocol: Request.Url.Scheme);

                string html = $"<strong style='font-size: 20px'>Dear K' {user_to.fname} {user_to.lname},</strong>"
                            + "<p style='font-size: 18px'>Please update the employee list in</p>"
                            + $"<table style='font-size:18px'>"
                            + $"<tr><td>WS Number : </td><td>{wstrain.Train_NumberWS}</td></tr>"
                            + $"<tr><td>WS Name : </td><td>{wstrain.Train_Header} {wstrain.Train_HeaderThai}</td></tr>"
                            + $"<tr><td>Line Name : </td><td>{wstrain.Trai_NameArea}</td></tr>"
                            + $"<tr><td>Registration WS Date : </td><td>{wstrain.Train_DateWS}</td></tr></table>"
                            + $"<p style='font-size:18px'><strong>*Please fill out the documents within 7 days. On <a href='{callbackUrl}'>OCT WEB SYSTEMS</a></strong></p>"
                            + $"<p style='font-size: 18px'>Best Regards,<br/>OCT WEB</p>";

                List<string> emailList = new List<string> { user_to.email };
                model.To = emailList;
                model.Subject = "Update the data working standard";
                model.Body = html;
                model.Tocc = new List<string>();

                SendMailCenterController.SendMail(model);

                return resultStatus;
            }
            catch (Exception)
            {
                return "error";
            }
        }



        public void ExportToCsv(List<WSTrainingModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    ws_no = v.Train_NumberWS,
                    ws_date = v.Train_DateWS,
                    train_area = "\"" + v.Trai_NameArea + "\"" ,
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
                response.AddHeader("content-disposition", "attachment;filename=WSTrainingCourse.CSV ");
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
