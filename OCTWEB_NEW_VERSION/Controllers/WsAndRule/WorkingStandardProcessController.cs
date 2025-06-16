using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class WorkingStandardProcessController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private LogsReportController savelogs = new LogsReportController();

        //
        // GET: /WorkingStandardProcess/WSProcessSetList
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult WSProcessSetList(WorkingStandardProcessSetModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                if (TempData["shortError"] != null)
                {
                    ViewBag.ErrorMessage = TempData["shortError"].ToString();
                }
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                int? searchDepartmentId = model.searchDepartmentId;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;

                IEnumerable<WSR_WorkingStandardProcessEdit> query = model.Page.HasValue ? db.WSR_WorkingStandardProcessEdit : db.WSR_WorkingStandardProcessEdit.Take(0);
                if (searchDepartmentId > 0)
                {
                    List<int> wst_id_list = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Dept == searchDepartmentId)
                       .Select(s => s.WST_Id).ToList();
                    query = query.Where(x => wst_id_list.Contains(x.WST_Id));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardType))
                {
                    List<int> wst_id_list = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Type == searchWorkingStandardType)
                      .Select(s => s.WST_Id).ToList();
                    query = query.Where(x => wst_id_list.Contains(x.WST_Id));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardProcess))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WSP_ProcessName)
                        && x.WSP_ProcessName.ToLowerInvariant().Contains(searchWorkingStandardProcess.ToLowerInvariant())));
                }
                var wsp_list = query.Select(s => new WSR_WorkingStandardProcessEditModel
                {
                    WSP_Id = s.WSP_Id,
                    WSP_ProcessName = s.WSP_ProcessName,
                    WSP_Note = s.WSP_Note,
                    WST_Id = s.WST_Id,
                    WorkingStandardType = (from wst in db.WSR_WorkingStandardTypeEdit
                                           where wst.WST_Id == s.WST_Id
                                           select wst.WST_Type).FirstOrDefault(),
                    DepartmentName = (from wst in db.WSR_WorkingStandardTypeEdit
                                      join dep in db.DepartmentEdits on wst.WST_Dept equals dep.Dep_Id
                                      where wst.WST_Id == s.WST_Id
                                      select dep.Dep_Name).FirstOrDefault()
                });

                IPagedList<WSR_WorkingStandardProcessEditModel> wsp_pagedList = wsp_list.ToPagedList(pageIndex, pageSize);

                model.WSR_WorkingStandardProcessEditModelPagedList = wsp_pagedList;

                GetSelectOptions(model);

                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }

        }

        public WorkingStandardProcessSetModel GetSelectOptions(WorkingStandardProcessSetModel model)
        {
            //Select department
            List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

            //Select working standard type
            IEnumerable<WSR_WorkingStandardTypeEdit> query_wst = db.WSR_WorkingStandardTypeEdit;
            query_wst = model.searchDepartmentId.HasValue ? query_wst.Where(w => w.WST_Dept == model.searchDepartmentId).ToList()
                : query_wst;
            List<SelectListItem> SelectWorkingStandardType_list = query_wst.Select(s => s.WST_Type).Distinct().OrderBy(o => o)
                .Select(s => new SelectListItem { Value = s, Text = s }).ToList();

            model.SelectDepartmentId = SelectDepartment_list;
            model.SelectWorkingStandardType = SelectWorkingStandardType_list;

            return model;
        }

        //
        // POST: /WorkingStandardProcess/WSProcessSetList
        [HttpPost]
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult WSProcessSetList(FormCollection form, WorkingStandardProcessSetModel model)
        {
            try
            {

                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                int pageSize = 30;
                int pageIndex = 1;

                int? searchDepartmentId = model.searchDepartmentId;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;

                IEnumerable<WSR_WorkingStandardProcessEdit> query = db.WSR_WorkingStandardProcessEdit;
                if (searchDepartmentId > 0)
                {
                    List<int> wst_id_list = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Dept == searchDepartmentId)
                       .Select(s => s.WST_Id).ToList();
                    query = query.Where(x => wst_id_list.Contains(x.WST_Id));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardType))
                {
                    List<int> wst_id_list = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Type == searchWorkingStandardType)
                       .Select(s => s.WST_Id).ToList();
                    query = query.Where(x => wst_id_list.Contains(x.WST_Id));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardProcess))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WSP_ProcessName)
                        && x.WSP_ProcessName.ToLowerInvariant().Contains(searchWorkingStandardProcess.ToLowerInvariant())));
                }
                List<WSR_WorkingStandardProcessEditModel> wsp_list = query.Select(s => new WSR_WorkingStandardProcessEditModel
                {
                    WSP_Id = s.WSP_Id,
                    WSP_ProcessName = s.WSP_ProcessName,
                    WSP_Note = s.WSP_Note,
                    WST_Id = s.WST_Id,
                    WorkingStandardType = (from wst in db.WSR_WorkingStandardTypeEdit
                                           where wst.WST_Id == s.WST_Id
                                           select wst.WST_Type).FirstOrDefault(),
                    DepartmentName = (from wst in db.WSR_WorkingStandardTypeEdit
                                      join dep in db.DepartmentEdits on wst.WST_Dept equals dep.Dep_Id
                                      where wst.WST_Id == s.WST_Id
                                      select dep.Dep_Name).FirstOrDefault()
                }).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(wsp_list);
                }

                IPagedList<WSR_WorkingStandardProcessEditModel> wsp_pagedList = wsp_list.ToPagedList(pageIndex, pageSize);

                model.WSR_WorkingStandardProcessEditModelPagedList = wsp_pagedList;

                GetSelectOptions(model);

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
        // GET: /WorkingStandardProcess/Create
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult Create()
        {
            try
            {
                WSR_WorkingStandardProcessEditModel model = new WSR_WorkingStandardProcessEditModel();

                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                // Select working standard type
                List<SelectListItem> SelectWorkingStandardType_list = db.WSR_WorkingStandardTypeEdit.OrderBy(o => o.WST_Type)
                    .Select(s => new SelectListItem { Value = s.WST_Id.ToString(), Text = s.WST_Type }).ToList();

                model.SelectDepartmentId = SelectDepartment_list;
                model.SelectWorkingStandardTypeId = SelectWorkingStandardType_list;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        //
        // POST: /WorkingStandardProcess/Create
        [HttpPost]
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult Create(FormCollection form, WSR_WorkingStandardProcessEditModel model)
        {
            try
            {
                GetSelectWST(model);

                if (ModelState.IsValid)
                {
                    WSR_WorkingStandardProcessEdit wsp = new WSR_WorkingStandardProcessEdit()
                    {
                        WSP_ProcessName = model.WSP_ProcessName,
                        WSP_Note = model.WSP_Note,
                        WST_Id = model.WST_Id
                    };
                    //Check add duplicate
                    var check = db.WSR_WorkingStandardProcessEdit
                        .Where(x => x.WST_Id == model.WST_Id && x.WSP_ProcessName == model.WSP_ProcessName).FirstOrDefault();
                    if (check != null)
                    {
                        return Json(new { View = model, success = false, message = "Invalid data, Duplicate on process name" });
                    }

                    var result = db.WSR_WorkingStandardProcessEdit.Add(wsp);

                    //Save Logs
                    if (result != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Working Standard Process",
                            Log_System = "WS & Rule",
                            Log_Detail = string.Concat(result.WSP_ProcessName, "/"
                                        , result.WST_Id),
                            Log_Action_Id = result.WSP_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }

                    ViewBag.Message = String.Format("Successfully created");

                    db.SaveChanges();

                    TempData["shortMessage"] = string.Format("Created successfully, {0} item.", model.WSP_ProcessName);

                    return Json(new
                    {
                        success = true,
                        message = "Successfully created",
                        redirectUrl = Url.Action("WSProcessSetList", new  { Page = 1, searchWorkingStandardProcess = model.WSP_ProcessName })
                    });

                }

                return View(model);

            }
            catch (Exception ex)
            {
                return View(Json(new { success = false, message = ex.Message }));
            }
        }

        public WSR_WorkingStandardProcessEditModel GetSelectWST(WSR_WorkingStandardProcessEditModel model)
        {
            try
            {
                int searchDepartmentId = model.searchDepartmentId > 0 ? model.searchDepartmentId : 0;

                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                // Select working standard type
                IEnumerable<WSR_WorkingStandardTypeEdit> query_wst = db.WSR_WorkingStandardTypeEdit;
                query_wst = searchDepartmentId > 0 ? query_wst.Where(w => w.WST_Dept == searchDepartmentId).ToList()
                    : query_wst;
                List<SelectListItem> SelectWorkingStandardType_list = query_wst.OrderBy(o => o.WST_Type)
                    .Select(s => new SelectListItem { Value = s.WST_Id.ToString(), Text = s.WST_Type }).ToList();

                model.SelectDepartmentId = SelectDepartment_list;
                model.SelectWorkingStandardTypeId = SelectWorkingStandardType_list;

                return model;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return model;
            }


        }

        //
        // GET: /WorkingStandardProcess/Edit/5
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult Edit(int id)
        {
            var model = db.WSR_WorkingStandardProcessEdit.Where(x => x.WSP_Id == id)
                .Select(s => new WSR_WorkingStandardProcessEditModel
                {
                    WSP_Id = s.WSP_Id,
                    WSP_ProcessName = s.WSP_ProcessName,
                    WSP_Note = s.WSP_Note,
                    WST_Id = s.WST_Id,
                    searchDepartmentId = (from wst in db.WSR_WorkingStandardTypeEdit
                                          where wst.WST_Id == s.WST_Id
                                          select wst.WST_Dept).FirstOrDefault()
                }).FirstOrDefault();

            if (model != null)
            {
                GetSelectWST(model);
                return View(model);
            }

            ViewBag.errorMessage = "Working Standard Process Id is null";
            return View("Error");
        }

        //
        // POST: /WorkingStandardProcess/Edit/5
        [HttpPost]
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult Edit(WSR_WorkingStandardProcessEditModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WSR_WorkingStandardProcessEdit wsp = db.WSR_WorkingStandardProcessEdit.Where(x => x.WSP_Id == model.WSP_Id).FirstOrDefault();
                    if (wsp != null)
                    {
                        //wsp.WST_Id = model.WST_Id;
                        wsp.WSP_ProcessName = model.WSP_ProcessName;
                        wsp.WSP_Note = model.WSP_Note;
                        //Check add duplicate
                        var check = db.WSR_WorkingStandardProcessEdit
                            .Where(x => x.WST_Id == model.WST_Id && x.WSP_ProcessName == model.WSP_ProcessName && x.WSP_Id != wsp.WSP_Id).FirstOrDefault();
                        if (check != null)
                        {
                            return Json(new { success = false, message = "Invalid data, Duplicate on process name" });
                        }
                        else
                        {
                            db.Entry(wsp).State = EntityState.Modified;

                            //Save Logs
                            if (wsp != null)
                            {
                                string user_nickname = null;
                                if (Session["NickName"] != null)
                                {
                                    user_nickname = Session["NickName"].ToString();
                                }
                                Log logmodel = new Log()
                                {
                                    Log_Action = "edit",
                                    Log_Type = "Working Standard Process",
                                    Log_System = "WS & Rule",
                                    Log_Detail = string.Concat(wsp.WSP_ProcessName, "/"
                                                , wsp.WST_Id),
                                    Log_Action_Id = wsp.WSP_Id,
                                    Log_Date = DateTime.Now,
                                    Log_by = user_nickname
                                };
                                db.Logs.Add(logmodel);
                            }

                            db.SaveChanges();

                            TempData["shortMessage"] = String.Format("Edited successfully, {0} item. ", model.WSP_ProcessName);
                            return Json(new
                            {
                                success = true,
                                message = "Edit successfully",
                                redirecUrl = Url.Action("WSProcessSetList", new { Page = 1, searchWorkingStandardProcess = model.WSP_ProcessName })
                            });
                        }

                    }
                    else
                    {  
                        return Json(new { success = false, message = "Working Standard Process Id is null" });
                    }
                }
                GetSelectWST(model);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }


        // POST: /WorkingStandardProcess/Delete/5
        [HttpPost]
        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var wsp_list = db.WSR_WorkingStandardProcessEdit.Where(x => id_list.Contains(x.WSP_Id)).ToList();
                    foreach (var i in wsp_list)
                    {
                        var check = db.WSR_WorkingStandardEdit.Where(x => x.WSP_Id == i.WSP_Id).FirstOrDefault();
                        if (check != null)
                        {
                            
                            return Json(new { success = false, message = "Cannot delete working standard because it is active." });
                        }
                        else
                        {
                            db.WSR_WorkingStandardProcessEdit.Remove(i);
                            //Save Logs
                            if (wsp_list != null)
                            {
                                string user_nickname = null;
                                if (Session["NickName"] != null)
                                {
                                    user_nickname = Session["NickName"].ToString();
                                }

                                Log logmodel = new Log()
                                {
                                    Log_Action = "delete",
                                    Log_Type = "Working Standard Process",
                                    Log_System = "WS & Rule",
                                    Log_Detail = string.Concat(i.WSP_ProcessName, "/"
                                                , i.WST_Id),
                                    Log_Action_Id = i.WSP_Id,
                                    Log_Date = DateTime.Now,
                                    Log_by = user_nickname
                                };
                                db.Logs.Add(logmodel);

                            }

                            db.SaveChanges();
                            TempData["shortMessage"] = $"Deleted successfully, {id_list.Count()} items.";     
                            return Json(new { success = true, message = "Deleted successfully." });
                        }
                    }
                }
                return Json(new { success = false, message = "No items selected for deletion." });

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [CustomAuthorize(23)]//2 Working standard Process/Setup WS/WS & Rule
        public void ExportToCsv(List<WSR_WorkingStandardProcessEditModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new WSR_WorkingStandardProcessEditModel
                {
                    item = i + 1,
                    WSP_ProcessName = v.WSP_ProcessName,
                    WorkingStandardType = v.WorkingStandardType,
                    WSP_Note = "\"" + v.WSP_Note + "\"",
                    DepartmentName = "\"" + v.DepartmentName + "\"",
                }).OrderBy(o => o.DepartmentName);

                sb.AppendFormat("{0},{1},{2},{3},{4},{5}",
                    "Item", "Department", "WorkingStandardType", "WorkingStandardProcess", "Note"
                    , Environment.NewLine);

                foreach (var item in forexport.Select((value, i) => new { i, value }))
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5}",
                        item.i + 1,
                        item.value.DepartmentName,
                        item.value.WorkingStandardType,
                        item.value.WSP_ProcessName,
                        item.value.WSP_Note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=WorkingStandardProcess.CSV ");
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
        // POST: /WorkingStandardProcess/GetWorkingStandardProcessName/
        [HttpPost]
        public JsonResult GetWorkingStandardProcessName(string Prefix)
        {
            var wspname = db.WSR_WorkingStandardProcessEdit
                            .Where(x => x.WSP_ProcessName.StartsWith(Prefix))
                            .Take(10)
                            .Select(s => new { label = s.WSP_ProcessName, val = s.WSP_ProcessName }).ToList();
            return Json(wspname);
        }

        // /WorkingStandardProcess/GetDepartment/
        public JsonResult GetDepartment()
        {
            var select_departments = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                                .Select(s => new { label = s.Dep_Name, val = s.Dep_Id }).ToList();
            return Json(select_departments, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardProcess/GetWSType/
        [HttpPost]
        public JsonResult GetWSType(int? department)
        {
            IEnumerable<WSR_WorkingStandardTypeEdit> query = db.WSR_WorkingStandardTypeEdit;
            if (department.HasValue)
            {
                query = query.Where(x => x.WST_Dept == department.Value).ToList();
            }
            var select_wst = query.Select(s => s.WST_Type).Distinct().OrderBy(o => o)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(select_wst, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardProcess/GetWSType/
        [HttpPost]
        public JsonResult GetWSTypeCreate(int department)
        {
            var select_wst = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Dept == department)
                            .Select(s => new { label = s.WST_Type, val = s.WST_Id }).ToList();
            return Json(select_wst, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
