using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections;
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
    public class DW_EngineerDrawingSetupController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_dw_engineerDrawing = ConfigurationManager.AppSettings["path_dw_engineerDrawing"];
        private string path_dw_engineerPartList = ConfigurationManager.AppSettings["path_dw_engineerPartList"];

        //
        // GET: /DW_EngineerDrawingSetup/EDList
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult EDList(EngineerDrawingListModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                if (TempData["errorMessage"] != null)
                {
                    ViewBag.errorMessage = TempData["errorMessage"].ToString();
                }
                if (TempData["errorDeleteMessage"] != null)
                {
                    ViewBag.errorDeleteMessage = TempData["errorDeleteMessage"];
                }
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                string partName = model.partName;
                string drawingNo = model.drawingNo;
                string modelNo = model.modelNo;
                int sortby_id = model.sortby_id;

                IEnumerable<DW_Model> query = model.Page.HasValue ? db.DW_Model : db.DW_Model.Take(0);
                if (!String.IsNullOrEmpty(partName))
                {
                    List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name == partName)
                        .Select(s => s.PAR_ID).ToList();
                    query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                }
                if (!String.IsNullOrEmpty(drawingNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOD_DrawingNumber) && x.MOD_DrawingNumber.ToLowerInvariant().Contains(drawingNo.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(modelNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Name) && x.MOD_Name.ToLowerInvariant().Contains(modelNo.ToLowerInvariant()));
                }

                var ed_list = query.Select(s => new EngineerDrawingModel
                {
                    id = s.MOD_ID,
                    level = s.MOD_Level,
                    drawingNo = s.MOD_DrawingNumber,
                    rev = s.MOD_Rev,
                    modelNo = s.MOD_Name,
                    semiAssemblyNo = s.MOD_SemiAssemblyNo,
                    customerNo = s.MOD_CustomerNo,
                    drawing_file = s.MOD_FilePath,
                    partList_file = s.MOD_PartListFilePath,
                    note = s.MOD_Note,
                    _hide = s.MOD_Hide.HasValue ? s.MOD_Hide.Value > 0 ? true : false : false,
                    _lock = s.MOD_Lock.HasValue ? s.MOD_Lock.Value > 0 ? true : false : false,
                    partName_id = s.PAR_ID,
                    tempPartName = s.MOD_TempPartName,
                    partName = (from pn in db.DW_PartName
                                where pn.PAR_ID == s.PAR_ID
                                select pn.PAR_Name).FirstOrDefault(),
                    formulas_count = (from ef in db.DW_Formulas 
                                      where ef.FOR_Parent == s.MOD_ID
                                      select ef.FOR_Child).Count(),
                }).ToList();
                switch (sortby_id)
                {
                    case 1:
                        ed_list = ed_list.OrderBy(s => s.partName).ThenBy(o => o.modelNo).ToList();
                        break;
                    case 2:
                        ed_list = ed_list.OrderBy(s => s.modelNo).ThenBy(o => o.partName).ToList();
                        break;
                    default:
                        ed_list = ed_list.OrderBy(s => s.partName).ThenBy(o => o.modelNo).ToList();
                        break;
                }

                //IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);
               
                //Select Order by
                List<SelectListItem> selectSortby = new List<SelectListItem>();
                selectSortby.Add(new SelectListItem { Value = "1", Text = "Part Name, Model Number" });
                selectSortby.Add(new SelectListItem { Value = "2", Text = "Model Number, Part Name" });

                model.SelectSortBy = selectSortby;
                model.engineerDrawingList = ed_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_EngineerDrawingSetup/EDList {0}", ex.ToString());
                return View("Error");
            }          
        }

        //
        // POST: /DW_EngineerDrawingSetup/EDList
        [HttpPost]
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult EDList(FormCollection form,EngineerDrawingListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
              
                string partName = model.partName;
                string drawingNo = model.drawingNo;
                string modelNo = model.modelNo;
                int sortby_id = model.sortby_id;

                IEnumerable<DW_Model> query = db.DW_Model ;
                if (!String.IsNullOrEmpty(partName))
                {
                    List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name == partName)
                        .Select(s => s.PAR_ID).ToList();
                    query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                }
                if (!String.IsNullOrEmpty(drawingNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOD_DrawingNumber) && x.MOD_DrawingNumber.ToLowerInvariant().Contains(drawingNo.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(modelNo))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Name) && x.MOD_Name.ToLowerInvariant().Contains(modelNo.ToLowerInvariant()));
                }

                var ed_list = query.Select(s => new EngineerDrawingModel
                {
                    id = s.MOD_ID,
                    level = s.MOD_Level,
                    drawingNo = s.MOD_DrawingNumber,
                    rev = s.MOD_Rev,
                    modelNo = s.MOD_Name,
                    semiAssemblyNo = s.MOD_SemiAssemblyNo,
                    customerNo = s.MOD_CustomerNo,
                    drawing_file = s.MOD_FilePath,
                    partList_file = s.MOD_PartListFilePath,
                    note = s.MOD_Note,
                    _hide = s.MOD_Hide.HasValue ? s.MOD_Hide.Value > 0 ? true : false : false,
                    _lock = s.MOD_Lock.HasValue ? s.MOD_Lock.Value > 0 ? true : false : false,
                    partName_id = s.PAR_ID,
                    tempPartName = s.MOD_TempPartName,
                    partName = (from pn in db.DW_PartName
                                where pn.PAR_ID == s.PAR_ID
                                select pn.PAR_Name).FirstOrDefault(),
                    formulas_count = (from ef in db.DW_Formulas
                                      where ef.FOR_Parent == s.MOD_ID
                                      select ef.FOR_Child).Count(),
                }).ToList();
                switch (sortby_id)
                {
                    case 1:
                        ed_list = ed_list.OrderBy(s => s.partName).ThenBy(o=>o.modelNo).ToList();
                        break;
                    case 2:
                        ed_list = ed_list.OrderBy(s => s.modelNo).ThenBy(o => o.partName).ToList();
                        break;
                    default:
                        ed_list = ed_list.OrderBy(s => s.partName).ThenBy(o => o.modelNo).ToList();
                        break;
                }
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(ed_list);
                }

                //IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);

                //Select Order by
                List<SelectListItem> selectSortby = new List<SelectListItem>();
                selectSortby.Add(new SelectListItem { Value = "1", Text = "Part Name, Model Number" });
                selectSortby.Add(new SelectListItem { Value = "2", Text = "Model Number, Part Name" });

                model.SelectSortBy = selectSortby;
                model.engineerDrawingList = ed_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_EngineerDrawingSetup/EDList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: //DW_EngineerDrawingSetup/Create
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult Create()
        {
            try
            {
                EngineerDrawingModel model = new EngineerDrawingModel();

                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName.OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                model.selectPartName = selectPartName;
                model.rev = "00";

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_EngineerDrawingSetup/Create
        [HttpPost]
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult Create(HttpPostedFileBase drawing_file, HttpPostedFileBase partList_file, EngineerDrawingModel model)
        {
            try
            {
                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName.OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();
                model.selectPartName = selectPartName;

                if(ModelState.IsValid)
                {
                    var check = db.DW_Model.Where(x => x.MOD_Level == model.level && x.MOD_DrawingNumber == model.drawingNo
                        && x.MOD_Rev == model.rev && x.MOD_Name == model.modelNo && x.MOD_SemiAssemblyNo == model.semiAssemblyNo
                        && x.MOD_CustomerNo == model.customerNo).FirstOrDefault();
                    if(check != null)
                    {
                        ViewBag.Message = "The Engineer Drawing is invalid, Duplicate Engineer Drawing setting values.";
                        return View(model);
                    }
                    var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };
                    string drawingNo = model.drawingNo;

                    DW_Model ed = new DW_Model();
                    ed.MOD_Level = model.level;
                    ed.MOD_DrawingNumber = model.drawingNo;
                    ed.MOD_Rev = model.rev;
                    ed.MOD_Name = model.modelNo;
                    ed.MOD_SemiAssemblyNo = model.semiAssemblyNo;
                    ed.MOD_CustomerNo = model.customerNo;
                    ed.MOD_Note = model.note;
                    ed.MOD_Hide = model._hide ? 1 : 0;
                    ed.PAR_ID = model.partName_id;

                    if(drawing_file != null)
                    {                     
                        //Concat filename
                        string _FileName = String.Concat(drawingNo, Path.GetExtension(drawing_file.FileName));
                        string _path = Path.Combine(path_dw_engineerDrawing, _FileName);

                        var fileExt = System.IO.Path.GetExtension(drawing_file.FileName).Substring(1);

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
                        //Set Drawing Path file
                        ed.MOD_FilePath = _FileName;
                        //Save Drawing File
                        drawing_file.SaveAs(_path);
                    }
                    if (partList_file != null)
                    {
                        string rev = model.rev;
                        //Concat filename
                        string _FileName = String.Concat(drawingNo, rev, Path.GetExtension(partList_file.FileName));
                        string _path = Path.Combine(path_dw_engineerPartList, _FileName);

                        var fileExt = System.IO.Path.GetExtension(partList_file.FileName).Substring(1);

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
                        //Set PartList Path file
                        ed.MOD_PartListFilePath = _FileName;
                        //Save PartList File
                        partList_file.SaveAs(_path);
                    }
                    //Add
                    var result = db.DW_Model.Add(ed);

                    //Save log
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    string log_detail = String.Concat("DrawingNo:", result.MOD_DrawingNumber
                                        , "/Rev:", result.MOD_Rev
                                        , "/ModelNo:", result.MOD_Name
                                        , "/SemiAssemblyNo:", result.MOD_SemiAssemblyNo
                                        , "/CustomerNo:", result.MOD_CustomerNo
                                        , "/PAR_ID:", result.PAR_ID);
                    log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Engineer Drawing",
                        Log_System = "Drawing",
                        Log_Detail = log_detail,
                        Log_Action_Id = result.MOD_ID,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);

                    //Save
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully created, {0}", model.drawingNo);
                    return RedirectToAction("EDList"
                        , new EngineerDrawingListModel { drawingNo = model.drawingNo, Page = 1 });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_EngineerDrawingSetup/Create {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET : /DW_EngineerDrawingSetup/Edit
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult Edit(int id)
        {
            try
            {
                EngineerDrawingModel model = db.DW_Model.Where(x => x.MOD_ID == id)
                    .Select(s => new EngineerDrawingModel
                    {
                        id = s.MOD_ID,
                        level = s.MOD_Level,
                        drawingNo = s.MOD_DrawingNumber,
                        rev = s.MOD_Rev,
                        modelNo = s.MOD_Name,
                        semiAssemblyNo = s.MOD_SemiAssemblyNo,
                        customerNo = s.MOD_CustomerNo,
                        drawing_file = s.MOD_FilePath,
                        partList_file = s.MOD_PartListFilePath,
                        note = s.MOD_Note,
                        _hide = s.MOD_Hide.HasValue ? s.MOD_Hide.Value > 0 ? true : false : false,
                        partName_id = s.PAR_ID,
                    }).FirstOrDefault();

                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName.OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();
                model.selectPartName = selectPartName;

                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_EngineerDrawingSetup/Edit
        [HttpPost]
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult Edit(HttpPostedFileBase drawing_file, HttpPostedFileBase partList_file, EngineerDrawingModel model)
        {
            try
            {
                //Select Part Name
                List<SelectListItem> selectPartName = db.DW_PartName.OrderBy(o => o.PAR_Name)
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();
                model.selectPartName = selectPartName;

                if (ModelState.IsValid)
                {
                    DW_Model ed = db.DW_Model.Where(x => x.MOD_ID == model.id).FirstOrDefault();
                    if (ed != null)
                    {
                        string drawing_file_old = ed.MOD_FilePath;
                        string partList_file_old = ed.MOD_PartListFilePath;
                        string rev_old = ed.MOD_Rev;

                        ed.MOD_Level = model.level;
                        //ed.MOD_DrawingNumber = model.drawingNo; //Readonly
                        ed.MOD_Rev = model.rev;
                        ed.MOD_Name = model.modelNo;
                        ed.MOD_SemiAssemblyNo = model.semiAssemblyNo;
                        ed.MOD_CustomerNo = model.customerNo;
                        ed.MOD_Note = model.note;
                        ed.MOD_Hide = model._hide ? 1 : 0;
                        ed.PAR_ID = model.partName_id;

                        var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };
                        string drawingNo = ed.MOD_DrawingNumber;
                        //Drawing File
                        if (drawing_file != null)
                        {
                            //Concat filename
                            string _FileName = String.Concat(drawingNo, Path.GetExtension(drawing_file.FileName));
                            string _path = Path.Combine(path_dw_engineerDrawing, _FileName);

                            var fileExt = System.IO.Path.GetExtension(drawing_file.FileName).Substring(1);

                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                                return View(model);
                            }
                            //Save New file
                            ed.MOD_FilePath = _FileName;
                            drawing_file.SaveAs(_path);
                        }
                        //Part List File
                        if(partList_file != null)
                        {
                            //Concat filename
                            string _FileName = String.Concat(drawingNo,model.rev, Path.GetExtension(partList_file.FileName));
                            string _path = Path.Combine(path_dw_engineerPartList, _FileName);

                            var fileExt = System.IO.Path.GetExtension(partList_file.FileName).Substring(1);

                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, Only word, PDF, excel and text files.";
                                return View(model);
                            }
                            //Delete old file
                            if(partList_file_old != _FileName)
                            {
                                string _path_old = Path.Combine(path_dw_engineerPartList, partList_file_old);
                                if (System.IO.File.Exists(_path_old))
                                {
                                    System.IO.File.Delete(_path_old);
                                }
                            }
                            //Save New file
                            ed.MOD_PartListFilePath = _FileName;
                            partList_file.SaveAs(_path);
                        }
                        else //Case change revision and no upload file
                        {
                            if(rev_old != model.rev)
                            {
                                //Get old path
                                string _path_old = Path.Combine(path_dw_engineerPartList, partList_file_old);
                                System.IO.FileInfo fi = new System.IO.FileInfo(_path_old);
                                if (fi.Exists)
                                {
                                    var ext_old = fi.Extension;
                                    string _FileName = String.Concat(drawingNo,model.rev, ext_old);
                                    string _path = Path.Combine(path_dw_engineerPartList, _FileName);
                                    //Change file name
                                    fi.MoveTo(_path);
                                    ed.MOD_PartListFilePath = _FileName;
                                }
                            }
                        }

                        //Save Edit
                        db.Entry(ed).State = System.Data.Entity.EntityState.Modified;

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("DrawingNo:", ed.MOD_DrawingNumber
                                           , "/Rev:", ed.MOD_Rev
                                           , "/ModelNo:", ed.MOD_Name
                                           , "/SemiAssemblyNo:", ed.MOD_SemiAssemblyNo
                                           , "/CustomerNo:", ed.MOD_CustomerNo 
                                           , "/PAR_ID:", ed.PAR_ID);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "edit",
                            Log_Type = "Engineer Drawing",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = ed.MOD_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Save
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully edited, {0}", model.drawingNo);
                        return RedirectToAction("EDList"
                            , new ProcessDrawingListModel { drawingNo = model.drawingNo, Page = 1 });
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_EngineerDrawingSetup/Edit {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /DW_EngineerDrawingSetup/DefineFormulas
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult DefineFormulas(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                if (TempData["errorMessage"] != null)
                {
                    ViewBag.errorMessage = TempData["errorMessage"].ToString();
                }
                var model = db.DW_Model.Where(x => x.MOD_ID == id)
                    .Select(s => new FormulasModel
                    {
                        id_parent = s.MOD_ID,
                        drawing_parent = s.MOD_DrawingNumber,
                        model_parent = s.MOD_Name,
                        partName_parent = (from pn in db.DW_PartName
                                           where pn.PAR_ID == s.PAR_ID
                                           select pn.PAR_Name).FirstOrDefault(),
                    }).FirstOrDefault();
                //Child model List
                var child_model_list = db.DW_Formulas.Join(db.DW_Model, f=>f.FOR_Child, m=>m.MOD_ID, (f,m) => new {f = f, m=m})
                    .Where(x=> x.f.FOR_Parent == id)
                    .Select(s=>new FormulasChildModel
                    {
                        id = s.f.FOR_ID,
                        model = s.m.MOD_Name,
                        drawingNo = s.m.MOD_DrawingNumber,
                        partName_id = s.m.PAR_ID,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.m.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                        level = s.m.MOD_Level,
                        quantity = s.f.FOR_Qty,
                        unit = s.f.FOR_UnitName,
                        note = s.f.FOR_Note,
                    }).OrderBy(o=>o.id).ToList();

                model.ChildModelList = child_model_list;
                model.SelectChildModelList = new List<FormulasSelectChildModel>();
                
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/DefineFormulas {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_EngineerDrawingSetup/DefineFormulas
        [HttpPost]
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult DefineFormulas(FormCollection form,FormulasModel model)
        {
            try
            {             
                string searchDrawingNo_child = model.searchDrawingNo_child;
                string searchModelNo_child = model.searchModelNo_child;
                string searchPartName_child = model.searchPartName_child;
                //Child Select List
                IEnumerable<DW_Model> query = db.DW_Model;
                if(!String.IsNullOrEmpty(searchDrawingNo_child))
                {
                    query = query.Where(x => x.MOD_DrawingNumber.ToLowerInvariant().Contains(searchDrawingNo_child.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchModelNo_child))
                {
                    query = query.Where(x => x.MOD_Name.ToLowerInvariant().Contains(searchModelNo_child.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchPartName_child))
                {
                    List<int> partName_id_list = db.DW_PartName.Where(x => x.PAR_Name.Contains(searchPartName_child))
                        .Select(s => s.PAR_ID).ToList();
                    query = query.Where(x => x.PAR_ID.HasValue && partName_id_list.Contains(x.PAR_ID.Value));
                }
                var search_child_list = query.Select(s => new FormulasSelectChildModel
                {
                    id_child = s.MOD_ID,
                    model_child = s.MOD_Name,
                    drawingNo_child = s.MOD_DrawingNumber, 
                    partName_child = (from pn in db.DW_PartName
                                where pn.PAR_ID == s.PAR_ID
                                select pn.PAR_Name).FirstOrDefault(),
                    level_child = s.MOD_Level,
                  
                }).OrderBy(o=>o.model_child).ToList();

                //Child model List
                var child_model_list = db.DW_Formulas.Join(db.DW_Model, f => f.FOR_Child, m => m.MOD_ID, (f, m) => new { f = f, m = m })
                    .Where(x => x.f.FOR_Parent == model.id_parent)
                    .Select(s => new FormulasChildModel
                    {
                        id = s.f.FOR_ID,
                        model = s.m.MOD_Name,
                        drawingNo = s.m.MOD_DrawingNumber,
                        partName_id = s.m.PAR_ID,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.m.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                        level = s.m.MOD_Level,
                        quantity = s.f.FOR_Qty,
                        unit = s.f.FOR_UnitName,
                        note = s.f.FOR_Note,
                    }).OrderBy(o => o.id).ToList();

                model.ChildModelList = child_model_list;
                model.SelectChildModelList = search_child_list;

                //Set First Search Child
                if (search_child_list.Any())
                {
                    var f1 = search_child_list.FirstOrDefault();
                    model.id_child = f1.id_child;
                    model.model_child = f1.model_child;
                    model.drawingNo_child = f1.drawingNo_child;
                    model.partName_child = f1.partName_child;
                    model.level_child = f1.level_child;
                }
                //Add New Child
                if (form["AddNew"] == "AddNew")
                {
                    string child_select_str = form["check_select"];
                    string drawingNo_child = form["drawingNo_child"];
                    int child_select = !String.IsNullOrEmpty(child_select_str) ? Convert.ToInt32(child_select_str) : 0;
                    if (ModelState.IsValid)
                    {
                        if (model.quantity_child <= 0)
                        {
                            ModelState.AddModelError("quantity_child", "Only positive number allowed.");
                            return View(model);
                        }
                        if (String.IsNullOrEmpty(model.unit_child))
                        {
                            ModelState.AddModelError("unit_child", "The Unit Name is required.");
                            return View(model);
                        }
                        if (child_select <= 0)
                        {
                            ModelState.AddModelError("check_select", "The Child Model is required.");
                            return View(model);
                        }
                        if(child_select == model.id_parent)
                        {
                            ViewBag.errorMessage = String.Format("Cannot insert Parent Model, {0}.", drawingNo_child);
                            return View(model);
                        }
                        var check = db.DW_Formulas.Where(x => x.FOR_Parent == model.id_parent && x.FOR_Child == child_select).FirstOrDefault();
                        if (check != null)
                        {
                            ViewBag.errorMessage = String.Format("Duplicate Drawing Number Child Model, {0}.", drawingNo_child);
                            return View(model);
                        }

                        DW_Formulas fm = new DW_Formulas();
                        fm.FOR_Parent = model.id_parent;
                        fm.FOR_Child = child_select; // id_child
                        fm.FOR_Qty = model.quantity_child;
                        fm.FOR_UnitName = model.unit_child;
                        fm.FOR_Note = model.note_child;
                        //Save model child
                        var result = db.DW_Formulas.Add(fm);
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("ParentId:", result.FOR_Parent
                                           , "/ChildId:", result.FOR_Child
                                           , "/Qty:", result.FOR_Qty
                                           , "/UnitName:", result.FOR_UnitName);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Engineer Drawing Formulas",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = result.FOR_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        //Save
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully added, {0}", drawingNo_child);
                        return RedirectToAction("DefineFormulas", new  { id = model.id_parent});
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/DefineFormulas {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // POST: /DW_EngineerDrawingSetup/Delete
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                   
                    foreach(int delete_id in id_list)
                    {
                        //Check use in formula
                        List<int> check_parent = db.DW_Formulas.Where(x => x.FOR_Child == delete_id).Select(s => s.FOR_Parent).ToList();
                        List<int> check_child = db.DW_Formulas.Where(x => x.FOR_Parent == delete_id).Select(s => s.FOR_Child).ToList();
                        if (check_parent.Any() || check_child.Any())
                        {
                            string parent_str = "";
                            string child_str = "";
                            ErrorDeleteDrawingModel error_model = db.DW_Model.Where(x => x.MOD_ID == delete_id)
                                .Select(s => new ErrorDeleteDrawingModel { 
                                    main_id = s.MOD_ID, 
                                    main_drawing_no = s.MOD_DrawingNumber,
                                    main_model_no = s.MOD_Name,
                                    main_part_name = (from pn in db.DW_PartName where pn.PAR_ID == s.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                                }).FirstOrDefault();

                            List<ErrorDeleteModel> error_parent = new List<ErrorDeleteModel>();
                            if(check_parent.Any())
                            {
                                 error_parent = db.DW_Model.Where(x => check_parent.Contains(x.MOD_ID))
                                 .Select(s => new ErrorDeleteModel
                                 {
                                     id = s.MOD_ID,
                                     level = s.MOD_Level,
                                     drawing_no = s.MOD_DrawingNumber,
                                     model_no = s.MOD_Name,
                                     part_name = (from pn in db.DW_PartName where pn.PAR_ID == s.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                                     formulas_count = (from ef in db.DW_Formulas
                                                       where ef.FOR_Parent == s.MOD_ID
                                                       select ef.FOR_Child).Count(),
                                 }).ToList();
                                 parent_str = String.Format("Common (parent) Drawing No: {0} ,", String.Join(", ", error_parent.Select(s => s.drawing_no).ToList()));
                            }
                            List<ErrorDeleteModel> error_child = new List<ErrorDeleteModel>();
                            if(check_child.Any())
                            {
                                error_child = db.DW_Model.Where(x => check_child.Contains(x.MOD_ID))
                                 .Select(s => new ErrorDeleteModel
                                 {
                                     id = s.MOD_ID,
                                     level = s.MOD_Level,
                                     drawing_no = s.MOD_DrawingNumber,
                                     model_no = s.MOD_Name,
                                     part_name = (from pn in db.DW_PartName where pn.PAR_ID == s.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                                     formulas_count = (from ef in db.DW_Formulas
                                                       where ef.FOR_Parent == s.MOD_ID
                                                       select ef.FOR_Child).Count(),
                                 }).ToList();
                                child_str = String.Format("Furmulas (child) Drawing No: {0}", String.Join(", ", error_child.Select(s => s.drawing_no).ToList()));
                            }

                            error_model.Parent_used = error_parent;
                            error_model.Child_used = error_child;

                            TempData["errorDeleteMessage"] = error_model;
                            TempData["errorMessage"] = String.Format("This drawing cannot be deleted because it is used in the formulas, {0} {1} ", parent_str, child_str);
                            return RedirectToAction("EDList", new EngineerDrawingListModel { drawingNo = error_model.main_drawing_no, Page=1 });
                        }
                    }
                    
                    var ed_list = db.DW_Model.Where(x=> id_list.Contains(x.MOD_ID)).ToList();
                    foreach(var ed in ed_list)
                    {
                        //Remove
                        db.DW_Model.Remove(ed);
                         //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Concat("DrawingNo:", ed.MOD_DrawingNumber
                                           , "/Rev:", ed.MOD_Rev
                                           , "/ModelNo:", ed.MOD_Name
                                           , "/SemiAssemblyNo:", ed.MOD_SemiAssemblyNo
                                           , "/CustomerNo:", ed.MOD_CustomerNo 
                                           , "/PAR_ID:", ed.PAR_ID);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Engineer Drawing",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = ed.MOD_ID,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Delete drawing file
                        if(!String.IsNullOrEmpty(ed.MOD_FilePath))
                        {
                            string _path_draw = Path.Combine(path_dw_engineerDrawing, ed.MOD_FilePath);
                            if (System.IO.File.Exists(_path_draw))
                            {
                                System.IO.File.Delete(_path_draw);
                            }
                        } 
                        //Delete part name file
                        if (!String.IsNullOrEmpty(ed.MOD_PartListFilePath))
                        {
                            string _path_part = Path.Combine(path_dw_engineerPartList, ed.MOD_PartListFilePath);
                            if (System.IO.File.Exists(_path_part))
                            {
                                System.IO.File.Delete(_path_part);
                            }
                        }                        
                    }
                    if(ed_list.Any())
                    {
                        //Save
                        db.SaveChanges();
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return RedirectToAction("EDList");
                }
                 return RedirectToAction("EDList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        // POST: /DW_EngineerDrawingSetup/DeleteChild
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult DeleteChild(FormCollection form, FormulasModel model)
        {
            try
            {              
                //------------------------Delete Select---------------------------------------------//
                if(form["DeleteSelect"] == "DeleteSelect")
                {
                    List<int> id_list = new List<int>();
                    var selectedItem = form["selectedItem"];
                    if (selectedItem != null)
                    {
                        id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                        var fo_list = db.DW_Formulas.Where(x => id_list.Contains(x.FOR_ID)).ToList();
                        foreach (var fo in fo_list)
                        {
                            //Remove
                            db.DW_Formulas.Remove(fo);
                            //Save Logs
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            string log_detail = String.Concat("ParentId:", fo.FOR_Parent
                                               , "/ChildId:", fo.FOR_Child
                                               , "/Qty:", fo.FOR_Qty
                                               , "/UnitName:", fo.FOR_UnitName);
                            log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                            Log logmodel = new Log()
                            {
                                Log_Action = "delete",
                                Log_Type = "Engineer Drawing Formulas",
                                Log_System = "Drawing",
                                Log_Detail = log_detail,
                                Log_Action_Id = fo.FOR_ID,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);

                        }
                        if (fo_list.Any())
                        {
                            //Save
                            db.SaveChanges();
                        }
                        TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                        return RedirectToAction("DefineFormulas", new { id = model.id_parent });
                    }
                }
                //---------------------Save Moved---------------------------------------------------------------// 
                if(form["SaveMoved"] == "SaveMoved")
                {
                    List<int> id_list = new List<int>();
                    var moved_id_list = form["moved_id_list"];
                    if (!String.IsNullOrEmpty(moved_id_list))
                    {
                        id_list = moved_id_list.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                        //add new
                        foreach (var id in id_list)
                        {
                            var fo = db.DW_Formulas.Where(x => x.FOR_ID == id).FirstOrDefault();
                            
                            DW_Formulas fo_new = new DW_Formulas();
                            fo_new.FOR_Parent = fo.FOR_Parent;
                            fo_new.FOR_Child = fo.FOR_Child;
                            fo_new.FOR_Qty = fo.FOR_Qty;
                            fo_new.FOR_UnitName = fo.FOR_UnitName;
                            fo_new.FOR_Note = fo.FOR_Note;

                            db.DW_Formulas.Add(fo_new);
                        }
                        //remove old
                        var fo_old_list = db.DW_Formulas.Where(x => id_list.Contains(x.FOR_ID)).ToList();
                        db.DW_Formulas.RemoveRange(fo_old_list);

                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        string log_detail = String.Join(",",id_list);
                        log_detail = log_detail.Length <= 240 ? log_detail : log_detail.Substring(0, 240);
                        Log logmodel = new Log()
                        {
                            Log_Action = "edit",
                            Log_Type = "Engineer Drawing Formulas",
                            Log_System = "Drawing",
                            Log_Detail = log_detail,
                            Log_Action_Id = 0,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                        //Save 
                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully moved, {0} items. ", id_list.Count());
                        return RedirectToAction("DefineFormulas", new { id = model.id_parent });
                    }
                }
                
                return RedirectToAction("DefineFormulas", new { id = model.id_parent });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //GET: /DW_EngineerDrawingSetup/ViewFormulas
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult ViewFormulas(int id)
        {
            try
            {
                ViewFormulasModel model = db.DW_Model.Where(x => x.MOD_ID == id)
                    .Select(s => new ViewFormulasModel
                    {
                        id = s.MOD_ID,
                        drawingNo = s.MOD_DrawingNumber,
                        modelNo = s.MOD_Name,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                    }).FirstOrDefault();

                List<ChildFormulasModel> children = new List<ChildFormulasModel>();
                //Add Main Parent
                ChildFormulasModel main_parent = db.DW_Model.Where(x => x.MOD_ID == id)
                    .Select(s => new ChildFormulasModel
                    {
                        id = s.MOD_ID,
                        drawing_file = s.MOD_FilePath,
                        partlist_file = s.MOD_PartListFilePath,
                        level = s.MOD_Level,
                        drawingNo = s.MOD_DrawingNumber,
                        revision = s.MOD_Rev,
                        modelNo = s.MOD_Name,
                        semiAssembly = s.MOD_SemiAssemblyNo,
                        customerNo = s.MOD_CustomerNo,
                        partName = (from pn in db.DW_PartName where pn.PAR_ID == s.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                        note = s.MOD_Note,
                    }).FirstOrDefault();
                children.Add(main_parent);
                //Get child Recursive
                children = GetChild(id, children);
               
                model.ChildFormulasModelList = children;

                //Get tree child Recursive
                List<JsTreeModel> tree_node = new List<JsTreeModel>();
                //Add Main Root
                tree_node.Add(new JsTreeModel
                    {
                        id = main_parent.id.ToString(),
                        parent = "#",
                        text = String.Concat(main_parent.drawingNo, "___", main_parent.modelNo, "___", main_parent.partName),
                        state = new StateModel() {selected = false },
                        type = "root",
                    });
                tree_node = GetTree(id, tree_node);
                model.JsTreeModelList = tree_node;

                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/ViewFormulas {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_EngineerDrawingSetup/ViewFormulas
        [HttpPost]
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult ViewFormulas(ViewFormulasModel model)
        {
            try
            {
                List<ChildFormulasModel> children = new List<ChildFormulasModel>();
                //Add Main Parent
                ChildFormulasModel main_parent = db.DW_Model.Where(x => x.MOD_ID == model.id)
                    .Select(s => new ChildFormulasModel
                    {
                        id = s.MOD_ID,
                        drawing_file = s.MOD_FilePath,
                        partlist_file = s.MOD_PartListFilePath,
                        level = s.MOD_Level,
                        drawingNo = s.MOD_DrawingNumber,
                        revision = s.MOD_Rev,
                        modelNo = s.MOD_Name,
                        semiAssembly = s.MOD_SemiAssemblyNo,
                        customerNo = s.MOD_CustomerNo,
                        partName = (from pn in db.DW_PartName where pn.PAR_ID == s.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                        note = s.MOD_Note,
                    }).FirstOrDefault();
                children.Add(main_parent);
                //Get child Recursive
                children = GetChild(model.id, children);

                model.ChildFormulasModelList = children;
                //Export csv
                ExportToCsvFormulas(children);
               
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /DW_EngineerDrawingSetup/ViewFormulas {0}", ex.ToString());
                return View("Error");
            }
        }
        //Get Child Recursive
        public List<ChildFormulasModel> GetChild(int id, List<ChildFormulasModel> children)
        {
            List<int> id_child_list = db.DW_Formulas.Where(x => x.FOR_Parent == id).Select(s => s.FOR_Child).ToList();

            if (id_child_list.Any())
            {
                foreach (var i in id_child_list)
                {
                    ChildFormulasModel chi = db.DW_Formulas.Join(db.DW_Model, f => f.FOR_Child, m => m.MOD_ID, (f, m) => new { f = f, m = m })
                        .Where(x => x.f.FOR_Parent == id && x.f.FOR_Child == i )
                        .Select(s => new ChildFormulasModel
                        {
                            id = s.f.FOR_Child,
                            drawing_file = s.m.MOD_FilePath,
                            partlist_file = s.m.MOD_PartListFilePath,
                            level = s.m.MOD_Level,
                            drawingNo = s.m.MOD_DrawingNumber,
                            revision = s.m.MOD_Rev,
                            modelNo = s.m.MOD_Name,
                            semiAssembly = s.m.MOD_SemiAssemblyNo,
                            customerNo = s.m.MOD_CustomerNo,
                            partName = (from pn in db.DW_PartName where pn.PAR_ID == s.m.PAR_ID select pn.PAR_Name).FirstOrDefault(),
                            quantity = s.f.FOR_Qty,
                            unitname = s.f.FOR_UnitName,
                            usage = String.Concat(s.f.FOR_Qty, " ", s.f.FOR_UnitName),
                            note = s.m.MOD_Note,

                        }).FirstOrDefault();

                    children.Add(chi);
                    children = GetChild(i, children);
                }
            }
            return children;
        }

        //Get Json Child Recursive
        public List<JsTreeModel> GetTree(int id, List<JsTreeModel> tree_node)
        {
            List<int> id_child_list = db.DW_Formulas.Where(x => x.FOR_Parent == id).Select(s => s.FOR_Child).ToList();

            if (id_child_list.Any())
            {
                foreach (var i in id_child_list)
                {
                    JsTreeModel chi = db.DW_Formulas.Join(db.DW_Model, f => f.FOR_Child, m => m.MOD_ID, (f, m) => new { f = f, m = m })
                        .Where(x => x.f.FOR_Parent == id && x.f.FOR_Child == i)
                        .Select(s => new JsTreeModel
                        {
                            id = s.f.FOR_Child.ToString(),
                            parent = id.ToString(),
                            text = String.Concat(s.m.MOD_DrawingNumber, "___", s.m.MOD_Name, "___",
                                    (from pn in db.DW_PartName
                                     where pn.PAR_ID == s.m.PAR_ID
                                     select pn.PAR_Name).FirstOrDefault()
                                     , " (", s.f.FOR_Qty, " ", s.f.FOR_UnitName, ")"
                                  ),
                            state = new StateModel(),
                           // children = (from df in db.DW_Formulas where df.FOR_Parent == s.f.FOR_Child select df.FOR_Child).Any() ? true : false,

                        }).FirstOrDefault();

                    tree_node.Add(chi);
                    tree_node = GetTree(i, tree_node);
                }
            }
            return tree_node;
        }

        //
        //GET: /DW_EngineerDrawingSetup/GetCommon
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult GetCommon(int id)
        {
            try
            {
                CommonModel model = db.DW_Model.Where(x => x.MOD_ID == id)
                    .Select(s => new CommonModel
                    {
                        id = s.MOD_ID,
                        drawingNo = s.MOD_DrawingNumber,
                        modelNo = s.MOD_Name,
                        partName = (from pn in db.DW_PartName
                                    where pn.PAR_ID == s.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                    }).FirstOrDefault();

                var parent_list = db.DW_Formulas.Join(db.DW_Model, f => f.FOR_Parent, m => m.MOD_ID, (f, m) => new {f=f,m=m })
                    .Where(x => x.f.FOR_Child == id)
                    .Select(s => new ParentFormulasModel
                    {
                        id = s.m.MOD_ID,
                        level = s.m.MOD_Level,
                        drawingNo = s.m.MOD_DrawingNumber,
                        revision = s.m.MOD_Rev,
                        modelNo = s.m.MOD_Name,
                        semiAssembly = s.m.MOD_SemiAssemblyNo,
                        customerNo = s.m.MOD_CustomerNo,
                        partName = (from pn in db.DW_PartName 
                                    where pn.PAR_ID == s.m.PAR_ID
                                    select pn.PAR_Name).FirstOrDefault(),
                        note = s.m.MOD_Note
                    }).ToList();

                model.ParentFormulasModelList = parent_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_EngineerDrawingSetup/GetCommon {0}", ex.ToString());
                return View("Error");
            }
        }
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult DownloadDrawingFile(string fileName)
        {
            string path = path_dw_engineerDrawing + fileName;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    //Read the File data into Byte Array.
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    //Send the File to Download.
                    return File(bytes, "application/octet-stream", fileName);
                }
                ViewBag.errorMessage = String.Format("Could not find file.");
                return View("Error");              
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                return View("Error");
            }
        }
        [CustomAuthorize(15)]//Engineer Drawing
        public ActionResult DownloadPartListFile(string fileName)
        {
            string path = path_dw_engineerPartList + fileName;
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


        public void ExportToCsv(List<EngineerDrawingModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    level = v.level,
                    drawingNo = "\"" + v.drawingNo + "\"",
                    rev = v.rev,
                    modelno =  "\"" + v.modelNo + "\"",
                    semiAssemblyNo = "\"" + v.semiAssemblyNo + "\"",
                    customerNo =  "\"" + v.customerNo + "\"",
                    note = "\"" + v.note + "\"",
                    partName = "\"" + v.partName + "\"",
                    formulas_count = v.formulas_count,
                });
                
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    "Items", "Level", "Drawing No.", "Rev", "Part Name", "Model No.", "SemiAssembly No."
                    ,"Customer No.", "Formulas", "Note", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        item.item, item.level, item.drawingNo, item.rev, item.partName, item.modelno, item.semiAssemblyNo
                        ,item.customerNo, item.formulas_count, item.note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=EngineerDrawing.CSV ");
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

        public void ExportToCsvFormulas(List<ChildFormulasModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;
                
                var forexport = data.Select((v, i) => new
                {
                    item = i,
                    level = v.level,
                    drawingNo = "\"" + v.drawingNo + "\"",
                    rev = v.revision,
                    modelno = "\"" + v.modelNo + "\"",
                    semiAssemblyNo = "\"" + v.semiAssembly + "\"",
                    customerNo = "\"" + v.customerNo + "\"",
                    note = "\"" + v.note + "\"",
                    partName = "\"" + v.partName + "\"",
                    usage = v.usage,
                    unit = v.unitname,
                    quantity = v.quantity,
                });
                
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    "Items", "Level", "Drawing No.", "Revision", "Model No.", "SemiAssembly No."
                    , "Customer No.", "Part Name", "Usage", "Unit", "Note", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        item.item, item.level, item.drawingNo, item.rev, item.modelno, item.semiAssemblyNo
                        , item.customerNo, item.partName, item.quantity, item.unit, item.note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=Model_Formulas_Table.CSV ");
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

        // POST: /DW_EngineerDrawingSetup/GetPartName
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
        // POST: /DW_EngineerDrawingSetup/GetDrawingNo
        [HttpPost]
        public JsonResult GetDrawingNo(string Prefix)
        {
            var drawingno = db.DW_Model
                            .Where(x => x.MOD_DrawingNumber.StartsWith(Prefix))
                            .Select(s=>s.MOD_DrawingNumber)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(drawingno);
        }
        // POST: /DW_EngineerDrawingSetup/GetModelNo
        [HttpPost]
        public JsonResult GetModelNo(string Prefix)
        {
            var modelno = db.DW_Model
                            .Where(x => x.MOD_Name.StartsWith(Prefix))
                            .Select(s=>s.MOD_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelno);
        }
        // POST: /DW_EngineerDrawingSetup/GetUnitName
        [HttpPost]
        public JsonResult GetUnitName(string Prefix)
       {
            List<string> unit_list = new List<string> { "P", "PCS", "UNIT", "PC" };
            var unit = unit_list
                            .Where(x => x.StartsWith(Prefix))
                            .Take(10)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(unit);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}