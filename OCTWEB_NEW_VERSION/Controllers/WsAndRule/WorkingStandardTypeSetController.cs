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
    public class WorkingStandardTypeSetController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        //
        // GET: /WorkingStandardTypeSet/WSTypeSetList
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult WSTypeSetList(WorkingStandardTypeSetModel model)
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

                int searchDepartmentId = 0;
                string searchWorkingStandardType = null;

                searchDepartmentId = model.searchDepartmentId > 0 ? model.searchDepartmentId : 0;
                searchWorkingStandardType = !String.IsNullOrEmpty(model.searchWorkingStandardType) ? model.searchWorkingStandardType : null;

                IEnumerable<WSR_WorkingStandardTypeEdit> query = model.Page.HasValue ? db.WSR_WorkingStandardTypeEdit : db.WSR_WorkingStandardTypeEdit.Take(0);
                if (searchDepartmentId > 0)
                {
                    query = query.Where(x => x.WST_Dept == searchDepartmentId);
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardType))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.WST_Type)
                        && x.WST_Type.ToLowerInvariant().Contains(searchWorkingStandardType.ToLowerInvariant()));
                }

                var wst_list = query.Select(s => new WSR_WorkingStandardTypeEditModel
                {
                    WST_Id = s.WST_Id,
                    WST_Dept = s.WST_Dept,
                    WST_Type = s.WST_Type,
                    WST_Note = s.WST_Note,
                    DepartmentName = (from dept in db.DepartmentEdits
                                      where dept.Dep_Id == s.WST_Dept
                                      select dept.Dep_Name).FirstOrDefault()
                });

                IPagedList<WSR_WorkingStandardTypeEditModel> wstPagedList = wst_list.ToPagedList(pageIndex, pageSize);

                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.WSR_WorkingStandardTypeEditModelPagedList = wstPagedList;
                model.SelectDepartmentId = SelectDepartment_list;

                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // POST: /WorkingStandardTypeSet/WSTypeSetList
        [HttpPost]
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult WSTypeSetList(FormCollection form, WorkingStandardTypeSetModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int searchDepartmentId = 0;
                string searchWorkingStandardType = null;

                searchDepartmentId = model.searchDepartmentId > 0 ? model.searchDepartmentId : 0;
                searchWorkingStandardType = !String.IsNullOrEmpty(model.searchWorkingStandardType) ? model.searchWorkingStandardType : null;

                IEnumerable<WSR_WorkingStandardTypeEdit> query = db.WSR_WorkingStandardTypeEdit;
                if (searchDepartmentId > 0)
                {
                    query = query.Where(x => x.WST_Dept == searchDepartmentId);
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardType))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.WST_Type)
                       && x.WST_Type.ToLowerInvariant().Contains(searchWorkingStandardType.ToLowerInvariant()));
                }

                var wst_list = query.Select(s => new WSR_WorkingStandardTypeEditModel
                {
                    WST_Id = s.WST_Id,
                    WST_Dept = s.WST_Dept,
                    WST_Type = s.WST_Type,
                    WST_Note = s.WST_Note,
                    DepartmentName = (from dept in db.DepartmentEdits
                                      where dept.Dep_Id == s.WST_Dept
                                      select dept.Dep_Name).FirstOrDefault()
                }).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(wst_list);
                }

                IPagedList<WSR_WorkingStandardTypeEditModel> wstPagedList = wst_list.ToPagedList(pageIndex, pageSize);

                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.WSR_WorkingStandardTypeEditModelPagedList = wstPagedList;
                model.SelectDepartmentId = SelectDepartment_list;

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
        // GET: /WorkingStandardTypeSet/Create
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult Create()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                WSR_WorkingStandardTypeEditModel model = new WSR_WorkingStandardTypeEditModel();

                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmentId = SelectDepartment_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }

        }

        //
        // POST: /WorkingStandardTypeSet/Create
        [HttpPost]
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult Create(WSR_WorkingStandardTypeEditModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WSR_WorkingStandardTypeEdit wst = new WSR_WorkingStandardTypeEdit()
                    {
                        WST_Dept = model.WST_Dept,
                        WST_Type = model.WST_Type,
                        WST_Note = model.WST_Note
                    };
                    //Check add duplicate
                    var check = db.WSR_WorkingStandardTypeEdit
                        .Where(x => x.WST_Dept == model.WST_Dept && x.WST_Type == model.WST_Type).FirstOrDefault();
                    if (check != null)
                    {
                        ModelState.AddModelError("", "Invalid data, Duplicate on department and working standard type");
                        return Json(new { success = false, message = "Invalid data, Duplicate on department and working standard type." });
                    }

                    else
                    {
                        var result = db.WSR_WorkingStandardTypeEdit.Add(wst);

                        //Save log
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
                                Log_Type = "Working Standard Type",
                                Log_System = "WS & Rule",
                                Log_Detail = String.Concat(result.WST_Type, "/"
                                            , result.WST_Dept),
                                Log_Action_Id = result.WST_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            ViewBag.Message = String.Format("db.Logs.Add(logmodel)");
                            db.Logs.Add(logmodel);
                        }
                        ViewBag.Message = String.Format("Successfully created");

                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully created the Working Standard {0}", model.WST_Type);

                        return Json(new
                        {
                            success = true,
                            redirectUrl = Url.Action("WSTypeSetList", new WorkingStandardTypeSetModel { Page = 1, searchWorkingStandardType = model.WST_Type })
                        });
                    }

                }
                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmentId = SelectDepartment_list;

                ViewBag.Message = "Please correct the highlighted errors.";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("An unexpected error occurred: ", ex.InnerException.Message);
                return View();
            }
        }

        //
        // GET: /WorkingStandardTypeSet/Edit/5
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult Edit(int id)
        {
            try
            {
                var model = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Id == id)
                    .Select(s => new WSR_WorkingStandardTypeEditModel
                    {
                        WST_Id = s.WST_Id,
                        WST_Type = s.WST_Type,
                        WST_Note = s.WST_Note,
                        WST_Dept = s.WST_Dept,

                    }).FirstOrDefault();
                if (model != null)
                {
                    //Select department
                    List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                        .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                    model.SelectDepartmentId = SelectDepartment_list;

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
            ViewBag.errorMessage = "Working Standard Type Id is null";
            return View("Error");
        }

        //
        // POST: /WorkingStandardTypeSet/Edit/5
        [HttpPost]
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult Edit(WSR_WorkingStandardTypeEditModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WSR_WorkingStandardTypeEdit wst = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Id == model.WST_Id).FirstOrDefault();
                    if (wst != null)
                    {
                        wst.WST_Dept = model.WST_Dept;
                        wst.WST_Type = model.WST_Type;
                        wst.WST_Note = model.WST_Note;
                        //Check add duplicate
                        var check = db.WSR_WorkingStandardTypeEdit
                            .Where(x => x.WST_Dept == model.WST_Dept && x.WST_Type == model.WST_Type && x.WST_Id != wst.WST_Id).FirstOrDefault();
                        if (check != null)
                        {
                            return Json(new { succes = true, message = "Invalid data, Duplicate on department and working standard type." });
                        }
                        else
                        {
                            db.Entry(wst).State = EntityState.Modified;

                            //Save log
                            if (wst != null)
                            {
                                string user_nickname = null;
                                if (Session["NickName"] != null)
                                {
                                    user_nickname = Session["NickName"].ToString();
                                }
                                Log logmodel = new Log()
                                {
                                    Log_Action = "edit",
                                    Log_Type = "Working Standard Type",
                                    Log_System = "WS & Rule",
                                    Log_Detail = String.Concat(wst.WST_Type, "/"
                                                , wst.WST_Dept),
                                    Log_Action_Id = wst.WST_Id,
                                    Log_Date = DateTime.Now,
                                    Log_by = user_nickname
                                };
                                db.Logs.Add(logmodel);
                            }
                            db.SaveChanges();

                            TempData["shortMessage"] = String.Format("Successfully edited, {0} item. ", model.WST_Type);

                            return Json(new { success = true, message = "Successfully edited.", redirectUrl = Url.Action("WSTypeSetList", new WorkingStandardTypeSetModel { Page = 1, searchWorkingStandardType = model.WST_Type }) });
                        }

                    }
                    else
                    {
                        ViewBag.errorMessage = "Working Standard Type Id is null";
                        return View("Error");
                    }
                }
                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectDepartmentId = SelectDepartment_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // POST: /WorkingStandardTypeSet/Delete/5
        [HttpPost]
        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var wst_list = db.WSR_WorkingStandardTypeEdit.Where(x => id_list.Contains(x.WST_Id)).ToList();

                    foreach (var i in wst_list)
                    {
                        var check = db.WSR_WorkingStandardEdit.Where(x => x.WST_Id == i.WST_Id).FirstOrDefault();
                        if (check != null)
                        {
                            return Json(new { success = false, message = "Cannot delete working standard tye because it is active." });
                        }
                        var check_process = db.WSR_WorkingStandardProcessEdit.Where(x => x.WST_Id == i.WST_Id).FirstOrDefault();
                        if (check_process != null)
                        {
                            return Json(new { success = false, message = "Cannot delete working standard tye because it is active." });
                        }
                        db.WSR_WorkingStandardTypeEdit.Remove(i);
                    }

                    //Save Logs
                    if (wst_list != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        foreach (var i in wst_list)
                        {
                            Log logmodel = new Log()
                            {
                                Log_Action = "delete",
                                Log_Type = "Working Standard Type",
                                Log_System = "WS & Rule",
                                Log_Detail = String.Concat(i.WST_Type, "/"
                                            , i.WST_Dept),
                                Log_Action_Id = i.WST_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }
                    }
                    db.SaveChanges();
                    TempData["shortMessage"] = $"Successfully deleted {id_list.Count()} items.";
                    return Json(new { success = true, message = "Deleted successfully." });
                }
                return RedirectToAction("WSTypeSetList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        [CustomAuthorize(20)]//1 Working standard Type/Setup WS/WS & Rule
        public void ExportToCsv(List<WSR_WorkingStandardTypeEditModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new WSR_WorkingStandardTypeEditModel
                {
                    item = i + 1,
                    WST_Type = v.WST_Type,
                    WST_Note = "\"" + v.WST_Note + "\"",
                    DepartmentName = v.DepartmentName,

                }).OrderBy(o => o.DepartmentName);

                sb.AppendFormat("{0},{1},{2},{3},{4}",
                    "Item", "Department", "WorkingStandardType", "Note", Environment.NewLine);

                foreach (var item in forexport.Select((value, i) => new { i, value }))
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4}",
                        item.i + 1,
                        item.value.DepartmentName,
                        item.value.WST_Type,
                        item.value.WST_Note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=WorkingStandardType.CSV ");
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
        // POST: /WorkingStandardTypeSet/GetWorkingStandardTypeName/
        [HttpPost]
        public JsonResult GetWorkingStandardTypeName(string Prefix)
        {
            var wstname = (from wst in db.WSR_WorkingStandardTypeEdit
                           where wst.WST_Type.StartsWith(Prefix)
                           select new { label = wst.WST_Type, val = wst.WST_Type }).Take(10).ToList();
            return Json(wstname);
        }

        // /WorkingStandardTypeSet/GetDepartment/
        public JsonResult GetDepartment()
        {
            var select_departments = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                                .Select(s => new { label = s.Dep_Name, val = s.Dep_Id }).ToList();
            return Json(select_departments, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardTypeSet/GetWSType/
        [HttpPost]
        public JsonResult GetWSType(int department)
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
