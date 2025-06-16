using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class WorkingStandardSetController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_ws = ConfigurationManager.AppSettings["path_ws"];

        //
        // GET: /WorkingStandardSet/WSSetList
        [CustomAuthorize(21)]//3 Working standard/Setup WS/WS & Rule
        public ActionResult WSSetList(WorkingStandardSetModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                if (TempData["shortMessageError"] != null)
                {
                    ViewBag.MessageError = TempData["shortMessageError"].ToString();
                }
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                int? searchDepartmentId = model.searchDepartmentId;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;
                string searchWorkingStandardName = model.searchWorkingStandardName;
                string searchWorkingStandardNo = model.searchWorkingStandardNo;
                int? searchSortBy = model.searchSortBy;

                //Set data update latest
                var date_latest = db.WSR_WorkingStandardEdit.OrderByDescending(o => o.WS_Date).Take(1).Select(s => s.WS_Date).FirstOrDefault();
                string date_str = date_latest.ToString("MMMM dd yyyy HH:mm tt");
                model.date_str = date_str;

                IEnumerable<WSR_WorkingStandardEdit> query = model.Page.HasValue ? db.WSR_WorkingStandardEdit : db.WSR_WorkingStandardEdit.Take(0);

                if (searchDepartmentId.HasValue)
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
                    List<int> wsp_id_list = db.WSR_WorkingStandardProcessEdit.Where(x => x.WSP_ProcessName == searchWorkingStandardProcess)
                       .Select(s => s.WSP_Id).ToList();
                    query = query.Where(x => x.WSP_Id.HasValue && wsp_id_list.Contains(x.WSP_Id.Value));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WS_Name) && x.WS_Name.Replace("/", "").ToLowerInvariant().Contains(searchWorkingStandardName.ToLowerInvariant())));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WS_Number) && x.WS_Number.ToLowerInvariant().Contains(searchWorkingStandardNo.ToLowerInvariant())));
                }

                var workingStandardList = query.Select(s => new WSR_WorkingStandardEditModel
                {
                    WS_Date = s.WS_Date,
                    WS_File = s.WS_File,
                    WS_Id = s.WS_Id,
                    WS_Name = s.WS_Name.Replace("/", "<br>"),
                    WS_Note = s.WS_Note,
                    WS_Number = s.WS_Number,
                    WS_Rev = s.WS_Rev,
                    UpDate_str = s.WS_Update.ToString("yyyy-MM-dd"),
                    WS_Update = s.WS_Update,
                    WSP_Id = s.WSP_Id,
                    WST_Id = s.WST_Id,
                    WorkingStandardType = (from wst in db.WSR_WorkingStandardTypeEdit
                                           where wst.WST_Id == s.WST_Id
                                           select wst.WST_Type).FirstOrDefault(),
                    WorkingStandardProcess = (from wsp in db.WSR_WorkingStandardProcessEdit
                                              where wsp.WSP_Id == s.WSP_Id
                                              select wsp.WSP_ProcessName).FirstOrDefault(),
                    dept_id = (from wst in db.WSR_WorkingStandardTypeEdit
                               where wst.WST_Id == s.WST_Id
                               select wst.WST_Dept).FirstOrDefault(),
                }).ToList();

                switch (searchSortBy)
                {
                    case 1:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Name)
                            .ThenBy(s => s.WorkingStandardType).ThenBy(s => s.WorkingStandardProcess).ToList();
                        break;
                    case 2:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Name)
                            .ThenBy(s => s.WorkingStandardProcess).ThenBy(s => s.WorkingStandardType).ToList();
                        break;
                    case 3:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardType)
                            .ThenBy(s => s.WorkingStandardProcess).ThenBy(s => s.WS_Name).ToList();
                        break;
                    case 4:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardType)
                            .ThenBy(s => s.WS_Name).ThenBy(s => s.WorkingStandardProcess).ToList();
                        break;
                    case 5:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardProcess)
                            .ThenBy(s => s.WS_Name).ThenBy(s => s.WorkingStandardType).ToList();
                        break;
                    case 6:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardProcess)
                            .ThenBy(s => s.WorkingStandardType).ThenBy(s => s.WS_Name).ToList();
                        break;
                    default:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Number)
                           .ToList();
                        break;
                }
                //Get permission
                List<int> dept_id = new List<int>() { 3, 4 };
                if (Session["USE_Id"] != null)
                {
                    int use_id = Convert.ToInt32(Session["USE_Id"]);
                    //Permision Document BOI and Purchase (35) /dept_id : 3
                    int rights_boi = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 35).Count();
                    //Permision Document Accounting and Finance (36)/dept_id :4
                    int rights_acc = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 36).Count();

                    if (rights_boi <= 0)
                    {
                        workingStandardList.Where(x => x.dept_id == 3).ToList().ForEach(f => f.WS_File = "");
                    }
                    if (rights_acc <= 0)
                    {
                        workingStandardList.Where(x => x.dept_id == 4).ToList().ForEach(f => f.WS_File = "");
                    }
                }
                else
                {
                    workingStandardList.Where(x => dept_id.Contains(x.dept_id)).ToList().ForEach(f => f.WS_File = "");
                }
                IPagedList<WSR_WorkingStandardEditModel> workingStandardPagedList = workingStandardList.ToPagedList(pageIndex, pageSize);

                GetSelectOption(model);

                model.WSR_WorkingStandardModelPagedList = workingStandardPagedList;

                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        //
        // POST: /WorkingStandardSet/WSSetList/
        [HttpPost]
        [CustomAuthorize(21)]//3 Working standard/Setup WS/WS & Rule
        public ActionResult WSSetList(FormCollection form, WorkingStandardSetModel model)
        {
            try
            {

                int pageSize = 30;
                int pageIndex = 1;

                int? searchDepartmentId = model.searchDepartmentId;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;
                string searchWorkingStandardName = model.searchWorkingStandardName;
                string searchWorkingStandardNo = model.searchWorkingStandardNo;
                int? searchSortBy = model.searchSortBy;

                //Set data update latest
                var date_latest = db.WSR_WorkingStandardEdit.OrderByDescending(o => o.WS_Date).Take(1).Select(s => s.WS_Date).FirstOrDefault();
                string date_str = date_latest.ToString("MMMM dd yyyy HH:mm tt");
                model.date_str = date_str;

                IEnumerable<WSR_WorkingStandardEdit> query = db.WSR_WorkingStandardEdit;
                if (searchDepartmentId.HasValue)
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
                    List<int> wsp_id_list = db.WSR_WorkingStandardProcessEdit.Where(x => x.WSP_ProcessName == searchWorkingStandardProcess)
                         .Select(s => s.WSP_Id).ToList();
                    query = query.Where(x => x.WSP_Id.HasValue && wsp_id_list.Contains(x.WSP_Id.Value));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WS_Name) && x.WS_Name.Replace("/", "").ToLowerInvariant().Contains(searchWorkingStandardName.ToLowerInvariant())));
                }
                if (!String.IsNullOrEmpty(searchWorkingStandardNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.WS_Number) && x.WS_Number.ToLowerInvariant().Contains(searchWorkingStandardNo.ToLowerInvariant())));
                }

                var workingStandardList = query.Select(s => new WSR_WorkingStandardEditModel
                {
                    WS_Date = s.WS_Date,
                    WS_File = s.WS_File,
                    WS_Id = s.WS_Id,
                    WS_Name = s.WS_Name.Replace("/", "<br>"),
                    WS_Note = s.WS_Note,
                    WS_Number = s.WS_Number,
                    WS_Rev = s.WS_Rev,
                    UpDate_str = s.WS_Update.ToString("yyyy-MM-dd"),
                    WS_Update = s.WS_Update,
                    WSP_Id = s.WSP_Id,
                    WST_Id = s.WST_Id,
                    WorkingStandardType = (from wst in db.WSR_WorkingStandardTypeEdit
                                           where wst.WST_Id == s.WST_Id
                                           select wst.WST_Type).FirstOrDefault(),
                    WorkingStandardProcess = (from wsp in db.WSR_WorkingStandardProcessEdit
                                              where wsp.WSP_Id == s.WSP_Id
                                              select wsp.WSP_ProcessName).FirstOrDefault(),
                    dept_id = (from wst in db.WSR_WorkingStandardTypeEdit
                               where wst.WST_Id == s.WST_Id
                               select wst.WST_Dept).FirstOrDefault(),

                }).ToList();

                switch (searchSortBy)
                {
                    case 1:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Name)
                            .ThenBy(s => s.WorkingStandardType).ThenBy(s => s.WorkingStandardProcess).ToList();
                        break;
                    case 2:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Name)
                            .ThenBy(s => s.WorkingStandardProcess).ThenBy(s => s.WorkingStandardType).ToList();
                        break;
                    case 3:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardType)
                            .ThenBy(s => s.WorkingStandardProcess).ThenBy(s => s.WS_Name).ToList();
                        break;
                    case 4:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardType)
                            .ThenBy(s => s.WS_Name).ThenBy(s => s.WorkingStandardProcess).ToList();
                        break;
                    case 5:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardProcess)
                            .ThenBy(s => s.WS_Name).ThenBy(s => s.WorkingStandardType).ToList();
                        break;
                    case 6:
                        workingStandardList = workingStandardList.OrderBy(s => s.WorkingStandardProcess)
                            .ThenBy(s => s.WorkingStandardType).ThenBy(s => s.WS_Name).ToList();
                        break;
                    default:
                        workingStandardList = workingStandardList.OrderBy(s => s.WS_Number)
                          .ToList();
                        break;
                }

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(workingStandardList);
                }

                //Get permission
                List<int> dept_id = new List<int>() { 3, 4 };
                if (Session["USE_Id"] != null)
                {
                    int use_id = Convert.ToInt32(Session["USE_Id"]);
                    //Permision Document BOI and Purchase (35) /dept_id : 3
                    int rights_boi = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 35).Count();
                    //Permision Document Accounting and Finance (36)/dept_id :4
                    int rights_acc = db.UserRights.Where(x => x.USE_Id == use_id && x.RIH_Id == 36).Count();

                    if (rights_boi <= 0)
                    {
                        workingStandardList.Where(x => x.dept_id == 3).ToList().ForEach(f => f.WS_File = "");
                    }
                    if (rights_acc <= 0)
                    {
                        workingStandardList.Where(x => x.dept_id == 4).ToList().ForEach(f => f.WS_File = "");
                    }
                }
                else
                {
                    workingStandardList.Where(x => dept_id.Contains(x.dept_id)).ToList().ForEach(f => f.WS_File = "");
                }


                //Get Select option
                GetSelectOption(model);

                IPagedList<WSR_WorkingStandardEditModel> workingStandardPagedList = workingStandardList.ToPagedList(pageIndex, pageSize);

                model.WSR_WorkingStandardModelPagedList = workingStandardPagedList;

                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }


        public WorkingStandardSetModel GetSelectOption(WorkingStandardSetModel model)
        {
            try
            {
                //Select department
                List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                // Select working standard type
                IEnumerable<WSR_WorkingStandardTypeEdit> query_wst = db.WSR_WorkingStandardTypeEdit;
                query_wst = model.searchDepartmentId.HasValue ?
                    query_wst.Where(w => w.WST_Dept == model.searchDepartmentId).ToList() : query_wst;

                List<SelectListItem> SelectWorkingStandardType_list = query_wst.Select(s => s.WST_Type).Distinct().OrderBy(o => o)
                    .Select(s => new SelectListItem { Value = s, Text = s }).ToList();

                //select working standard process
                IEnumerable<WSR_WorkingStandardProcessEdit> query_wsp = db.WSR_WorkingStandardProcessEdit;

                List<int> wst_id_list_2 = new List<int>();
                wst_id_list_2 = query_wst.Select(s => s.WST_Id).ToList();

                if (query_wsp.Any())
                {
                    query_wsp = query_wsp.Where(w => wst_id_list_2.Contains(w.WST_Id)).ToList();
                }
                if (!String.IsNullOrEmpty(model.searchWorkingStandardType))
                {
                    int wst_id = query_wst.Where(w => w.WST_Type == model.searchWorkingStandardType).Select(s => s.WST_Id).FirstOrDefault();
                    query_wsp = query_wsp.Where(w => w.WST_Id == wst_id).ToList();
                }
                List<SelectListItem> SelectWorkingStandardProcess_list = query_wsp.Select(s => s.WSP_ProcessName).Distinct().OrderBy(o => o)
                    .Select(s => new SelectListItem { Value = s, Text = s }).ToList();

                //Select SortBy
                List<SelectListItem> SelectSortBy_list = new List<SelectListItem>();
                SelectSortBy_list.Add(new SelectListItem() { Value = "1", Text = "WS Name, WS Type, WS Process" });
                SelectSortBy_list.Add(new SelectListItem() { Value = "2", Text = "WS Name, WS Process, WS Type" });
                SelectSortBy_list.Add(new SelectListItem() { Value = "3", Text = "WS Type, WS Process, WS Name" });
                SelectSortBy_list.Add(new SelectListItem() { Value = "4", Text = "WS Type, WS Name, WS Process" });
                SelectSortBy_list.Add(new SelectListItem() { Value = "5", Text = "WS Process, WS Name, WS Type " });
                SelectSortBy_list.Add(new SelectListItem() { Value = "6", Text = "WS Process, WS Type, WS Name" });

                model.SelectDepartment = SelectDepartment_list;
                model.SelectWorkingStandardType = SelectWorkingStandardType_list;
                model.SelectWorkingStandardProcess = SelectWorkingStandardProcess_list;
                model.SelectSearchSortBy = SelectSortBy_list;

                return model;

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return model;
            }
        }

        //
        // GET: /WorkingStandardSet/Create
        [CustomAuthorize(21)]//3 Working standard/Setup WS/WS & Rule
        public ActionResult Create()
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            WSR_WorkingStandardEditModel model = new WSR_WorkingStandardEditModel();

            //Select department
            List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

            // Select working standard type
            List<SelectListItem> SelectWorkingStandardType_list = db.WSR_WorkingStandardTypeEdit.OrderBy(o => o.WST_Type)
                .Select(s => new SelectListItem { Value = s.WST_Id.ToString(), Text = s.WST_Type }).ToList();

            //select working standard process
            List<SelectListItem> SelectWorkingStandardProcess_list = db.WSR_WorkingStandardProcessEdit.OrderBy(o => o.WSP_ProcessName)
                .Select(s => new SelectListItem { Value = s.WSP_Id.ToString(), Text = s.WSP_ProcessName }).ToList();

            //select ws trianing section left
            List<SelectListItem> SelectWSTrainingSection_list = db.WS_Training_Section.OrderBy(o => o.SectionName)
                .Select(s => new SelectListItem { Value = s.SectionName, Text = s.SectionName }).ToList();

            model.SelectDepartment = SelectDepartment_list;
            model.SelectWorkingStandardType = SelectWorkingStandardType_list;
            model.SelectWorkingStandardProcess = SelectWorkingStandardProcess_list;
            model.SelectWSTrainingSection = SelectWSTrainingSection_list;
            model.checktraining = false;

            return View(model);
        }


        [HttpPost]
        [CustomAuthorize(21)] //3 Working standard/Setup WS/WS & Rule
        public ActionResult Create(HttpPostedFileBase file, WSR_WorkingStandardEditModel model, FormCollection form)
        {
            try
            {
                GetSelectOption_WS(model);

                List<string> wstraining_selectedvalue = model.selected_trainWS;
                var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };

                if (!ModelState.IsValid)
                {

                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "Validation errors occurred.", errors });

                }

                if (file != null)
                {
                    string wst_str = model.WST_Id > 0 ? model.WST_Id.ToString() : "000";
                    string wsp_str = model.WSP_Id > 0 ? model.WSP_Id.ToString() : "000";
                    string _FileName = String.Concat(wst_str, wsp_str, model.WS_Number, Path.GetExtension(file.FileName));
                    string _path = Path.Combine(path_ws, _FileName);
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                    string pattern = "^[a-zA-Z0-9_.\\s-]*$";
                    Match match = Regex.Match(_FileName, pattern, RegexOptions.IgnoreCase);

                    if (!match.Success)
                    {
                        return Json(new { success = false, message = "The following characters are invalid in a filename." });
                    }

                    if (!supportedTypes.Contains(fileExt))
                    {
                        return Json(new { success = false, message = "Invalid file extension. Only word, PDF, excel, text and image (.jpg, .jpeg, .png) files are allowed." });
                    }

                    if (System.IO.File.Exists(_path))
                    {
                        //return Json(new { success = false, message = $"The file {_FileName} already exists." });
                        return Json(new { success = false, message = "The file already exists." });
                    }

                    WSR_WorkingStandardEdit ws = new WSR_WorkingStandardEdit()
                    {
                        WS_File = _FileName,
                        WS_Date = DateTime.Now,
                        WS_Name = String.Concat(model.WS_Name_th, "/", model.WS_Name_eng),
                        WS_Number = model.WS_Number,
                        WS_Note = model.WS_Note,
                        WS_Rev = model.WS_Rev,
                        WS_Update = model.WS_Update,
                        WSP_Id = model.WSP_Id,
                        WST_Id = model.WST_Id
                    };
                    WSTrainingForSendEmailModel wstrain = new WSTrainingForSendEmailModel();

                    if (wstraining_selectedvalue != null && wstraining_selectedvalue.Any())
                    {
                        wstrain.Train_Header = model.WS_Name_eng;
                        wstrain.Train_HeaderThai = model.WS_Name_th;
                        wstrain.Train_NumberWS = model.WS_Number;
                        wstrain.Train_DateWS = model.WS_Update.ToString("yyyy-MM-dd");
                        wstrain.Trai_NameArea = String.Join(",", wstraining_selectedvalue);
                        List<Emp_TrainingWS_temporary> wstraining_list = new List<Emp_TrainingWS_temporary>();

                        foreach (var i in wstraining_selectedvalue)
                        {
                            if (!String.IsNullOrEmpty(i))
                            {
                                Emp_TrainingWS_temporary wstraining = new Emp_TrainingWS_temporary()
                                {
                                    Train_NumberWS = model.WS_Number,
                                    Train_Header = model.WS_Name_eng,
                                    Train_HeaderThai = model.WS_Name_th,
                                    Train_DateWS = model.WS_Update.ToString("yyyy-MM-dd"),
                                    Train_Status = 0,
                                    Trai_NameArea = i,
                                    Trai_Rev = model.WS_Rev,
                                };
                                wstraining_list.Add(wstraining);
                            }
                        }
                        db.Emp_TrainingWS_temporary.AddRange(wstraining_list);
                    }

                    var result = db.WSR_WorkingStandardEdit.Add(ws);

                    //Save Logs
                    if (result != null)
                    {
                        string user_nickname = null;
                        Console.WriteLine("Hello World!");

                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                            Console.WriteLine(user_nickname);
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Working Standard",
                            Log_System = "WS & Rule",
                            Log_Detail = String.Concat(result.WS_Number, "/"
                                        , result.WS_Name, "/", result.WS_Rev, "/"
                                        , result.WST_Id, "/", result.WSP_Id, "/"
                                        , result.WS_File),
                            Log_Action_Id = result.WS_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }

                    //db.WSR_WorkingStandardEdit.Add(ws);
                    db.SaveChanges();

                    if (wstrain.Train_Header != null && wstraining_selectedvalue != null)
                    {
                        SendEmailWS_with_wsnumber(wstrain);
                    }

                    file.SaveAs(_path);

                    TempData["shortMessage"] = String.Format("Successfully created the Working Standard {0}  ", model.WS_Number);

                    return Json(new { success = true, message = "Successfully created the Working Standard.", redirectUrl = Url.Action("WSSetList" , new { Page = 1, searchWorkingStandardNo = model.WS_Number }) });
                }
                else
                {
                    return Json(new { success = false, message = "The Working Standard File is required." });
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public WSR_WorkingStandardEditModel GetSelectOption_WS(WSR_WorkingStandardEditModel model)
        {
            List<string> wstraining_selectedvalue = model.selected_trainWS;
            int? searchDepartmentId = null;
            int searchWorkingStandardTypeId = 0;
            int? searchWorkingStandardProcessId = null;

            searchDepartmentId = model.searchDepartmentId.HasValue ? model.searchDepartmentId : null;
            searchWorkingStandardTypeId = model.WST_Id > 0 ? model.WST_Id : 0;
            searchWorkingStandardProcessId = model.WSP_Id.HasValue ? model.WSP_Id : null;

            //Select department
            List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

            // Select working standard type
            IEnumerable<WSR_WorkingStandardTypeEdit> query_wst = db.WSR_WorkingStandardTypeEdit;
            query_wst = searchDepartmentId.HasValue ? query_wst.Where(w => w.WST_Dept == searchDepartmentId).ToList()
                : query_wst;
            List<SelectListItem> SelectWorkingStandardType_list = query_wst.OrderBy(o => o.WST_Type)
                .Select(s => new SelectListItem { Value = s.WST_Id.ToString(), Text = s.WST_Type }).ToList();

            //select working standard process
            IEnumerable<WSR_WorkingStandardProcessEdit> query_wsp = db.WSR_WorkingStandardProcessEdit;
            query_wsp = searchWorkingStandardTypeId > 0 ? query_wsp.Where(w => w.WST_Id == searchWorkingStandardTypeId).ToList()
                : query_wsp;
            List<SelectListItem> SelectWorkingStandardProcess_list = query_wsp.OrderBy(o => o.WSP_ProcessName)
                .Select(s => new SelectListItem { Value = s.WSP_Id.ToString(), Text = s.WSP_ProcessName }).ToList();

            //select ws trianing section 
            IEnumerable<WS_Training_Section> query_sec = db.WS_Training_Section;
            query_sec = wstraining_selectedvalue != null ? query_sec.Where(x => !wstraining_selectedvalue.Contains(x.SectionName)).ToList()
                : query_sec.ToList();
            List<SelectListItem> SelectWSTrainingSection_list = query_sec.OrderBy(o => o.SectionName)
                .Select(s => new SelectListItem { Value = s.SectionName, Text = s.SectionName }).ToList();

            if (wstraining_selectedvalue != null)
            {
                model.checktraining = true;
                foreach (var i in wstraining_selectedvalue)
                {
                    SelectWSTrainingSection_list.Add(new SelectListItem() { Value = i, Text = i, Selected = true });
                }
            }
            model.SelectDepartment = SelectDepartment_list;
            model.SelectWorkingStandardType = SelectWorkingStandardType_list;
            model.SelectWorkingStandardProcess = SelectWorkingStandardProcess_list;
            model.SelectWSTrainingSection = SelectWSTrainingSection_list;
            return model;
        }

        //
        // GET: /WorkingStandardSet/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }

                var model = db.WSR_WorkingStandardEdit.Where(x => x.WS_Id == id)
                    .Select(s => new WSR_WorkingStandardEditModel
                    {
                        WS_Id = s.WS_Id,
                        WS_Name = s.WS_Name,
                        WS_Number = s.WS_Number,
                        WS_File = s.WS_File,
                        WS_Rev = s.WS_Rev,
                        WS_Note = s.WS_Note,
                        WS_Update = s.WS_Update,
                        WST_Id = s.WST_Id,
                        WSP_Id = s.WSP_Id,
                        searchDepartmentId = (from wst in db.WSR_WorkingStandardTypeEdit
                                              where wst.WST_Id == s.WST_Id
                                              select wst.WST_Dept).FirstOrDefault(),
                        selected_trainWS = (from traintemp in db.Emp_TrainingWS_temporary
                                            where traintemp.Train_NumberWS == s.WS_Number && traintemp.Trai_Rev == s.WS_Rev
                                            //&& traintemp.Train_Status == 0
                                            select traintemp.Trai_NameArea).ToList()

                    }).FirstOrDefault();
                if (model != null)
                {
                    string[] str_list = model.WS_Name.Split(new char[] { '/' }, 2);
                    model.WS_Name_th = str_list[0];
                    model.WS_Name_eng = str_list[1];

                    GetSelectOption_WS(model);


                    return View(model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            ViewBag.errorMessage = "Working Standard Id is null";
            return View("Error");
        }

        //
        // POST: /WorkingStandardSet/Edit/5
        [HttpPost]
        public ActionResult Edit(HttpPostedFileBase file, FormCollection form, WSR_WorkingStandardEditModel model, string[] selected_trainWS)
        {
            try
            {
                // Convert selected_trainWS from array to List
                List<string> wstraining_selectedvalue = selected_trainWS != null ? selected_trainWS.ToList() : new List<string>();

                // Load options again for the dropdown/checkbox
                GetSelectOption_WS(model);

                var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    WSR_WorkingStandardEdit ws = db.WSR_WorkingStandardEdit
                        .Where(x => x.WS_Id == model.WS_Id)
                        .FirstOrDefault();

                    if (ws != null)
                    {
                        // Handle File Upload
                        if (file != null)
                        {
                            string wst_str = model.WST_Id > 0 ? model.WST_Id.ToString() : "000";
                            string wsp_str = model.WSP_Id > 0 ? model.WSP_Id.ToString() : "000";
                            string _FileName = $"{wst_str}{wsp_str}{model.WS_Number}{Path.GetExtension(file.FileName)}";
                            string _path = Path.Combine(path_ws, _FileName);

                            var fileExt = Path.GetExtension(file.FileName).Substring(1);
                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, only word, PDF, excel, text, and image files are allowed.";
                                return View(model);
                            }

                            file.SaveAs(_path);
                            ws.WS_File = _FileName;
                        }

                        // Update other fields
                        ws.WS_Date = DateTime.Now;
                        ws.WS_Name = $"{model.WS_Name_th}/{model.WS_Name_eng}";
                        ws.WS_Note = model.WS_Note;
                        ws.WS_Rev = model.WS_Rev;
                        ws.WS_Update = model.WS_Update;
                        ws.WST_Id = model.WST_Id;
                        ws.WSP_Id = model.WSP_Id;

                        db.Entry(ws).State = System.Data.Entity.EntityState.Modified;

                        // Update TrainingWS_temporary areas
                        List<string> oldArea = db.Emp_TrainingWS_temporary
                            .Where(x => x.Train_NumberWS == model.WS_Number && x.Trai_Rev == model.WS_Rev)
                            .Select(s => s.Trai_NameArea).ToList();

                        var oldArea_NotIn_newArea = oldArea.Except(wstraining_selectedvalue).ToList();
                        var newArea_NotIn_oldArea = wstraining_selectedvalue.Except(oldArea).ToList();

                        // Delete areas that are not in newArea
                        if (oldArea_NotIn_newArea.Any())
                        {
                            var oldArea_model = db.Emp_TrainingWS_temporary
                                .Where(x => x.Train_NumberWS == model.WS_Number
                                            && x.Trai_Rev == model.WS_Rev
                                            && oldArea_NotIn_newArea.Contains(x.Trai_NameArea)
                                            && x.Train_Status == 0)
                                .ToList();

                            db.Emp_TrainingWS_temporary.RemoveRange(oldArea_model);
                        }

                        // Add new areas
                        if (newArea_NotIn_oldArea.Any())
                        {
                            foreach (var area in newArea_NotIn_oldArea)
                            {
                                Emp_TrainingWS_temporary wstemp_new = new Emp_TrainingWS_temporary
                                {
                                    Train_NumberWS = model.WS_Number,
                                    Train_Header = model.WS_Name_eng,
                                    Train_HeaderThai = model.WS_Name_th,
                                    Train_DateWS = model.WS_Update.ToString("yyyy-MM-dd"),
                                    Train_Status = 0,
                                    Trai_NameArea = area,
                                    Trai_Rev = model.WS_Rev
                                };
                                db.Emp_TrainingWS_temporary.Add(wstemp_new);
                            }
                        }

                        // Save changes to the database
                        db.SaveChanges();
                        TempData["shortMessage"] = $"Successfully edited, {model.WS_Number}.";

                        return Json(new { success = true, message = "Successfully edited the Working Standard.", redirectUrl = Url.Action("WSSetList", new WorkingStandardSetModel { Page = 1, searchWorkingStandardNo = model.WS_Number }) });
                        //return RedirectToAction("WSSetList", new WorkingStandardSetModel { Page = 1, searchWorkingStandardNo = model.WS_Number });
                    }
                }

                ViewBag.Message = "Model is invalid.";
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
        // POST: /WorkingStandardSet/Delete/5
        [HttpPost]
        public ActionResult Delete(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var ws_list = db.WSR_WorkingStandardEdit.Where(x => id_list.Contains(x.WS_Id)).ToList();
                    foreach (WSR_WorkingStandardEdit ws in ws_list)
                    {
                        //Check WS course is use
                        var ws_course_temp_use_list = db.Emp_TrainingWS_temporary.Where(x => x.Train_NumberWS == ws.WS_Number && x.Train_Status != 0).ToList();
                        if (ws_course_temp_use_list != null && ws_course_temp_use_list.Any())
                        {
                            var ws_use_list = ws_course_temp_use_list.Select(s => s.Trai_NameArea).ToList();
                            string ws_use_str = String.Join(",", ws_use_list);

                            TempData["shortMessageError"] = String.Format("Can't delete {0} ,Because the WS training course (Area: {1} ) is already in use. ", ws.WS_Number, ws_use_str);
                            return RedirectToAction("WSSetList");
                        }
                        //Check WS course not use
                        var ws_course_temp_list = db.Emp_TrainingWS_temporary.Where(x => x.Train_NumberWS == ws.WS_Number && x.Train_Status == 0).ToList();
                        if (ws_course_temp_list != null && ws_course_temp_list.Any())
                        {   //Remove WS Training Temp
                            db.Emp_TrainingWS_temporary.RemoveRange(ws_course_temp_list);
                        }
                        //Remove WS
                        db.WSR_WorkingStandardEdit.Remove(ws);
                        //Remove File
                        string _path = Path.Combine(path_ws, ws.WS_File);
                        if (System.IO.File.Exists(_path))
                        {  // If file found, delete it    
                            System.IO.File.Delete(_path);
                            Console.WriteLine("File deleted.");
                        }
                        //Save Logs
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "delete",
                            Log_Type = "Working Standard",
                            Log_System = "WS & Rule",
                            Log_Detail = String.Concat(ws.WS_Number, "/"
                                        , ws.WS_Name, "/", ws.WS_Rev, "/"
                                        , ws.WST_Id, "/", ws.WSP_Id, "/"
                                        , ws.WS_File),
                            Log_Action_Id = ws.WS_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);

                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Successfully deleted, {0} .", ws.WS_Number);
                    }
                    TempData["shortMessage"] = String.Format("Successfully deleted, {0} items. ", id_list.Count());
                    return Json(new { success = true, message = "Successfully deleted." });
                    //return RedirectToAction("WSSetList");
                }
                return RedirectToAction("WSSetList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        public ActionResult DownloadFile(string fileName, string WsNumber)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["message"] = "The file name is incorrect.";
                return RedirectToAction("WSSetList");
            }
            //Build the File Path.
            //string path = Server.MapPath("~/File/") + fileName;  --visual path
            string path = path_ws + fileName;
            try
            {
                //Read the File data into Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                //string downloadName = string.IsNullOrEmpty(WsNumber) ? fileName : $"{WsNumber}{Path.GetExtension(fileName)}";

                string downloadName = string.IsNullOrEmpty(WsNumber) ? fileName : $"{WsNumber}{Path.GetExtension(fileName)}";

                //Send the File to Download.
                return File(bytes, "application/octet-stream", downloadName);
            }
            catch (IOException)
            {
                TempData["message"] = $"Could not find file: {fileName}";
                return RedirectToAction("WSSetList");
                //ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                //return View("Error");
            }
        }

        public void ExportToCsv(List<WSR_WorkingStandardEditModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new Export_WorkingStandardModel
                {
                    item = i + 1,
                    WS_Number = v.WS_Number,
                    WS_Rev = v.WS_Rev,
                    WS_Name = v.WS_Name.Replace("<br>", "/"),
                    UpDate_str = v.UpDate_str,
                    WorkingStandardType = v.WorkingStandardType,
                    WorkingStandardProcess = v.WorkingStandardProcess,
                    WS_Note = v.WS_Note

                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    "Item", "Working Standard No.", "Revision", "Working Standard Name"
                    , "Update", "Working Standard Type", "Working Standard Process", "Note", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        item.item, item.WS_Number, item.WS_Rev, item.WS_Name, item.UpDate_str,
                        item.WorkingStandardType, item.WorkingStandardProcess, item.WS_Note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=WorkingStandard.CSV ");
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
        //  /WorkingStandardSet/SendEmailWS/
        public ActionResult SendEmailWS()
        {
            try
            {
                SendMailCenterModel model = new SendMailCenterModel();
                //Send mail 
                int idgroup = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_WS").Select(s => s.USG_Id).FirstOrDefault();
                var userlist = db.GroupMembers.Where(x => x.USG_Id == idgroup).Select(x => x.USE_Id).ToList();
                var tomail = db.UserDetails.Where(x => userlist.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();
                if (tomail == null || !tomail.Any())
                {
                    TempData["shortMessage"] = String.Format("Send email failed, User group name 'SEND_MAIL_WS' is null. ");
                    return RedirectToAction("WSSetList");
                }
                //Send mail CC
                int idgroup_cc = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_WS_CC").Select(s => s.USG_Id).FirstOrDefault();
                var userlist_cc = db.GroupMembers.Where(x => x.USG_Id == idgroup_cc).Select(x => x.USE_Id).ToList();
                var tomail_cc = db.UserDetails.Where(x => userlist_cc.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();

                var callbackUrl = Url.Action("WSTrainingList", "TrainingCourseAddWS", "", protocol: Request.Url.Scheme);

                string html = "<strong style='font-size: 20px'> Dear HR Department, </strong>"
                             + "<p style='font-size: 18px'>Now. Have a new /revise WS to register&training on OCT WEB "
                             + "(Please send email to head department concern for upload training)</p>"
                             + "<p style='font-size: 20px'>เนื่องจาก มีการลงทะเบียน/แก้ไข อบรมขั้นตอนการปฏิบัติงาน  บนระบบ OCTWEB, "
                             + "กรุณาส่งอีเมล์ แจ้งหัวหน้าของแผนกนั้นๆ ให้ทำการอัพโหลดการอบรมตามหลักสูตร WS ด้วยค่ะ - "
                             + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"
                             + "<p><strong>*Do Not Reply This E-Mail</strong></p><br/><br/><br/><br/>"
                             + "<p style='font-size: 18px'>Best Regards,<br/>"
                             + "OCT WEB</p>";

                model.To = tomail;
                model.Tocc = tomail_cc;
                model.Subject = "Update traning working standard";
                model.Body = html;

                SendMailCenterController.SendMail(model);

                TempData["shortMessage"] = String.Format("Successfully send mail, To {0} ", String.Join(", ", tomail));

                return RedirectToAction("WSSetList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }

        }

        //
        //  /WorkingStandardSet/SendEmailWS_with_wsnumber/
        public ActionResult SendEmailWS_with_wsnumber(WSTrainingForSendEmailModel wstrain)
        {
            try
            {
                SendMailCenterModel model = new SendMailCenterModel();
                //Send mail 
                int idgroup = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_WS").Select(s => s.USG_Id).FirstOrDefault();
                var userlist = db.GroupMembers.Where(x => x.USG_Id == idgroup).Select(x => x.USE_Id).ToList();
                var tomail = db.UserDetails.Where(x => userlist.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();
                if (tomail == null || !tomail.Any())
                {
                    TempData["shortMessage"] = String.Format("Send email failed, User group name 'SEND_MAIL_WS' is null. ");
                    return RedirectToAction("WSSetList");
                }
                //Send mail CC
                int idgroup_cc = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_WS_CC").Select(s => s.USG_Id).FirstOrDefault();
                var userlist_cc = db.GroupMembers.Where(x => x.USG_Id == idgroup_cc).Select(x => x.USE_Id).ToList();
                var tomail_cc = db.UserDetails.Where(x => userlist_cc.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();

                var callbackUrl = Url.Action("WSTrainingList", "TrainingCourseAddWS", "", protocol: Request.Url.Scheme);

                string html = "<strong style='font-size: 20px'> Dear HR Department, </strong>"
                             + "<p style='font-size: 18px'>Now. Have a new /revise WS to register&training on OCT WEB "
                             + "(Please send email to head department concern for upload training)</p>"
                             + "<p style='font-size: 20px'>เนื่องจาก มีการลงทะเบียน/แก้ไข อบรมขั้นตอนการปฏิบัติงาน  บนระบบ OCTWEB, "
                             + "กรุณาส่งอีเมล์ แจ้งหัวหน้าของแผนกนั้นๆ ให้ทำการอัพโหลดการอบรมตามหลักสูตร WS ด้วยค่ะ - "
                             + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"
                             + "<table style='font-size:18px'>"
                             + "<tr><td>WS Number : </td><td>" + wstrain.Train_NumberWS + "</td></tr>"
                             + "<tr><td>WS Name : </td><td>" + wstrain.Train_Header + " " + wstrain.Train_HeaderThai + "</td></tr>"
                             + "<tr><td>Line Name : </td><td>" + wstrain.Trai_NameArea + "</td></tr>"
                             + "<tr><td>Registration WS Date : </td><td>" + wstrain.Train_DateWS + "</td></tr></table><br/>"

                             + "<p><strong>*Do Not Reply This E-Mail!</strong></p><br/><br/><br/><br/>"
                             + "<p style='font-size: 18px'>Best Regards,<br/>"
                             + "OCT WEB</p>";

                model.To = tomail;
                model.Tocc = tomail_cc;
                model.Subject = "Update traning working standard";
                model.Body = html;

                SendMailCenterController.SendMail(model);

                TempData["shortMessage"] = String.Format("Successfully send mail, To {0} ", String.Join(", ", tomail));

                return RedirectToAction("WSSetList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }

        }

        //
        // POST: /WorkingStandardSet/GetWorkingStandardNo/
        [HttpPost]
        public JsonResult GetWorkingStandardNo(string Prefix)
        {
            var wsno = (from ws in db.WSR_WorkingStandardEdit
                        where ws.WS_Number.StartsWith(Prefix)
                        select new { label = ws.WS_Number, val = ws.WS_Number }).Take(10).ToList();
            return Json(wsno);
        }
        //
        // POST: /WorkingStandardSet/GetWorkingStandardName/
        [HttpPost]
        public JsonResult GetWorkingStandardName(string Prefix)
        {
            var wsname = db.WSR_WorkingStandardEdit
                            .Select(s => new { WS_Name = s.WS_Name.Replace("/", "") })
                            .Where(x => x.WS_Name.StartsWith(Prefix))
                            .Take(10)
                            .Select(s => new { label = s.WS_Name, val = s.WS_Name }).ToList();
            return Json(wsname);
        }
        // /WorkingStandardSet/GetDepartment/
        public JsonResult GetDepartment()
        {
            var select_departments = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                                .Select(s => new { label = s.Dep_Name, val = s.Dep_Id }).ToList();
            return Json(select_departments, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardSet/GetWSType/
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

        // /WorkingStandardSet/GetWSProcess/
        [HttpPost]
        public JsonResult GetWSProcess(int? department, string wstype)
        {
            IEnumerable<WSR_WorkingStandardTypeEdit> query = db.WSR_WorkingStandardTypeEdit;
            if (department.HasValue)
            {
                query = query.Where(x => x.WST_Dept == department.Value).ToList();
            }
            if (!String.IsNullOrEmpty(wstype))
            {
                query = query.Where(x => !String.IsNullOrEmpty(x.WST_Type) && x.WST_Type == wstype).ToList();
            }
            List<int> wst_id_list = query.Select(s => s.WST_Id).ToList();
            var select_wsp = db.WSR_WorkingStandardProcessEdit.Where(x => wst_id_list.Contains(x.WST_Id))
                            .Select(s => s.WSP_ProcessName).Distinct().OrderBy(o => o)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(select_wsp, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardSet/GetWSTypeCreate/
        [HttpPost]
        public JsonResult GetWSTypeCreate(int department)
        {
            var select_wst = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Dept == department)
                            .Select(s => new { label = s.WST_Type, val = s.WST_Id }).ToList();
            return Json(select_wst, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardSet/GetWSProcessCreate/
        [HttpPost]
        public JsonResult GetWSProcessCreate(int wstype)
        {
            var select_wsp = db.WSR_WorkingStandardProcessEdit.Where(x => x.WST_Id == wstype)
                            .Select(s => new { label = s.WSP_ProcessName, val = s.WSP_Id }).ToList();
            return Json(select_wsp, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
