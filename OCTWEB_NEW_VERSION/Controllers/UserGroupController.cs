using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using OCTWEB_NET45.Infrastructure;

namespace OCTWEB_NET45.Controllers
{
    [Authorize]
    public class UserGroupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private LogsReportController savelogs = new LogsReportController();

        //
        // GET: /UserGroup/UserGroupList
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult userGroupList()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                var model = db.UserGroups
                    .Select(s => new UserGroupModel
                    {
                        USG_Id = s.USG_Id,
                        USG_Memo = s.USG_Memo,
                        USG_Name = s.USG_Name,
                        memberNumber = s.GroupMembers.Count()
                    }).ToList();


                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //POST: /UserGroup/userGroupList
        [HttpPost]
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult userGroupList(FormCollection form)
        {
            try
            {
                string groupname = null;
                if (!string.IsNullOrEmpty(form["searchGroupname"]))
                {
                    groupname = form["searchGroupname"];
                }
                IEnumerable<UserGroup> query = db.UserGroups;
                if (groupname != null)
                {
                    query = query.Where(x => String.Equals(x.USG_Name, groupname, StringComparison.CurrentCultureIgnoreCase));
                }
                var usergroup_list = query.Select(s => new UserGroupModel
                {
                    USG_Id = s.USG_Id,
                    USG_Memo = s.USG_Memo,
                    USG_Name = s.USG_Name,
                    memberNumber = s.GroupMembers.Count()
                }).ToList();

                ViewData["searchGroupname"] = groupname;
                TempData["shortMessage"] = String.Format("Searching success: found {0} items. ", usergroup_list.Count());
                return View(usergroup_list);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        // GET: /UserGroup/createUserGroup
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult createUserGroup()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                UserGroupModel model = new UserGroupModel();

                return View(model);
            
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
           
        }

        //
        // POST: /UserGroup/createUserGroup
        [HttpPost]
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult createUserGroup(UserGroupModel model)
        {
            try
            {
                UserGroup usergroup = new UserGroup()
                {
                    USG_Memo = model.USG_Memo,
                    USG_Name = model.USG_Name
                };

                var result = db.UserGroups.Add(usergroup);
                db.SaveChanges();
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
                        Log_Action_Id = result.USG_Id,
                        Log_System = "Administration",
                        Log_Type = "User Groups",
                        Log_Detail = string.Concat(result.USG_Name, "/"
                                    , result.USG_Memo),
                        Log_Action = "add",
                        Log_by = user_nickname
                    };
                    savelogs.SaveLogs(logmodel);
                }

                TempData["shortMessage"] = String.Format("Created successfully, {0} group. ", model.USG_Name);
                return RedirectToAction("userGroupList");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // GET: /UserGroup/editUserGroup
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult editUserGroup(int id)
        {
            try
            {
                var model = db.UserGroups.Where(x => x.USG_Id == id)
                    .Select(s => new UserGroupModel { USG_Id = s.USG_Id, USG_Memo = s.USG_Memo, USG_Name = s.USG_Name })
                    .FirstOrDefault();
                if (model != null)
                {
                    return View(model);
                }
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
            ViewBag.errorMessage = "User group is null";
            return View("Error");
            
        }

        //
        // POST: /UserGroup/editUserGroup
        [HttpPost]
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult editUserGroup(UserGroupModel model)
        {
            try
            {
                UserGroup usergroup = db.UserGroups.Where(x => x.USG_Id == model.USG_Id).FirstOrDefault();
                if (usergroup != null)
                {
                    usergroup.USG_Memo = model.USG_Memo;
                    usergroup.USG_Name = model.USG_Name;
                    //Edit UserGroup
                    db.Entry(usergroup).State = EntityState.Modified;
                    db.SaveChanges();
                    //Save Logs
                    if (usergroup != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        LogsReportModel logmodel = new LogsReportModel()
                        {
                            Log_Action_Id = usergroup.USG_Id,
                            Log_System = "Administration",
                            Log_Type = "User Groups",
                            Log_Detail = string.Concat(usergroup.USG_Name, "/"
                                        , usergroup.USG_Memo),
                            Log_Action = "edit",
                            Log_by = user_nickname
                        };
                        savelogs.SaveLogs(logmodel);
                    }

                    TempData["shortMessage"] = String.Format("Edited successfully, {0} group. ", model.USG_Name);
                    return RedirectToAction("userGroupList");
                }
                
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
            ViewBag.errorMessage = "User group is null";
            return View("Error");
        }


        //
        // POST: /UserGroup/deleteUserGroup
        [HttpPost]
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult deleteUserGroup(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var usergrop_list = db.UserGroups.Where(x => id_list.Contains(x.USG_Id)).ToList();

                    db.UserGroups.RemoveRange(usergrop_list);
                    db.SaveChanges();
                    //Save Logs
                    if (usergrop_list != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        foreach (var i in usergrop_list)
                        {
                            LogsReportModel logmodel = new LogsReportModel()
                            {
                                Log_Action_Id = i.USG_Id,
                                Log_System = "Administration",
                                Log_Type = "User Groups",
                                Log_Detail = string.Concat(i.USG_Name, "/"
                                            , i.USG_Memo),
                                Log_Action = "delete",
                                Log_by = user_nickname
                            };
                            savelogs.SaveLogs(logmodel);
                        }
                    }
                    TempData["shortMessage"] = String.Format("Deleted successfully, {0} groups. ", id_list.Count());
                    return RedirectToAction("userGroupList");
                   
                }
                return RedirectToAction("userGroupList");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //GET : UserGroup/addGroupMember
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult addGroupMember(int id,UserDetailPagedListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;    

                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                var usergroup = db.UserGroups.Where(x => x.USG_Id == id).FirstOrDefault();

                var userdetail_list = db.GroupMembers.Where(x => x.USG_Id == id).Select(s=>s.USE_Id).ToList();

                var ud_list = db.UserDetails.Where(x => userdetail_list.Contains(x.USE_Id))
                    .Select(s=> new UserDetailModel {USE_Id=s.USE_Id,USE_FName=s.USE_FName,USE_NickName=s.USE_NickName,USE_Usercode=s.USE_Usercode }).ToList();

                if (usergroup !=null)
                {
                    IEnumerable<UserDetailModel> selectModel = userDetaislList(userdetail_list);
                    DepartmentsDropDownList();
                    ViewData["groupid"] = usergroup.USG_Id;
                    ViewData["groupName"] = usergroup.USG_Name;
                    ViewData["selectedmembers"] = String.Join(", ", userdetail_list);
                    ViewBag.SelectedMembers = ud_list;
                    model.UserDetailModelPagedList = selectModel.ToPagedList(pageIndex, pageSize);
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
            return View();
        }

        //POST: /UserGroup/addGroupMember
        [HttpPost]
        [CustomAuthorize(8)]//User Groups/Setup/Administration
        public ActionResult addGroupMember(FormCollection form, UserDetailPagedListModel model)
        {
            try
            {
                ViewData["groupid"] = form["groupid"];
                ViewData["groupName"] = form["groupName"];
               
                if(form["Add"] == "Add"){//Save

                    return saveUserMembers(form);
                }
                if (form["Search"] == "Search")
                {
                    return selectUserDetaislList(form,model);
                }
                if (form["Delete"] == "Delete")
                {
                    return deleteUserMembers(form);
                }


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            ViewBag.errorMessage = "group id is null";
            return View("Error");
        }


        //For Add/Save
        public ActionResult saveUserMembers(FormCollection form)
        {
            try
            {
                int groupId = 0;
                if (!String.IsNullOrEmpty(form["groupid"]))
                {
                     groupId = int.Parse(form["groupid"]);
                }
                var selectedItem = form["selectedItem-add"];
                List<int> id_selected = new List<int>();
                if (!String.IsNullOrEmpty(selectedItem))
                {
                    id_selected = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                }

                var groupmembersAdded = db.GroupMembers.Where(x => x.USG_Id == groupId).Where(e => id_selected.Contains(e.USE_Id)).Select(s => s.USE_Id).ToList();

                var id_except = id_selected.Except(groupmembersAdded).ToList();

                List<GroupMember> gmember = new List<GroupMember>();
                foreach (var i in id_except)
                {
                    gmember.Add(new GroupMember { USG_Id = groupId, USE_Id = i });
                }
                db.GroupMembers.AddRange(gmember);
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Saved successfully, {0} members. ", id_selected.Count());
                return RedirectToAction("addGroupMember");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }           
            
        }
        //For Delete
        public ActionResult deleteUserMembers(FormCollection form)
        {
            try
            {
                int groupId = 0;
                if (!String.IsNullOrEmpty(form["groupid"]))
                {
                     groupId = int.Parse(form["groupid"]);
                }
               
                var selectedToDeleteMember = form["selectedToDeleteMember"];
                List<int> id_selected = new List<int>();
                if (!String.IsNullOrEmpty(selectedToDeleteMember))
                {
                    id_selected = selectedToDeleteMember.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                }

                var group = db.UserGroups.Where(x => x.USG_Id == groupId).FirstOrDefault();
                var groupmember = db.GroupMembers.Where(x => x.USG_Id == groupId).Where(x => id_selected.Contains(x.USE_Id)).ToList();

                db.GroupMembers.RemoveRange(groupmember);
                db.SaveChanges();

                TempData["shortMessage"] = String.Format("Deleted successfully, {0} members. ", id_selected.Count());
                return RedirectToAction("addGroupMember");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }           
            
        }

        //For Search
        public ActionResult selectUserDetaislList(FormCollection form, UserDetailPagedListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;   

                string fname = null;
                string nickname = null;
                int? dep = null;
                int groupId = 0;
                if (!String.IsNullOrEmpty(form["groupid"]))
                {
                     groupId = int.Parse(form["groupid"]);
                }

                if (!string.IsNullOrEmpty(form["searchFname"]))
                {
                    fname = form["searchFname"];
                }
                if (!string.IsNullOrEmpty(form["searchNickname"]))
                {
                    nickname = form["searchNickname"];
                }
                if (!string.IsNullOrEmpty(form["searchDep"]))
                {
                    dep = int.Parse(form["searchDep"]);
                }

                var userdetail_list = db.GroupMembers.Where(x => x.USG_Id == groupId).Select(s=>s.USE_Id).ToList();
                var ud_list = db.UserDetails.Where(x => userdetail_list.Contains(x.USE_Id));

                IEnumerable<UserDetail> query = db.UserDetails;

                if (userdetail_list != null)
                {
                    query = query.Where(x => !userdetail_list.Contains(x.USE_Id));
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

                ViewBag.SelectedMembers = ud_list;
                ViewData["searchFname"] = fname;
                ViewData["searchNickname"] = nickname;
                DepartmentsDropDownList(dep);
                TempData["shortMessage"] = String.Format("Searching fonded, {0} members. ", userDetailList.Count());
                model.UserDetailModelPagedList = userDetailList.ToPagedList(pageIndex, pageSize);
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }

      
        public IEnumerable<UserDetailModel> userDetaislList(List<int> userdetail_id = null)
        {
           
            IEnumerable<UserDetail> query = db.UserDetails;
            if (userdetail_id != null)
            {
                query = query.Where(x => !userdetail_id.Contains(x.USE_Id));
            }
            IEnumerable<UserDetailModel> userDetailList = query.Select(s => new UserDetailModel
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

            return userDetailList;
        }


        private void DepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in db.Departments
                                   orderby d.Dep_Name
                                   select d;
            ViewBag.DepartmentID = new SelectList(departmentsQuery, "Dep_Id", "Dep_Name", selectedDepartment);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
