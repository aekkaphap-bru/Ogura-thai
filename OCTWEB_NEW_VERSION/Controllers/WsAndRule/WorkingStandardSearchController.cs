using OCTWEB_NET45.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OCTWEB_NET45.Models;
using PagedList;
using System.IO;
using System.Text;
using OCTWEB_NET45.Infrastructure;
using System.Configuration;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class WorkingStandardSearchController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_ws = ConfigurationManager.AppSettings["path_ws"];

        //
        // GET: /WorkingStandardSearch/WorkingStandardList/
        [CustomAuthorize(22)]//Working standard Search/Report/WS & Rule
        public ActionResult WorkingStandardList(WorkigStandardSearchModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                int? searchDepartmentId = model.searchDepartmentId.HasValue ? model.searchDepartmentId : null;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;
                string searchWorkingStandardName = model.searchWorkingStandardName;
                string searchWorkingStandardNo = model.searchWorkingStandardNo;
                int? searchSortBy = model.searchSortBy.HasValue ? model.searchSortBy : null;

                //Set data update latest
                var date_latest = db.WSR_WorkingStandardEdit.OrderByDescending(o => o.WS_Date).Take(1).Select(s => s.WS_Date).FirstOrDefault();
                string date_str = date_latest.ToString("MMMM dd yyyy HH:mm tt");
                model.date_str = date_str;
             
                IEnumerable<WSR_WorkingStandardEdit> query = model.Page.HasValue ? db.WSR_WorkingStandardEdit : db.WSR_WorkingStandardEdit.Take(0);
                if (searchDepartmentId.HasValue)
                {
                    List<int> wst_id_list = db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Dept == searchDepartmentId.Value)
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
                            .ThenBy(s => s.WorkingStandardProcess).ThenBy(s => s.WS_Name).ToList();;
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
                           .ToList();;
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

                    if(rights_boi <= 0)
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
                    workingStandardList.Where(x=>dept_id.Contains(x.dept_id)).ToList().ForEach(f=>f.WS_File = "");
                }

                IPagedList<WSR_WorkingStandardEditModel> workingStandardPagedList = workingStandardList.ToPagedList(pageIndex, pageSize);

                
                model.WSR_WorkingStandardModelPagedList = workingStandardPagedList;

                GetSelectOptions(model);

                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }           
        }

        public WorkigStandardSearchModel GetSelectOptions(WorkigStandardSearchModel model)
        {

            //Select department
            List<SelectListItem> SelectDepartment_list = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

            // Select working standard type
            IEnumerable<WSR_WorkingStandardTypeEdit> query_wst = db.WSR_WorkingStandardTypeEdit;
            query_wst = model.searchDepartmentId.HasValue ? query_wst.Where(w => w.WST_Dept == model.searchDepartmentId).ToList()
                : query_wst;
            List<SelectListItem> SelectWorkingStandardType_list = query_wst.Select(s=>s.WST_Type).Distinct().OrderBy(o => o)
                .Select(s => new SelectListItem { Value = s, Text = s }).ToList();

            //select working standard process
            IEnumerable<WSR_WorkingStandardProcessEdit> query_wsp = db.WSR_WorkingStandardProcessEdit;
            List<int> wst_id_list_2 = new List<int>();
            wst_id_list_2 = query_wst.Select(s => s.WST_Id).ToList();
            /*List<int> wsp_id_list_2 = !String.IsNullOrEmpty(model.searchWorkingStandardType) ?
                db.WSR_WorkingStandardTypeEdit.Where(x => x.WST_Type == model.searchWorkingStandardType).Select(s => s.WST_Id).ToList() : new List<int>();*/

            /*query_wsp = wsp_id_list_2.Any() ? query_wsp.Where(w => wsp_id_list_2.Contains(w.WST_Id)).ToList()
                : query_wsp;*/
            if (query_wsp.Any())
            {
                query_wsp = query_wsp.Where(w => wst_id_list_2.Contains(w.WST_Id)).ToList();
            }
            if (!String.IsNullOrEmpty(model.searchWorkingStandardType))
            {
                int wst_id = query_wst.Where(w => w.WST_Type == model.searchWorkingStandardType).Select(s => s.WST_Id).FirstOrDefault();
                query_wsp = query_wsp.Where(w => w.WST_Id == wst_id).ToList();
            }
            List<SelectListItem> SelectWorkingStandardProcess_list = query_wsp.Select(s=>s.WSP_ProcessName).Distinct().OrderBy(o => o)
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

        //
        // POST: /WorkingStandardSearch/WorkingStandardList/
        [HttpPost]
        [CustomAuthorize(22)]//Working standard Search/Report/WS & Rule
        public ActionResult WorkingStandardList(FormCollection form,WorkigStandardSearchModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;

                int? searchDepartmentId = model.searchDepartmentId.HasValue ? model.searchDepartmentId : null;
                string searchWorkingStandardType = model.searchWorkingStandardType;
                string searchWorkingStandardProcess = model.searchWorkingStandardProcess;
                string searchWorkingStandardName = model.searchWorkingStandardName;
                string searchWorkingStandardNo = model.searchWorkingStandardNo;
                int? searchSortBy = model.searchSortBy.HasValue ? model.searchSortBy : null;

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

                IPagedList<WSR_WorkingStandardEditModel> workingStandardPagedList = workingStandardList.ToPagedList(pageIndex, pageSize);
               
                model.WSR_WorkingStandardModelPagedList = workingStandardPagedList;

                GetSelectOptions(model);
              
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("POST: /WorkingStandardSearch/WorkingStandardList/ {0}", ex.ToString());
                return View("Error");
            }
        }
      
        //
        // POST: /WorkingStandardSearch/GetWorkingStandardNo/
        [HttpPost]
        public JsonResult GetWorkingStandardNo(string Prefix)
        {
            var wsno = (from ws in db.WSR_WorkingStandardEdit
                        where ws.WS_Number.StartsWith(Prefix)
                        select new { label = ws.WS_Number, val = ws.WS_Number }).Take(20).ToList();
            return Json(wsno);
        }
        //
        // POST: /WorkingStandardSearch/GetWorkingStandardName/
        [HttpPost]
        public JsonResult GetWorkingStandardName(string Prefix)
        {
            var wsname = db.WSR_WorkingStandardEdit
                            .Select(s=> new{WS_Name= s.WS_Name.Replace("/","") })
                            .Where(x => x.WS_Name.StartsWith(Prefix))
                            .Take(20)
                            .Select(s => new { label = s.WS_Name, val = s.WS_Name }).ToList();
            return Json(wsname);
        }
        // /WorkingStandardSearch/GetDepartment/
        public JsonResult GetDepartment()
        {
            var select_departments = db.DepartmentEdits.OrderBy(o => o.Dep_Name)
                                .Select(s => new { label = s.Dep_Name, val = s.Dep_Id }).ToList();
            return Json(select_departments, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardSearch/GetWSType/
        [HttpPost]
        public JsonResult GetWSType(int? department)
        {
            IEnumerable<WSR_WorkingStandardTypeEdit> query = db.WSR_WorkingStandardTypeEdit;                
            if(department.HasValue)
            {
                query = query.Where(x => x.WST_Dept == department.Value).ToList();
            }
            var select_wst = query.Select(s=>s.WST_Type).Distinct().OrderBy(o=>o)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(select_wst, JsonRequestBehavior.AllowGet);
        }

        // /WorkingStandardSearch/GetWSProcess/
        [HttpPost]
        public JsonResult GetWSProcess(int? department, string wstype)
        {
            IEnumerable<WSR_WorkingStandardTypeEdit> query = db.WSR_WorkingStandardTypeEdit;                
            if(department.HasValue)
            {
                query = query.Where(x => x.WST_Dept == department.Value).ToList();
            }
            if(!String.IsNullOrEmpty(wstype))
            {
                query = query.Where(x => !String.IsNullOrEmpty(x.WST_Type) && x.WST_Type == wstype).ToList();
            }
            List<int> wst_id_list = query.Select(s => s.WST_Id).ToList();
            var select_wsp = db.WSR_WorkingStandardProcessEdit.Where(x=> wst_id_list.Contains(x.WST_Id))
                            .Select(s=>s.WSP_ProcessName).Distinct().OrderBy(o=>o)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(select_wsp, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(22)]//Working standard Search/Report/WS & Rule
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
                    WS_Note = "\""+ v.WS_Note + "\""
                    
                });
                
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    "Item","Working Standard No.","Revision","Working Standard Name"
                    ,"Update","Working Standard Type","Working Standard Process","Note", Environment.NewLine);

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

        [CustomAuthorize(22)] // Working standard Search/Report/WS & Rule
        public ActionResult DownloadFile(string fileName, string WsNumber)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["message"] = "ชื่อไฟล์ไม่ถูกต้อง";
                return RedirectToAction("WorkingStandardList");
            }

            string path = path_ws + fileName;

            try
            {
                byte[] filebytes = System.IO.File.ReadAllBytes(path);

                // กำหนดชื่อไฟล์ที่จะดาวน์โหลดเป็น WsNumber แทน
                string downloadName = string.IsNullOrEmpty(WsNumber) ? fileName : $"{WsNumber}{Path.GetExtension(fileName)}";

                return File(filebytes, "application/octet-stream", downloadName);
            }
            catch (IOException)
            {
                TempData["message"] = $"ไม่พบไฟล์: {fileName}";
                return RedirectToAction("WorkingStandardList");
            }
        }



        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }



    
    }
}
