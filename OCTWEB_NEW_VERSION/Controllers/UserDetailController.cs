using OCTWEB_NET45.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OCTWEB_NET45.Models;
using PagedList;
using System.Data.Entity;
using OCTWEB_NET45.Infrastructure;


namespace OCTWEB_NET45.Controllers
{
    [Authorize]
    public class UserDetailController : Controller
    {

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private LogsReportController savelogs = new LogsReportController();

        //
        // GET: /UserDetail/userDetailList
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult userDetailList(UserDetailPagedListModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();

                }
                int? usercode = null;
                string fname = null;
                string nickname = null;
                int? dep = null;
                string national = null;

                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                if (model.Usercode.HasValue)
                {
                    usercode = model.Usercode;
                }
                if (!string.IsNullOrEmpty(model.Fname))
                {
                    fname = model.Fname;
                }
                if (!string.IsNullOrEmpty(model.Nickname))
                {
                    nickname = model.Nickname;
                }
                if (model.Dep.HasValue)
                {
                    dep = model.Dep;
                }
                if (!string.IsNullOrEmpty(model.National))
                {
                    national = model.National;
                }

                IEnumerable<UserDetail> query = db.UserDetails;
                if (usercode != null)
                {
                    query = query.Where(x => x.USE_Usercode == usercode);
                }
                if (fname != null)
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.USE_FName) && x.USE_FName.ToLowerInvariant().Contains(fname.ToLowerInvariant())));
                }
                if (nickname != null)
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.USE_NickName) && x.USE_NickName.ToLowerInvariant().Contains(nickname.ToLowerInvariant())));
                }
                if (dep != null)
                {
                    query = query.Where(x => x.Dep_Id == dep);
                }
                if (national != null)
                {
                    query = query.Where(x => x.USE_Nationality == national);
                }
                var userDetailList = query.Select(s => new UserDetailModel
                {
                    Dep_Id = s.Dep_Id,
                    USE_Account = s.USE_Account,
                    USE_Email = s.USE_Email,
                    USE_FName = s.USE_FName,
                    USE_Id = s.USE_Id,
                    USE_LName = s.USE_LName,
                    USE_Nationality = s.USE_Nationality,
                    USE_NickName = s.USE_NickName,
                    USE_Status = s.USE_Status,
                    USE_TelNo = s.USE_TelNo,
                    USE_Usercode = s.USE_Usercode,
                    Department_name = s.Department.Dep_Name
                }).ToList();

                List<Department> departmentList = db.Departments.OrderBy(o => o.Dep_Name).ToList();
                model.Departments = departmentList.Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();
                model.Nationals = new[]
                { 
                    new SelectListItem { Value = "Thai", Text = "Thai" }, 
                    new SelectListItem { Value = "Japanese", Text = "Japanese" } ,
                }.ToList();

                ViewData["searchFname"] = fname;
                ViewData["searchNickname"] = nickname;
                ViewData["searchUsercode"] = usercode;
                ViewData["searchNational"] = national;
                //DepartmentsDropDownList(dep);

                IPagedList<UserDetailModel> userDetailPagedList = userDetailList.ToPagedList(pageIndex, pageSize);
                model.UserDetailModelPagedList = userDetailPagedList;
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }


        // POST: /UserDetail/userDetailList
        [HttpPost]
 //       [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult userDetailList(FormCollection form, UserDetailPagedListModel model)
        {
            try
            {
                int? usercode = null;
                string fname = null;
                string nickname = null;
                int? dep = null;
                string national = null;

                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                if (model.Usercode.HasValue)
                {
                    usercode = model.Usercode;
                }
                if (!string.IsNullOrEmpty(model.Fname))
                {
                    fname = model.Fname;
                }
                if (!string.IsNullOrEmpty(model.Nickname))
                {
                    nickname = model.Nickname;
                }
                if (model.Dep.HasValue)
                {
                    dep = model.Dep;
                }
                if (!string.IsNullOrEmpty(model.National))
                {
                    national = model.National;
                }

                IEnumerable<UserDetail> query = db.UserDetails;
                if (usercode != null)
                {
                    query = query.Where(x => x.USE_Usercode == usercode);
                }
                if (fname != null)
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.USE_FName) && x.USE_FName.ToLowerInvariant().Contains(fname.ToLowerInvariant())));//String.Equals(x.USE_FName, fname, StringComparison.InvariantCultureIgnoreCase)
                }
                if (nickname != null)
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.USE_NickName) && x.USE_NickName.ToLowerInvariant().Contains(nickname.ToLowerInvariant())));//String.Equals(x.USE_NickName, nickname, StringComparison.CurrentCultureIgnoreCase)
                }
                if (dep != null)
                {
                    query = query.Where(x => x.Dep_Id == dep);
                }
                if (national != null)
                {
                    query = query.Where(x => x.USE_Nationality == national);
                }
                var userDetailList = query.Select(s => new UserDetailModel
                {
                    Dep_Id = s.Dep_Id,
                    USE_Account = s.USE_Account,
                    USE_Email = s.USE_Email,
                    USE_FName = s.USE_FName,
                    USE_Id = s.USE_Id,
                    USE_LName = s.USE_LName,
                    USE_Nationality = s.USE_Nationality,
                    USE_NickName = s.USE_NickName,
                    USE_Status = s.USE_Status,
                    USE_TelNo = s.USE_TelNo,
                    USE_Usercode = s.USE_Usercode,
                    Department_name = s.Department.Dep_Name
                }).ToList();

                List<Department> departmentList = db.Departments.OrderBy(o => o.Dep_Name).ToList();
                model.Departments = departmentList.Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();
                model.Nationals = new[]
                { 
                    new SelectListItem { Value = "Thai", Text = "Thai" }, 
                    new SelectListItem { Value = "Japanese", Text = "Japanese" } ,
                }.ToList();

                ViewData["searchFname"] = fname;
                ViewData["searchNickname"] = nickname;
                ViewData["searchUsercode"] = usercode;
                ViewData["searchNational"] = national;
               // DepartmentsDropDownList(dep);

                model.Fname = fname;
                model.Nickname = nickname;
                model.Usercode = usercode;
                model.National = national;
                model.Dep = dep;

                IPagedList<UserDetailModel> userDetailPagedList = userDetailList.ToPagedList(pageIndex, pageSize);
                model.UserDetailModelPagedList = userDetailPagedList;
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }


        // POST: /UserDetail/ -->DeleteUserDetail-->DeleteUser
        [HttpPost]
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult DeleteUserUserDetail(FormCollection form)
        {
            try
            {

                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];

                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    //find USE_Id(primary_key(USE_Id) of UserDetails Table) in ud table
                    List<UserDetail> userdetail_list = db.UserDetails.Where(x => id_list.Contains(x.USE_Id)).ToList();
                    //get USE_Usercode in ud table
                    var user_code_list = userdetail_list.Select(x => x.USE_Usercode).ToList();

                    List<AspNetUser> user_list = db.AspNetUsers.Where(x => user_code_list.Contains(x.USE_Usercode)).ToList();
                   
                    //delete all Id in AspUser table
                    db.AspNetUsers.RemoveRange(user_list);
                    ///Delete in UserDetail Table
                    db.UserDetails.RemoveRange(userdetail_list);

                    db.SaveChanges();

                    //Save Logs
                    if (userdetail_list != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        foreach (var i in userdetail_list)
                        {
                            LogsReportModel logmodel = new LogsReportModel()
                            {
                                Log_Action_Id = i.USE_Id,
                                Log_System = "Administration",
                                Log_Type = "User Detail",
                                Log_Detail = string.Concat(i.USE_Account, "/"
                                            , i.USE_FName, "/", i.USE_LName, "/"
                                            , i.USE_NickName, "/", i.USE_Usercode),
                                Log_Action = "delete",
                                Log_by = user_nickname
                            };
                            savelogs.SaveLogs(logmodel);
                        }
                    }
                }
                TempData["shortMessage"] = "Deleted successfully.";
                return RedirectToAction("userDetailList");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }

        }

        // GET: /UserDetail/getUserDetail -->editUserDetail
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult getUserDetail(int id)
        {

            UserDetail ud = db.UserDetails.Where(x => x.USE_Id == id).FirstOrDefault();
            AspNetUser aspuser = db.AspNetUsers.Where(x => x.USE_Usercode == ud.USE_Usercode).FirstOrDefault();

            if (aspuser != null)
            {

                return RedirectToAction("editUserDetail", new { id = aspuser.Id });
            }
            else
            {
                return RedirectToAction("createUser", "Account", new { id = id });
            }

        }


        //
        // GET: /UserDetail/createUserDetail
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult createUserDetail(string id)
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }

            if (id == null)
            {
                ViewBag.errorMessage = "Id from regiter is null.";
                return View("Error");
            }
            AspNetUser userModel = db.AspNetUsers.Find(id); //AspNet User
            if (userModel == null)
            {
                ViewBag.errorMessage = "User account from AspNetUser table is null, Should register before create user detail.";
                return View("Error");
            }
            UserDetailModel userdetailModel = db.UserDetails
                .Where(u => u.USE_Usercode == userModel.USE_Usercode)
                .Select(s => new UserDetailModel
                {
                    Dep_Id = s.Dep_Id,
                    USE_Account = s.USE_Account,
                    USE_Email = s.USE_Email,
                    USE_FName = s.USE_FName,
                    USE_Id = s.USE_Id,
                    USE_LName = s.USE_LName,
                    USE_Nationality = s.USE_Nationality,
                    USE_NickName = s.USE_NickName,
                    USE_Status = s.USE_Status,
                    USE_TelNo = s.USE_TelNo,
                    USE_Usercode = s.USE_Usercode,
                    Department_name = s.Department.Dep_Name
                }).FirstOrDefault();

            List<Department> departmentList = db.Departments.OrderBy(o => o.Dep_Name).ToList();
            if (userdetailModel == null)
            {
                //Go to Create UserDetail method.
                UserDetailModel model = new UserDetailModel();
                model.USE_Account = userModel.UserName;
                model.USE_Email = userModel.Email;
                model.USE_Usercode = userModel.USE_Usercode;
                model.BoolStatus = true;

                GetSelectList(model);
                return View(model);
            }

            userdetailModel.Departments = departmentList.Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name });
            userdetailModel.National = new[]
                { 
                    new SelectListItem { Value = "Thai", Text = "Thai" }, 
                    new SelectListItem { Value = "Japanese", Text = "Japanese" } ,
                }.ToList();
            //DepartmentsDropDownList(userdetailModel.Dep_Id);

            return View(userdetailModel);
        }

        //
        // POST: /UserDetail/createUserDetail
        [HttpPost]
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        [ValidateAntiForgeryToken]
        public ActionResult createUserDetail(UserDetailModel model, string id)
        {
            try
            {
                GetSelectList(model);

                if (ModelState.IsValid)
                {
                    UserDetail userdetail = new UserDetail();
                    UserDetail result;
                    if (model.USE_Id != 0)
                    {
                        userdetail = db.UserDetails.Find(model.USE_Id);
                    }

                    userdetail.USE_Account = model.USE_Account;
                    userdetail.USE_Email = model.USE_Email;
                    userdetail.USE_FName = model.USE_FName;
                    userdetail.USE_LName = model.USE_LName;
                    userdetail.USE_NickName = model.USE_NickName;
                    userdetail.USE_Nationality = model.USE_Nationality;
                    userdetail.USE_Status = model.USE_Status;
                    userdetail.USE_TelNo = model.USE_TelNo;
                    userdetail.USE_Usercode = model.USE_Usercode;
                    userdetail.USE_Id = model.USE_Id;
                    userdetail.Dep_Id = model.Dep_Id;


                    if (model.USE_Id == 0)
                    {
                        //Create UserDetail
                        userdetail.USE_Password = "-";
                        result = db.UserDetails.Add(userdetail);
                        //Save Logs
                        if (result != null)
                        {
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            LogsReportModel logmodel = new LogsReportModel()
                            {
                                Log_Action_Id = result.USE_Id,
                                Log_System = "Administration",
                                Log_Type = "User Detail",
                                Log_Detail = string.Concat(result.USE_Account, "/"
                                            , result.USE_FName, "/", result.USE_LName, "/"
                                            , result.USE_NickName, "/",result.USE_Usercode),
                                Log_Action = "add",
                                Log_by = user_nickname
                            };
                            savelogs.SaveLogs(logmodel);
                        }
                    }
                    else
                    {
                        //Edit UserDetailx
                        db.Entry(userdetail).State = EntityState.Modified;
                        //Save Logs
                        if (userdetail != null)
                        {
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            LogsReportModel logmodel = new LogsReportModel()
                            {
                                Log_Action_Id = userdetail.USE_Id,
                                Log_System = "Administration",
                                Log_Type = "User Detail",
                                Log_Detail = string.Concat(userdetail.USE_Account, "/"
                                            , userdetail.USE_FName, "/", userdetail.USE_LName, "/"
                                            , userdetail.USE_NickName, "/", userdetail.USE_Usercode),
                                Log_Action = "edit",
                                Log_by = user_nickname
                            };
                            savelogs.SaveLogs(logmodel);
                        }
                    }

                    if (id != null)
                    { 
                        try
                        {
                            db.SaveChanges();
                            TempData["shortMessage"] = "Created User Detail successfully";
                            return RedirectToAction("userDetailList");
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "Cannot insert duplicate key row in UsersUserDetail Table.");
                            ModelState.AddModelError("", ex.Message);
                            ViewBag.Message = "Your user account and password have created, but user detail not insert to database. You can login to account and insert user detail again.";
                            return View(model);                           
                        }
                    }
                    else
                    {
                        ViewBag.errorMessage = "Your user account not create.";
                        return View("Error");
                    }
                }

                return View(model);

            }
            catch (Exception ioe) /*Exception dex */
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", ioe.Message);
                return View("Error");
            }

        }


        //
        // GET: /UserDetail/editUserDetail
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult editUserDetail(string id)
        {
            try
            {
                if (id == null)
                {
                    ViewBag.errorMessage = "Id is null";
                    return View("Error");
                }

                var result = db.AspNetUsers.Where(u => u.Id == id).FirstOrDefault();
                if (result == null)
                {
                    ViewBag.errorMessage = "Id not found in AspNetUser Table";
                    return View("Error");
                }

                var model = db.UserDetails.Where(u => u.USE_Usercode == result.USE_Usercode).Select(s => new UserDetailModel
                {
                    Dep_Id = s.Dep_Id,
                    USE_Account = s.USE_Account,
                    USE_Email = s.USE_Email,
                    USE_FName = s.USE_FName,
                    USE_Id = s.USE_Id,
                    USE_LName = s.USE_LName,
                    USE_Nationality = s.USE_Nationality,
                    USE_NickName = s.USE_NickName,
                    USE_Status = s.USE_Status,
                    USE_TelNo = s.USE_TelNo,
                    USE_Usercode = s.USE_Usercode,
                    Department_name = s.Department.Dep_Name
                }).FirstOrDefault();

                GetSelectList(model);

                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }

        }

        //
        // POST: /UserDetail/editUserDetail
        [HttpPost]
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        [ValidateAntiForgeryToken]
        public ActionResult editUserDetail(UserDetailModel model)
        {
            GetSelectList(model);

            if (ModelState.IsValid)
            {
                try
                {                  
                    UserDetail ud = db.UserDetails.Where(u => u.USE_Id == model.USE_Id).FirstOrDefault();
                    ud.Dep_Id = model.Dep_Id;
                    ud.USE_Account = model.USE_Account;
                    ud.USE_Email = model.USE_Email;
                    ud.USE_FName = model.USE_FName;
                    ud.USE_LName = model.USE_LName;
                    ud.USE_Nationality = model.USE_Nationality;
                    ud.USE_NickName = model.USE_NickName;
                    ud.USE_Status = model.USE_Status;
                    ud.USE_TelNo = model.USE_TelNo;
                    ud.USE_Usercode = model.USE_Usercode;

                    //Edit UserDetail
                    db.Entry(ud).State = EntityState.Modified;

                    AspNetUser result = db.AspNetUsers.Where(u => u.USE_Usercode == model.USE_Usercode).FirstOrDefault();
                    if (result != null)
                    {
                        result.Email = model.USE_Email;
                        result.UserName = model.USE_Account;
                        result.USE_Usercode = model.USE_Usercode;
                        //Edit User
                        db.Entry(result).State = EntityState.Modified;
                    }
                    
                    //Save 
                    db.SaveChanges();

                    //Save Logs
                    if (ud != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        LogsReportModel logmodel = new LogsReportModel()
                        {
                            Log_Action_Id = ud.USE_Id,
                            Log_System = "Administration",
                            Log_Type = "User Detail",
                            Log_Detail = string.Concat(ud.USE_Account, "/"
                                        , ud.USE_FName, "/", ud.USE_LName, "/"
                                        , ud.USE_NickName, "/", ud.USE_Usercode.ToString()),
                            Log_Action = "edit",
                            Log_by = user_nickname
                        };
                        savelogs.SaveLogs(logmodel);
                    }

                    TempData["shortMessage"] = "Edited successfully";
                    return RedirectToAction("userDetailList");

                }
                catch (Exception ex)
                {
                    if(ex.InnerException != null)
                    {
                        ViewBag.Message = String.Format("Cannot insert duplicate user name, The duplicate  value is {0}",model.USE_Account);
                        return View(model);
                    }
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.errorMessage = ex.Message.ToString(); //"Cannot insert duplicate key row in UsersUserDetail Table.";
                    return View("Error");

                }
            }
            // If we got this far, something failed, redisplay form
            //ViewBag.errorMessage = "Model Invalid";
            return View(model);

        }

        public UserDetailModel GetSelectList(UserDetailModel model)
        {
            List<Department> departmentList = db.Departments.OrderBy(o => o.Dep_Name).ToList();
            model.Departments = departmentList.Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();
            model.National = new[]
                    { 
                        new SelectListItem { Value = "Thai", Text = "Thai" }, 
                        new SelectListItem { Value = "Japanese", Text = "Japanese" } ,
                    }.ToList();

            return model;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}