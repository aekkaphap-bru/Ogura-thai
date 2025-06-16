using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Drawing
{
    [Authorize]
    public class DW_DrawingReportController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_dw_engineerDrawing = ConfigurationManager.AppSettings["path_dw_engineerDrawing"];
        private string path_dw_engineerPartList = ConfigurationManager.AppSettings["path_dw_engineerPartList"];
        private string path_dw_processDrawing = ConfigurationManager.AppSettings["path_dw_processDrawing"];

        //
        // GET: /DW_DrawingReport/DList
        [CustomAuthorize(12)]//Drawing Search
        public ActionResult DList(DrawingReportModel model)
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
                int pageSize = 30;
                int pageIndex = 1;

                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;
                int drawingType_id = model.Page.HasValue ? model.drawingType_id : 0;

                if (drawingType_id == 1)
                {
                    //------------------For Drawing----------------------------------------------------------------------------------//
                    int? groupLine_id = model.groupLine_id_pr;
                    string partName = model.partName_pr;
                    string lineName = model.lineName_pr;
                    string drawingNo = model.drawingNo_pr;
                    string modelNo = model.modelNo_pr;

                    IEnumerable<DW_ModelProcess> query = model.Page.HasValue ? db.DW_ModelProcess : db.DW_ModelProcess.Take(0);
                    if (groupLine_id.HasValue)
                    {
                        List<int> process_id = db.DW_Process.Where(x => x.PAR_ID == groupLine_id).Select(s => s.PRO_ID).ToList();
                        query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value));
                    }
                    if (!String.IsNullOrEmpty(partName))
                    {
                        List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name.Contains(partName))
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
                    }).OrderBy(o => o.groupLine).ToList();

                    /*IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);
                    model.ProcessDrawingPagedList = processdrawingPagedList;*/
                    model.ProcessDrawingList = pd_list;
                    //Set Emply list
                    List<EngineerDrawingModel> ed_list = new List<EngineerDrawingModel>();
                    /*IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);
                    model.engineerDrawingPagedList = engineerdrawingPagedList;*/
                    model.EngineerDrawingList = ed_list;
                }
                else
                {
                    //------------------For Engineer----------------------------------------------------------------------------------//
                    string partName_en = model.partName_en;
                    string drawingNo_en = model.drawingNo_en;
                    string modelNo_en = model.modelNo_en;
                    string level_en = model.level_en;
                    string semiAssemblyNo_en = model.semiAssemblyNo_en;
                    string customer_en = model.customerNo_en;

                    IEnumerable<DW_Model> query = model.Page.HasValue ? db.DW_Model : db.DW_Model.Take(0);
                    if (!String.IsNullOrEmpty(partName_en))
                    {
                        List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name.Contains(partName_en))
                            .Select(s => s.PAR_ID).ToList();
                        query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                    }
                    if (!String.IsNullOrEmpty(drawingNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_DrawingNumber) && x.MOD_DrawingNumber.ToLowerInvariant().Contains(drawingNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(modelNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Name) && x.MOD_Name.ToLowerInvariant().Contains(modelNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(level_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Level) && x.MOD_Level.ToLowerInvariant().Contains(level_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(semiAssemblyNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_SemiAssemblyNo) && x.MOD_SemiAssemblyNo.ToLowerInvariant().Contains(semiAssemblyNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(customer_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_CustomerNo) && x.MOD_CustomerNo.ToLowerInvariant().Contains(customer_en.ToLowerInvariant()));
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
                    }).OrderBy(s => s.partName).ThenBy(o => o.modelNo).ToList();

                    /*IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);
                    model.engineerDrawingPagedList = engineerdrawingPagedList;*/
                    model.EngineerDrawingList = ed_list;
                    //Set 
                    List<ProcessDrawingModel> pd_list = new List<ProcessDrawingModel>();
                    /*IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);
                    model.ProcessDrawingPagedList = processdrawingPagedList;*/
                    model.ProcessDrawingList = pd_list;
                }

                //Select Group Line Name(Part Name)
                List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                //Select Drawing Type
                List<SelectListItem> selectDrawingType = new List<SelectListItem>();
                selectDrawingType.Add(new SelectListItem { Value = "0", Text = "Engineer" });
                selectDrawingType.Add(new SelectListItem { Value = "1", Text = "Process" });

                model.SelectGroupLine = selectPartName;
                model.SelectDrawingType = selectDrawingType;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //DW_DrawingReport/DList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //POSt: /DW_DrawingReport/DList
        [HttpPost]
        [CustomAuthorize(12)]//Drawing Search
        public ActionResult DList(FormCollection form, DrawingReportModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                int drawingType_id = model.drawingType_id;

                if (drawingType_id == 1)
                {
                    //------------------For Drawing----------------------------------------------------------------------------------//
                    int? groupLine_id = model.groupLine_id_pr;
                    string partName = model.partName_pr;
                    string lineName = model.lineName_pr;
                    string drawingNo = model.drawingNo_pr;
                    string modelNo = model.modelNo_pr;

                    IEnumerable<DW_ModelProcess> query = db.DW_ModelProcess;
                    if (groupLine_id.HasValue)
                    {
                        List<int> process_id = db.DW_Process.Where(x => x.PAR_ID == groupLine_id).Select(s => s.PRO_ID).ToList();
                        query = query.Where(x => x.PRO_ID.HasValue && process_id.Contains(x.PRO_ID.Value));
                    }
                    if (!String.IsNullOrEmpty(partName))
                    {
                        List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name.Contains(partName))
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
                    }).OrderBy(o => o.groupLine).ToList();

                    if (form["ExportToCsv"] == "ExportToCsv")
                    {
                        ExportToCsvProcess(pd_list);
                    }
                    /*IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);
                    model.ProcessDrawingPagedList = processdrawingPagedList;*/
                    model.ProcessDrawingList = pd_list;
                    //Set Emply list
                    List<EngineerDrawingModel> ed_list = new List<EngineerDrawingModel>();
                    /*IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);
                    model.engineerDrawingPagedList = engineerdrawingPagedList;*/
                    model.EngineerDrawingList = ed_list;
                }
                else
                {
                    //------------------For Engineer----------------------------------------------------------------------------------//
                    string partName_en = model.partName_en;
                    string drawingNo_en = model.drawingNo_en;
                    string modelNo_en = model.modelNo_en;
                    string level_en = model.level_en;
                    string semiAssemblyNo_en = model.semiAssemblyNo_en;
                    string customer_en = model.customerNo_en;

                    IEnumerable<DW_Model> query = db.DW_Model;
                    if (!String.IsNullOrEmpty(partName_en))
                    {
                        List<int> partname_id = db.DW_PartName.Where(x => x.PAR_Name.Contains(partName_en))
                            .Select(s => s.PAR_ID).ToList();
                        query = query.Where(x => x.PAR_ID.HasValue && partname_id.Contains(x.PAR_ID.Value));
                    }
                    if (!String.IsNullOrEmpty(drawingNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_DrawingNumber) && x.MOD_DrawingNumber.ToLowerInvariant().Contains(drawingNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(modelNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Name) && x.MOD_Name.ToLowerInvariant().Contains(modelNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(level_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_Level) && x.MOD_Level.ToLowerInvariant().Contains(level_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(semiAssemblyNo_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_SemiAssemblyNo) && x.MOD_SemiAssemblyNo.ToLowerInvariant().Contains(semiAssemblyNo_en.ToLowerInvariant()));
                    }
                    if (!String.IsNullOrEmpty(customer_en))
                    {
                        query = query.Where(x => !String.IsNullOrEmpty(x.MOD_CustomerNo) && x.MOD_CustomerNo.ToLowerInvariant().Contains(customer_en.ToLowerInvariant()));
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
                    }).OrderBy(s => s.partName).ThenBy(o => o.modelNo).ToList();

                    if (form["ExportToCsv"] == "ExportToCsv")
                    {
                        ExportToCsvEngineer(ed_list);
                    }
                    /*IPagedList<EngineerDrawingModel> engineerdrawingPagedList = ed_list.ToPagedList(pageIndex, pageSize);
                    model.engineerDrawingPagedList = engineerdrawingPagedList;*/
                    model.EngineerDrawingList = ed_list;
                    //Set 
                    List<ProcessDrawingModel> pd_list = new List<ProcessDrawingModel>();
                    /*IPagedList<ProcessDrawingModel> processdrawingPagedList = pd_list.ToPagedList(pageIndex, pageSize);
                    model.ProcessDrawingPagedList = processdrawingPagedList;*/
                    model.ProcessDrawingList = pd_list;
                }

                //Select Group Line Name(Part Name)
                List<int> select_partname_id_list = db.DW_Process.Select(s => s.PAR_ID).Distinct().ToList();
                List<SelectListItem> selectPartName = db.DW_PartName
                    .Where(x => select_partname_id_list.Contains(x.PAR_ID))
                    .Select(s => new SelectListItem { Value = s.PAR_ID.ToString(), Text = s.PAR_Name }).ToList();

                //Select Drawing Type
                List<SelectListItem> selectDrawingType = new List<SelectListItem>();
                selectDrawingType.Add(new SelectListItem { Value = "0", Text = "Engineer" });
                selectDrawingType.Add(new SelectListItem { Value = "1", Text = "Process" });

                model.SelectGroupLine = selectPartName;
                model.SelectDrawingType = selectDrawingType;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //DW_DrawingReport/DList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        //GET: /DW_DrawingReport/ViewFormulas
        [CustomAuthorize(12)]//Drawing Search
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
                    state = new StateModel() { selected = false },
                    type = "root",
                });
                tree_node = GetTree(id, tree_node);
                model.JsTreeModelList = tree_node;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get /DW_DrawingReport/ViewFormulas {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //POST: /DW_DrawingReport/ViewFormulas
        [HttpPost]
        [CustomAuthorize(12)]//Drawing Search
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
                ViewBag.errorMessage = String.Format("Error: Post /DW_DrawingReport/ViewFormulas {0}", ex.ToString());
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
                        .Where(x => x.f.FOR_Parent == id && x.f.FOR_Child == i)
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
        //GET: /DW_DrawingReport/GetCommon
        [CustomAuthorize(12)]//Drawing Search
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

                var parent_list = db.DW_Formulas.Join(db.DW_Model, f => f.FOR_Parent, m => m.MOD_ID, (f, m) => new { f = f, m = m })
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
                ViewBag.errorMessage = String.Format("Error: Get /DW_DrawingReport/GetCommon {0}", ex.ToString());
                return View("Error");
            }
        }

        //Export Formulas
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

        //Export to Csv Engineer
        public void ExportToCsvEngineer(List<EngineerDrawingModel> model)
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
                    modelno = "\"" + v.modelNo + "\"",
                    semiAssemblyNo = "\"" + v.semiAssemblyNo + "\"",
                    customerNo = "\"" + v.customerNo + "\"",
                    note = "\"" + v.note + "\"",
                    partName = "\"" + v.partName + "\"",
                    formulas_count = v.formulas_count,
                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    "Items", "Level", "Drawing No.", "Rev", "Part Name", "Model No.", "SemiAssembly No."
                    , "Customer No.", "Formulas", "Note", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        item.item, item.level, item.drawingNo, item.rev, item.partName, item.modelno, item.semiAssemblyNo
                        , item.customerNo, item.formulas_count, item.note, Environment.NewLine);
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
        //Export to Csv Process
        public void ExportToCsvProcess(List<ProcessDrawingModel> model)
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
                        , item.rev, item.partName, item.modelno, item.remark, Environment.NewLine);
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

        //Load Engineer Drawing File
        [CustomAuthorize(12)]//Drawing Search
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
        //Load Engineer Part List File
        [CustomAuthorize(12)]//Drawing Search
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
        //Load Process Drawing File
        [CustomAuthorize(12)]//Drawing Search
        public ActionResult DownloadFileProcessDrawing(string fileName)
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


        // POST: /DW_DrawingReport/GetPartNamePr
        [HttpPost]
        public JsonResult GetPartNamePr(string Prefix)
        {
            var partname = db.DW_PartName
                            .Where(x => x.PAR_Name.StartsWith(Prefix))
                            .Take(20)
                            .Select(s => new { label = s.PAR_Name, val = s.PAR_Name }).ToList();
            return Json(partname);
        }
        // POST: /DW_DrawingReport/GetLineNamePr
        [HttpPost]
        public JsonResult GetLineNamePr(string Prefix)
        {
            var linename = db.DW_Process
                            .Where(x => x.PRO_Name.StartsWith(Prefix))
                            .Select(s => s.PRO_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(linename);
        }
        // POST: /DW_DrawingReport/GetDrawingNoPr
        [HttpPost]
        public JsonResult GetDrawingNoPr(string Prefix)
        {
            var drawingno = db.DW_ModelProcess
                            .Where(x => x.MOP_DrawingNumber.StartsWith(Prefix))
                            .Select(s => s.MOP_DrawingNumber)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(drawingno);
        }
        // POST: /DW_DrawingReport/GetModelNoPr
        [HttpPost]
        public JsonResult GetModelNoPr(string Prefix)
        {
            var modelno = db.DW_ModelProcess
                            .Where(x => x.MOP_Name.StartsWith(Prefix))
                            .Select(s => s.MOP_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelno);
        }
        // POST: /DW_DrawingReport/GetPartNameEn
        [HttpPost]
        public JsonResult GetPartNameEn(string Prefix)
        {
            var partname = db.DW_PartName
                            .Where(x => x.PAR_Name.StartsWith(Prefix))
                            .Select(s => s.PAR_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(partname);
        }
        // POST: /DW_DrawingReport/GetDrawingNoEn
        [HttpPost]
        public JsonResult GetDrawingNoEn(string Prefix)
        {
            var drawingno = db.DW_Model
                            .Where(x => x.MOD_DrawingNumber.StartsWith(Prefix))
                            .Select(s => s.MOD_DrawingNumber)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(drawingno);
        }
        // POST: /DW_DrawingReport/GetModelNoEn
        [HttpPost]
        public JsonResult GetModelNoEn(string Prefix)
        {
            var modelno = db.DW_Model
                            .Where(x => x.MOD_Name.StartsWith(Prefix))
                            .Select(s => s.MOD_Name)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelno);
        }
        // POST: /DW_DrawingReport/GetLevelEn
        [HttpPost]
        public JsonResult GetLevelEn(string Prefix)
        {
            var level = db.DW_Model
                            .Where(x => x.MOD_Level.StartsWith(Prefix))
                            .Select(s => s.MOD_Level)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(level);
        }
        // POST: /DW_DrawingReport/GetSemiAssemblyEn
        [HttpPost]
        public JsonResult GetSemiAssemblyEn(string Prefix)
        {
            var semi = db.DW_Model
                            .Where(x => x.MOD_SemiAssemblyNo.StartsWith(Prefix))
                            .Select(s => s.MOD_SemiAssemblyNo)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(semi);
        }
        // POST: /DW_DrawingReport/GetCustomerNoEn
        [HttpPost]
        public JsonResult GetCustomerNoEn(string Prefix)
        {
            var customer = db.DW_Model
                            .Where(x => x.MOD_CustomerNo.StartsWith(Prefix))
                            .Select(s => s.MOD_CustomerNo)
                            .Distinct()
                            .Take(20)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(customer);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}