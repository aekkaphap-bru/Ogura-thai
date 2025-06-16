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

namespace OCTWEB_NET45.Controllers.Drawing
{
    [Authorize]
    public class DW_ProcessDrawingSetupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_dw_processDrawing = ConfigurationManager.AppSettings["path_dw_processDrawing"];

        //
        // GET: /DW_ProcessDrawingSetup/PDList
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult PDList(ProcessDrawingListModel model)
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

                int? groupLine_id = model.groupLine_id;
                string partName = model.partName;
                string lineName = model.lineName;
                string drawingNo = model.drawingNo;
                string modelNo = model.modelNo;

                IEnumerable<DW_ModelProcess> query = model.Page.HasValue ? db.DW_ModelProcess : db.DW_ModelProcess.Take(0);
                if(groupLine_id.HasValue)
                {
                    List<int> process_id = db.DW_Process.Where(x => x.PAR_ID == groupLine_id).Select(s => s.PRO_ID).ToList();
                    query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value)); 
                }
                if(!String.IsNullOrEmpty(partName))
                {
                    List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name == partName)
                        .Select(s => s.PAR_ID).ToList();
                    query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                }
                if(!String.IsNullOrEmpty(lineName))
                {
                    List<int> process_id = db.DW_Process.Where(x => x.PRO_Name.Contains(lineName))
                        .Select(s => s.PRO_ID).ToList();
                    query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value));
                }
                if (!String.IsNullOrEmpty(drawingNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOP_DrawingNumber) && x.MOP_DrawingNumber.ToLowerInvariant().Contains(drawingNo.ToLowerInvariant()));
                }
                if(!String.IsNullOrEmpty(modelNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOP_Name) && x.MOP_Name.ToLowerInvariant().Contains(modelNo.ToLowerInvariant()));
                }

                var pd_list = query.Select(s => new ProcessDrawingModel
                        {
                            id = s.MOP_ID,
                            drawingNumber = s.MOP_DrawingNumber,
                            rev = s.MOP_Rev,
                            modelNo = s.MOP_Name,
                            file = s.MOP_FilePath,
                            remark = s.MOP_Note1,
                            _hide = s.MOP_Hide,
                            _lock = s.MOP_Lock,
                            process_id = s.PRO_ID,
                            partName_id = s.PAR_ID,
                            groupLine = (from p in db.DW_Process
                                         join pn in db.DW_PartName on p.PAR_ID equals pn.PAR_ID
                                         where p.PRO_ID == s.PRO_ID
                                         select pn.PAR_Name).FirstOrDefault(),
                            lineName = (from p in db.DW_Process
                                        where p.PRO_ID == s.PRO_ID
                                        select p.PRO_Name).FirstOrDefault(),
                            partName = (from pn in db.DW_PartName
                                        where pn.PAR_ID == s.PAR_ID
                                        select pn.PAR_Name).FirstOrDefault(),
                        }).OrderBy(o => o.groupLine).ToList();
                //IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);

                //Select Group Line Name(Part Name)
                List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.SelectGroupLine = selectPartName;
                model.ProcessDrawingList = pd_list;

                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_ProcessDrawingSetup/PDList {0}", ex.ToString());
                return View("Error");
            }
            
        }
        //
        // POST: /DW_ProcessDrawingSetup/PDList
        [HttpPost]
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult PDList(FormCollection form,ProcessDrawingListModel model)
        {
            try
            {
                
                int pageSize = 30;
                int pageIndex = 1;
                
                int? groupLine_id = model.groupLine_id;
                string partName = model.partName;
                string lineName = model.lineName;
                string drawingNo = model.drawingNo;
                string modelNo = model.modelNo;

                IEnumerable<DW_ModelProcess> query = db.DW_ModelProcess;
                if (groupLine_id.HasValue)
                {
                    List<int> process_id = db.DW_Process.Where(x => x.PAR_ID == groupLine_id).Select(s => s.PRO_ID).ToList();
                    query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value));
                }
                if (!String.IsNullOrEmpty(partName))
                {
                    List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name == partName)
                        .Select(s => s.PAR_ID).ToList();
                    query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                }
                if (!String.IsNullOrEmpty(lineName))
                {
                    List<int> process_id = db.DW_Process.Where(x => x.PRO_Name.Contains(lineName))
                        .Select(s => s.PRO_ID).ToList();
                    query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value));
                }
                if (!String.IsNullOrEmpty(drawingNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOP_DrawingNumber) && x.MOP_DrawingNumber.ToLowerInvariant().Contains(drawingNo.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(modelNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOP_Name) && x.MOP_Name.ToLowerInvariant().Contains(modelNo.ToLowerInvariant()));
                }

                var pd_list = query.Select(s => new ProcessDrawingModel
                {
                    id = s.MOP_ID,
                    drawingNumber = s.MOP_DrawingNumber,
                    rev = s.MOP_Rev,
                    modelNo = s.MOP_Name,
                    file = s.MOP_FilePath,
                    remark = s.MOP_Note1,
                    _hide = s.MOP_Hide,
                    _lock = s.MOP_Lock,
                    process_id = s.PRO_ID,
                    partName_id = s.PAR_ID,
                    groupLine = (from p in db.DW_Process
                                 join pn in db.DW_PartName on p.PAR_ID equals pn.PAR_ID
                                 where p.PRO_ID == s.PRO_ID
                                 select pn.PAR_Name).FirstOrDefault(),
                    lineName = (from p in db.DW_Process
                                where p.PRO_ID == s.PRO_ID
                                select p.PRO_Name).FirstOrDefault(),
                    partName = (from pn in db.DW_PartName
                                where pn.PAR_ID == s.PAR_ID
                                select pn.PAR_Name).FirstOrDefault(),
                }).OrderBy(o=>o.groupLine).ToList();

                if(form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(pd_list);
                }

                //IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);

                //Select Group Line Name(Part Name)
                List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.SelectGroupLine = selectPartName;
                model.ProcessDrawingList = pd_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_ProcessDrawingSetup/PDList {0}", ex.ToString());
                return View("Error");
            }

        }

        //
        // GET: //DW_ProcessDrawingSetup/Create
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult Create()
        {
            try
            {
                ProcessDrawingModel model = new ProcessDrawingModel();
                model.rev = "00";
                GetSelectOption(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_ProcessDrawingSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }
       
        //
        // POST: //DW_ProcessDrawingSetup/Create
        [HttpPost]
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult Create(HttpPostedFileBase file, FormCollection form, ProcessDrawingModel model)
        {
            try
            {
                GetSelectOption(model);

                if (ModelState.IsValid)
                {
                    var check = db.DW_ModelProcess.Where(x => x.MOP_DrawingNumber == model.drawingNumber
                        && x.MOP_Name == model.modelNo && x.PRO_ID == model.process_id && x.PAR_ID == model.partName_id).FirstOrDefault();
                    if(check != null)
                    {
                        ViewBag.Message = "The Process Drawing is invalid, Duplicate Process Drawing setting values.";
                        return View(model);
                    }
                    if (file != null)
                    {
                        var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };

                        string drawingNo = model.drawingNumber;
                        string processName = db.DW_Process.Where(x => x.PRO_ID == model.process_id).Select(s => s.PRO_Name).FirstOrDefault();
                        //Concat filename temp
                        string fileName_temp = String.Concat(drawingNo, "-", processName);
                        fileName_temp = fileName_temp.Length <= 40 ? fileName_temp : fileName_temp.Substring(0, 40);
                        //Concat filename
                        string _FileName = String.Concat(fileName_temp, Path.GetExtension(file.FileName));
                        string _path = Path.Combine(path_dw_processDrawing, _FileName);

                        var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                        if (!supportedTypes.Contains(fileExt))
                        {
                            ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                            return View(model);
                        }
                        if (System.IO.File.Exists(_path))
                        {
                            ViewBag.Message = String.Format("The {0} file already exists.", _FileName);
                            return View(model);
                        }

                        DW_ModelProcess mp = new DW_ModelProcess();
                        mp.MOP_DrawingNumber = model.drawingNumber;
                        mp.MOP_Rev = model.rev;
                        mp.MOP_Name = model.modelNo;
                        mp.MOP_Note1 = model.remark;
                        mp.PRO_ID = model.process_id;
                        mp.PAR_ID = model.partName_id;
                        mp.MOP_FilePath = _FileName;
                        //Add
                        var result = db.DW_ModelProcess.Add(mp);

                        //Save log
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("DrawingNo:", result.MOP_DrawingNumber
                                            , "/Rev:", result.MOP_Rev
                                            , "/ModelNo:", result.MOP_Name
                                            , "/PRO_ID:",result.PRO_ID
                                            , "/PAR_ID:",result.PAR_ID);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Process Drawing",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = result.MOP_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Save 
                        db.SaveChanges();
                        //Save file
                        file.SaveAs(_path);

                        TempData["shortMessage"] = String.Format("Successfully created, {0}", model.drawingNumber);
                        return RedirectToAction("PDList"
                            , new ProcessDrawingListModel {drawingNo=model.drawingNumber,Page=1 });
                    }
                    else
                    {
                        ViewBag.Message = String.Format("Upload file is required.");
                        return View(model);
                    }

                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_ProcessDrawingSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET : //DW_ProcessDrawingSetup/Edit
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult Edit(int id)
        {
            try
            {
                ProcessDrawingModel model = db.DW_ModelProcess.Where(x => x.MOP_ID == id)
                    .Select(s => new ProcessDrawingModel 
                    {
                        id = s.MOP_ID,
                        drawingNumber = s.MOP_DrawingNumber,
                        rev = s.MOP_Rev,
                        modelNo = s.MOP_Name,
                        file = s.MOP_FilePath,
                        remark = s.MOP_Note1,
                        process_id = s.PRO_ID,
                        partName_id = s.PAR_ID,
                        groupLine_id = (from p in db.DW_Process 
                                        where p.PRO_ID == s.PRO_ID
                                        select p.PAR_ID).FirstOrDefault(),

                    }).FirstOrDefault();

                GetSelectOption(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_ProcessDrawingSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: //DW_ProcessDrawingSetup/Edit
        [HttpPost]
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult Edit(HttpPostedFileBase file, FormCollection form, ProcessDrawingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DW_ModelProcess pd = db.DW_ModelProcess.Where(x => x.MOP_ID == model.id).FirstOrDefault();
                    if (pd != null)
                    {
                        string file_old = pd.MOP_FilePath;
                        string drawingNo_old = pd.MOP_DrawingNumber;
                        int? process_id_old = pd.PRO_ID; 

                        //pd.MOP_DrawingNumber = model.drawingNumber; //Readonly
                        pd.MOP_Rev = model.rev;
                        pd.MOP_Name = model.modelNo;
                        pd.MOP_Note1 = model.remark;
                        pd.PRO_ID = model.process_id;
                        pd.PAR_ID = model.partName_id;

                        //Concat filename temp
                        string drawingNo = pd.MOP_DrawingNumber;
                        string processName = db.DW_Process.Where(x => x.PRO_ID == model.process_id).Select(s => s.PRO_Name).FirstOrDefault();
                        string fileName_temp = String.Concat(drawingNo, "-", processName);
                        fileName_temp = fileName_temp.Replace(" ", "");
                        fileName_temp = fileName_temp.Length <= 40 ? fileName_temp : fileName_temp.Substring(0, 40);

                        if (file != null)
                        {
                            var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };

                            //Concat filename
                            string _FileName = String.Concat(fileName_temp, Path.GetExtension(file.FileName));
                            string _path = Path.Combine(path_dw_processDrawing, _FileName);

                            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                                return View(model);
                            }
                            //Delete old file
                            if(file_old != _FileName)
                            {
                                string _path_old = Path.Combine(path_dw_processDrawing, file_old);
                                if(System.IO.File.Exists(_path_old))
                                {
                                    System.IO.File.Delete(_path_old);
                                }
                            }
                            //Save New file
                            pd.MOP_FilePath = _FileName;
                            file.SaveAs(_path);  
                        }
                        else //Case change part name and no upload file
                        { 
                            if (process_id_old != model.process_id)
                            {
                                //Get old path
                                string _path_old = Path.Combine(path_dw_processDrawing, file_old);
                                System.IO.FileInfo fi = new System.IO.FileInfo(_path_old);
                                if (fi.Exists)
                                {
                                    var ext_old = fi.Extension;
                                    string _FileName = String.Concat(fileName_temp, ext_old);
                                    string _path = Path.Combine(path_dw_processDrawing, _FileName);
                                    //Change file name
                                    fi.MoveTo(_path);
                                    pd.MOP_FilePath = _FileName;
                                }
                            }
                        }
                        //Save Edit
                        db.Entry(pd).State = System.Data.Entity.EntityState.Modified;

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("DrawingNo:", pd.MOP_DrawingNumber
                                           , "/Rev:", pd.MOP_Rev
                                           , "/ModelNo:", pd.MOP_Name
                                           , "/PRO_ID:", pd.PRO_ID
                                           , "/PAR_ID:", pd.PAR_ID);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "edit",
                            Log_Type = "Process Drawing",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = pd.MOP_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Save
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.drawingNumber);
                        return RedirectToAction("PDList"
                            , new ProcessDrawingListModel { drawingNo = model.drawingNumber, Page = 1 });
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_ProcessDrawingSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: DW_ProcessDrawingSetup/Delete
        [CustomAuthorize(16)]//Process Drawing
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var pd_list = db.DW_ModelProcess.Where(x=> id_list.Contains(x.MOP_ID)).ToList();
                    foreach(var pd in pd_list)
                    {
                        //Remove
                        db.DW_ModelProcess.Remove(pd);

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("DrawingNo:", pd.MOP_DrawingNumber
                                           , "/Rev:", pd.MOP_Rev
                                           , "/ModelNo:", pd.MOP_Name
                                           , "/PRO_ID:", pd.PRO_ID
                                           , "/PAR_ID:", pd.PAR_ID);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Process Drawing",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = pd.MOP_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Delete file
                        string _FileName = pd.MOP_FilePath;
                        if (!String.IsNullOrEmpty(_FileName))
                        {
                            string _path = Path.Combine(path_dw_processDrawing, _FileName);
                            if (System.IO.File.Exists(_path))
                            {
                                System.IO.File.Delete(_path);
                            }
                        }                      
                    }
                    if(pd_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return RedirectToAction("PDList");
                }
                return RedirectToAction("PDList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //GetSelectOption
        public ProcessDrawingModel GetSelectOption(ProcessDrawingModel model)
        {
            int? groupLine_id = model.groupLine_id;

            //Select Group Line(Process/groupLineName(PartName))
            List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
            List<SelectListItem> selectGroupLine = db.DW_PartName
                .Where(x => select_partname_id_list.Contains(x.PAR_ID)).OrderBy(o => o.PAR_Name)
                .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

            //Select Line Name
            IEnumerable<DW_Process> query_ln = groupLine_id.HasValue ? 
                db.DW_Process.Where(x => x.PAR_ID == groupLine_id).OrderBy(o=>o.PRO_Name) : db.DW_Process.Take(0);
            List<SelectListItem> selectLineName = query_ln
                .Select(s => new SelectListItem { Value = s.PRO_ID.ToString(), Text = s.PRO_Name }).ToList();

            //Select Part Name
            List<SelectListItem> selectPartName = db.DW_PartName.OrderBy(o => o.PAR_Name)
                .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

            model.SelectGroupLine = selectGroupLine;
            model.SelectLineName = selectLineName;
            model.SelectPartName = selectPartName;

            return model;
        }

        [CustomAuthorize(16)]//Process Drawing
        public ActionResult DownloadFile(string fileName)
        {
            string path = path_dw_processDrawing + fileName;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);

                    return File(bytes, "application/octet-stream", fileName);
                }
                ViewBag.errorMessage = String.Format("Could not find file.");
                return View("Error");
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                return View("Error");
            }
        }

        public void ExportToCsv(List<ProcessDrawingModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    drawingNumber = "\"" + v.drawingNumber + "\"",
                    rev = v.rev,
                    modelno = v.modelNo,
                    remark = "\"" + v.remark + "\"",
                    groupLine = "\"" + v.groupLine + "\"",
                    lineName = "\"" + v.lineName + "\"",
                    partName = "\"" + v.partName + "\"",
                });
                
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    "Items", "Groupline Name", "Line Name", "Drawing No."
                    , "Revision", "Part Name", "Model No.", "Remark", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        item.item, item.groupLine, item.lineName, item.drawingNumber
                        ,item.rev, item.partName, item.modelno, item.remark, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=ProcessDrawing.CSV ");
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

        // POST: /DW_ProcessDrawingSetup/GetPartName
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
        // POST: /DW_ProcessDrawingSetup/GetLineName
        [HttpPost]
        public JsonResult GetLineName(string Prefix)
        {
            var linename = db.DW_Process
                            .Where(x => x.PRO_Name.StartsWith(Prefix))
                            .Select(s=>s.PRO_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(linename);
        }
        // POST: /DW_ProcessDrawingSetup/GetDrawingNo
        [HttpPost]
        public JsonResult GetDrawingNo(string Prefix)
        {
            var drawingno = db.DW_ModelProcess
                            .Where(x => x.MOP_DrawingNumber.StartsWith(Prefix))
                            .Select(s=>s.MOP_DrawingNumber)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(drawingno);
        }
        // POST: /DW_ProcessDrawingSetup/GetModelNo
        [HttpPost]
        public JsonResult GetModelNo(string Prefix)
        {
            var modelno = db.DW_ModelProcess
                            .Where(x => x.MOP_Name.StartsWith(Prefix))
                            .Select(s=>s.MOP_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelno);
        }

        // /DW_ProcessDrawingSetup/GetSelectLineName
        public JsonResult GetSelectLineName(int groupLine_id)
        {
            var select_ln = db.DW_Process.Where(x => x.PAR_ID == groupLine_id)
                .OrderBy(o=>o.PRO_Name)
                .Select(s => new { label = s.PRO_Name, val = s.PRO_ID }).ToList();

            return Json(select_ln, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}