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
    public class DW_ProcessSetupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        //
        // GET: /DW_ProcessSetup/ProcessList
        [CustomAuthorize(13)]//Process
        public ActionResult ProcessList(ProcessListModel model)
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

                string search_processCode = model.search_processCode;
                string search_processNo = model.search_processNo;
                int? search_partName_id = model.search_partName_id;

                IEnumerable<DW_Process> query = model.Page.HasValue ? db.DW_Process : db.DW_Process;
                if (!String.IsNullOrEmpty(search_processCode))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PRO_Name) && x.PRO_Name.ToLowerInvariant().Contains(search_processCode)).ToList();
                }
                if(!String.IsNullOrEmpty(search_processNo))
                {
                    query = query.Where(x => x.PRO_Sequence.ToString() == search_processNo).ToList();
                }
                if(search_partName_id.HasValue)
                {
                    query = query.Where(x => x.PAR_ID == search_partName_id).ToList();
                }
                List<ProcessModel> pc_list = query
                    .Select(s => new ProcessModel
                    {
                        id = s.PRO_ID,
                        processCode = s.PRO_Name,
                        processNo = s.PRO_Sequence,
                        partName_id = s.PAR_ID,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                    }).OrderBy(o=>o.partName).ToList();

                IPagedList<ProcessModel> processPagedList = pc_list.ToPagedList(pageIndex, pageSize);

                //Select Part Name
                List<int> select_partname_id_list = db.DW_Process.Select(s=>s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.ProcessList = pc_list;
                model.SelectPartName = selectPartName;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_ProcessSetup/processList {0}", ex.ToString());
                return View("Error");
            }          
        }

        //
        // POST: /DW_ProcessSetup/ProcessList
        [HttpPost]
        [CustomAuthorize(13)]//Process
        public ActionResult ProcessList(FormCollection form,ProcessListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                string search_processCode = model.search_processCode;
                string search_processNo = model.search_processNo;
                int? search_partName_id = model.search_partName_id;

                IEnumerable<DW_Process> query = db.DW_Process;
                if (!String.IsNullOrEmpty(search_processCode))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.PRO_Name) && x.PRO_Name.ToLowerInvariant().Contains(search_processCode)).ToList();
                }
                if (!String.IsNullOrEmpty(search_processNo))
                {
                    query = query.Where(x => x.PRO_Sequence.ToString() == search_processNo).ToList();
                }
                if (search_partName_id.HasValue)
                {
                    query = query.Where(x => x.PAR_ID == search_partName_id).ToList();
                }
                List<ProcessModel> pc_list = query
                    .Select(s => new ProcessModel
                    {
                        id = s.PRO_ID,
                        processCode = s.PRO_Name,
                        processNo = s.PRO_Sequence,
                        partName_id = s.PAR_ID,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                    }).OrderBy(o => o.partName).ToList();

                if(form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(pc_list);
                }

                IPagedList<ProcessModel> processPagedList = pc_list.ToPagedList(pageIndex, pageSize);

                //Select Part Name
                List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.ProcessList = pc_list;
                model.SelectPartName = selectPartName;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //DW_ProcessSetup/processList {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // GET: /DW_ProcessSetup/Create
        [CustomAuthorize(13)]//Process
        public ActionResult Create()
        {
            try
            {
                ProcessModel model = new ProcessModel();
                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName
                    .OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();
                
                model.SelectPartName = selectPartName;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_ProcessSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        // POST: /DW_ProcessSetup/Create
        [HttpPost]
        [CustomAuthorize(13)]//Process
        public ActionResult Create(ProcessModel model)
        {
            try
            {
                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName
                    .OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.SelectPartName = selectPartName;

                if(ModelState.IsValid)
                {
                    var check = db.DW_Process.Where(x => x.PRO_Name == model.processCode
                        && x.PRO_Sequence == model.processNo && x.PAR_ID == model.partName_id).FirstOrDefault();
                    if(check != null)
                    {
                        ViewBag.Message = "The process is invalid, Duplicate process names.";
                        return View(model);
                    }

                    DW_Process pc = new DW_Process();
                    pc.PRO_Name = model.processCode;
                    pc.PRO_Sequence = model.processNo;
                    pc.PAR_ID = model.partName_id;
                    //Add
                    var result = db.DW_Process.Add(pc);
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("ProcessCode:", result.PRO_Name
                                        , "/ProcessNo:", result.PRO_Sequence
                                        , "/PartNameId:", result.PAR_ID.ToString());
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Process",
                        Log_System = "Drawing",
                        Log_Detail = log_detail,
                        Log_Action_Id = result.PRO_ID,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save 
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully created, {0}.", model.processCode);
                    return RedirectToAction("ProcessList");
                }
               
                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_ProcessSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // GET: /DW_ProcessSetup/Edit
        [CustomAuthorize(13)]//Process
        public ActionResult Edit(int id)
        {
            try
            {
                ProcessModel model = db.DW_Process.Where(x => x.PRO_ID == id)
                    .Select(s => new ProcessModel 
                    {
                         id = s.PRO_ID,
                         processCode = s.PRO_Name,
                         processNo = s.PRO_Sequence,
                         partName_id = s.PAR_ID
                    }).FirstOrDefault();

                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName
                    .OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.SelectPartName = selectPartName;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_ProcessSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        // POST: /DW_ProcessSetup/Edit
        [HttpPost]
        [CustomAuthorize(13)]//Process
        public ActionResult Edit(ProcessModel model)
        {
            try
            {
                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName
                    .OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.SelectPartName = selectPartName;

                if(ModelState.IsValid)
                {
                    var check = db.DW_Process.Where(x => x.PRO_ID != model.id && x.PRO_Name == model.processCode 
                        && x.PRO_Sequence == model.processNo && x.PAR_ID == model.partName_id).FirstOrDefault();
                    if (check != null)
                    {
                        ViewBag.Message = "The process is invalid, Duplicate process names.";
                        return View(model);
                    }

                    DW_Process pn = db.DW_Process.Where(x => x.PRO_ID == model.id).FirstOrDefault();
                    pn.PRO_Name = model.processCode;
                    pn.PRO_Sequence = model.processNo;
                    pn.PAR_ID = model.partName_id;
                    //Edit
                    db.Entry(pn).State = System.Data.Entity.EntityState.Modified;
                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("ProcessCode:", model.processCode
                                       , "/ProcessNo:", model.processNo
                                       , "/PartNameId:", model.partName_id.ToString());
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "edit",
                        Log_Type = "Process",
                        Log_System = "Drawing",
                        Log_Detail = log_detail,
                        Log_Action_Id = pn.PAR_ID,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    //Save
                    db.SaveChanges();
                    TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.processCode);
                    return RedirectToAction("ProcessList");                       
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_ProcessSetup/Edit {0}", ex.ToString());
                return View("Error");
            }

        }

        //
        // POST: /DW_ProcessSetup/Delete
        [CustomAuthorize(13)]//Process
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var pc_list = db.DW_Process.Where(x=> id_list.Contains(x.PRO_ID)).ToList();
                    foreach(var pc in pc_list)
                    {
                        var check_process_drawing = db.DW_ModelProcess.Where(x => x.PRO_ID == pc.PRO_ID).FirstOrDefault();
                        if(check_process_drawing != null)
                        {
                            TempData["shortError"] = String.Format("Can not delete, {0} (Process Drawing)", pc.PRO_Name);
                            return RedirectToAction("ProcessList");
                        }
                        //Remove
                        db.DW_Process.Remove(pc);
                        //Save log
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("ProcessCode:", pc.PRO_Name
                                       , "/ProcessNo:", pc.PRO_Sequence
                                       , "/PartNameId:", pc.PAR_ID.ToString());
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Process",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = pc.PRO_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }
                    if(pc_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items.", id_list.Count());
                    return RedirectToAction("ProcessList");
                }
                return RedirectToAction("ProcessList");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_ProcessSetup/Delete {0}", ex.ToString());
                return View("Error");
            }
        }


        public void ExportToCsv(List<ProcessModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    processCode = "\"" + v.processCode + "\"",
                    processNo = v.processNo,
                    partName =  "\"" + v.partName + "\"",
                });

                sb.AppendFormat("{0},{1},{2},{3},{4}",
                    "Items", "Process Code", "Process Number", "Group Line Name",Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4}",
                        item.item, item.processCode,item.processNo,item.partName, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=Process.CSV ");
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

        // POST: /DW_ProcessSetup/GetProcessCode
        [HttpPost]
        public JsonResult GetProcessCode(string Prefix)
        {
            var procode = db.DW_Process
                        .Where(x => x.PRO_Name.StartsWith(Prefix))
                        .Select(s=>s.PRO_Name)
                        .Distinct()
                        .Take(20)
                        .Select(s => new { label = s, val = s }).ToList();
            return Json(procode);
        }

        // POST: /DW_ProcessSetup/GetProcessNo
        [HttpPost]
        public JsonResult GetProcessNo(int Prefix)
        {
            var prono = db.DW_Process
                        .Where(x => x.PRO_Sequence == Prefix)
                        .Select(s=>s.PRO_Sequence)
                        .Distinct()
                        .Take(20)
                        .Select(s => new { label = s, val = s }).ToList();
            return Json(prono);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}