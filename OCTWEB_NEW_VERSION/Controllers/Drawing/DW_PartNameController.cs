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

namespace OCTWEB_NET45.Controllers.Drawing
{
    [Authorize]
    public class DW_PartNameController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        //
        // GET: /DW_PartName/PartNameLit
        [CustomAuthorize(14)]//Part Name
        public ActionResult PartNameList(PartNameListModel model)
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

                string partname = model.partname;
                IEnumerable<DW_PartName> query = model.Page.HasValue ? db.DW_PartName : db.DW_PartName;

                if (!String.IsNullOrEmpty(partname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PAR_Name) && x.PAR_Name.ToLowerInvariant().Contains(partname.ToLowerInvariant()));
                }

                var pn_list = query.Select(x => new PartNameModel
                    {
                        id = x.PAR_ID,
                        name = x.PAR_Name,
                    }).OrderBy(o=>o.name).ToList();

                //IPagedList<PartNameModel> partNamePagedList = pn_list.ToPagedList(pageIndex, pageSize);

                model.PartNameList = pn_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_PartName/PartNameList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: /DW_PartName/PartNameList
        [HttpPost]
        [CustomAuthorize(14)]//Part Name
        public ActionResult PartNameList(FormCollection form,PartNameListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                string partname = model.partname;
                IEnumerable<DW_PartName> query = db.DW_PartName;

                if (!String.IsNullOrEmpty(partname))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PAR_Name) && x.PAR_Name.ToLowerInvariant().Contains(partname.ToLowerInvariant()));
                }

                var pn_list = query.Select(x => new PartNameModel
                {
                    id = x.PAR_ID,
                    name = x.PAR_Name,
                }).ToList();

                if(form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(pn_list);
                }

                //IPagedList<PartNameModel> partNamePagedList = pn_list.ToPagedList(pageIndex, pageSize);

                model.PartNameList = pn_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_PartName/PartNameList {0}", ex.ToString());
                return View("Error");
            }
        }

       //
       // GET: /DW_PartName/Create
        [CustomAuthorize(14)]//Part Name
        public ActionResult Create()
        {
            try
            {
                PartNameModel model = new PartNameModel();
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_PartName/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /DW_PartName/Create
        [HttpPost]
        [CustomAuthorize(14)]//Part Name
        public ActionResult Create(FormCollection form, PartNameModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var check = db.DW_PartName.Where(x => x.PAR_Name == model.name).FirstOrDefault();
                    if(check != null)
                    {
                        ViewBag.Message = "The part name is invalid, Duplicate part names.";
                        return View(model);
                    }
                    DW_PartName pn = new DW_PartName();
                    pn.PAR_Name = model.name;
                    //Add part name
                    var result = db.DW_PartName.Add(pn);
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Part Name",
                        Log_System = "Drawing",
                        Log_Detail = result.PAR_Name,
                        Log_Action_Id = result.PAR_ID,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save Change
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully created, {0}.", model.name);
                    return RedirectToAction("PartNameList");
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_PartName/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // GET: /DW_PartName/Edit
        [CustomAuthorize(14)]//Part Name
        public ActionResult Edit(int id)
        {
            try
            {
                PartNameModel model = db.DW_PartName.Where(x => x.PAR_ID == id)
                    .Select(s => new PartNameModel 
                    {
                        id = s.PAR_ID,
                        name = s.PAR_Name,
                    }).FirstOrDefault();

                return View(model);

            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_PartName/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /DW_PartName/Edit
        [HttpPost]
        [CustomAuthorize(14)]//Part Name
        public ActionResult Edit(FormCollection form,PartNameModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    DW_PartName pn = db.DW_PartName.Where(x => x.PAR_ID == model.id).FirstOrDefault();
                    if (pn != null)
                    {
                        pn.PAR_Name = model.name;
                        var check = db.DW_PartName.Where(x => x.PAR_Name == model.name && x.PAR_ID != pn.PAR_ID).FirstOrDefault();
                        if (check != null)
                        {
                            ViewBag.Message = "The part name is invalid, Duplicate part names.";
                            return View(model);
                        }
                        //Edit
                        db.Entry(pn).State = System.Data.Entity.EntityState.Modified;
                        //Save log
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "edit",
                            Log_Type = "Part Name",
                            Log_System = "Drawing",
                            Log_Detail = pn.PAR_Name,
                            Log_Action_Id = pn.PAR_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Save
                        db.SaveChanges();
                        TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.name);
                        return RedirectToAction("PartNameList");
                    }      
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_PartName/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //POS: /DW_PartName/Delete
        [CustomAuthorize(14)]//Part Name
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var pn_list = db.DW_PartName.Where(x => id_list.Contains(x.PAR_ID)).ToList();
                    foreach (var pn in pn_list)
                    {
                        var check_engineer_drawing = db.DW_Model.Where(x => x.PAR_ID == pn.PAR_ID).FirstOrDefault();
                        if (check_engineer_drawing != null)
                        {
                            TempData["shortError"] = String.Format("Can not delete, {0} (Engineer Drawing)", pn.PAR_Name);
                            return RedirectToAction("PartNameList");
                        }
                        var check_process_drawing = db.DW_ModelProcess.Where(x => x.PAR_ID == pn.PAR_ID).FirstOrDefault();
                        if (check_process_drawing != null)
                        {
                            TempData["shortError"] = String.Format("Can not delete, {0} (Process Drawing)", pn.PAR_Name);
                            return RedirectToAction("PartNameList");
                        }
                        var check_process = db.DW_Process.Where(x => x.PAR_ID == pn.PAR_ID).FirstOrDefault();
                        if (check_process_drawing != null)
                        {
                            TempData["shortError"] = String.Format("Can not delete, {0} (Process)", pn.PAR_Name);
                            return RedirectToAction("PartNameList");
                        }
                        //Remove
                        db.DW_PartName.Remove(pn);
                        //Save log
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Part Name",
                            Log_System = "Drawing",
                            Log_Detail = pn.PAR_Name,
                            Log_Action_Id = pn.PAR_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }
                    if(pn_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items.", id_list.Count());
                    return RedirectToAction("PartNameList");
                }
                return RedirectToAction("PartNameList");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_PartName/Delete {0}", ex.ToString());
                return View("Error");
            }
        }
        
        public void ExportToCsv(List<PartNameModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    name = "\"" + v.name + "\"",
                });

                sb.AppendFormat("{0},{1},{2}",
                    "Items", "Part Name", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2}",
                        item.item, item.name, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=PartName.CSV ");
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
        // POST: /DW_PartName/GetPartName/
        [HttpPost]
        public JsonResult GetPartName(string Prefix)
        {
            var partname = db.DW_PartName
                .Where(x => x.PAR_Name.StartsWith(Prefix))
                .Select(s=>s.PAR_Name)
                .Distinct()
                .Take(20)
                .Select(s => new { label = s, val = s }).ToList();
            return Json(partname);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

	}
}