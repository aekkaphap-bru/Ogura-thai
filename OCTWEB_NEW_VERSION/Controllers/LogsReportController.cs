using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;
using OCTWEB_NET45.Infrastructure;
using System.Text.RegularExpressions;


namespace OCTWEB_NET45.Controllers
{
    [Authorize]
    public class LogsReportController : Controller
    {

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
     
        //
        // GET: /LogsReport/logDetailsList
        [CustomAuthorize(10)]//Log Report/Setup/Administration
        public ActionResult logDetailsList(LogsReportPagedListModel model)
        {
            try
            {
                   
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;      

                ViewBag.SystemSortParm = String.IsNullOrEmpty(model.sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = model.sortOrder == "Date" ? "date_desc" : "Date";

                if(!model.Page.HasValue)
                {
                     DateTime? _SetDefaultDate_fromdate = DateTime.Now.AddDays(-30);
                     DateTime? _SetDefaultDate_todate = DateTime.Now;
                     model.fromdate = _SetDefaultDate_fromdate;
                     model.todate = _SetDefaultDate_todate;
                }

                string searchsystem = model.searchSystem;
                string searchaction = model.searchAction;
                string searchtype = model.searchType;
                string searchDetail = model.searchDetail;
                DateTime? fromdate = model.fromdate;
                DateTime? todate = model.todate;
               
                IEnumerable<Log> query = model.Page.HasValue ? db.Logs : db.Logs.Take(0);

                if(fromdate.HasValue)
                {
                    DateTime fd = new DateTime(fromdate.Value.Year, fromdate.Value.Month, fromdate.Value.Day, 0, 0, 0);
                    query = query.Where(x => x.Log_Date >= fd);
                }
                if (todate.HasValue)
                {
                    DateTime td = new DateTime(todate.Value.Year, todate.Value.Month, todate.Value.Day, 23, 59, 59);
                    query = query.Where(x => x.Log_Date <= td);
                }
                if (!String.IsNullOrEmpty(searchsystem))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_System)
                        && x.Log_System.ToLowerInvariant().Contains(searchsystem.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchtype))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Type)
                        && x.Log_Type.ToLowerInvariant().Contains(searchtype.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchaction))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Action)
                        && x.Log_Action.ToLowerInvariant().Contains(searchaction.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchDetail))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Detail)
                        && x.Log_Detail.Replace("/", "").Replace("@", "").Contains(searchDetail.Replace("/", "").Replace("@", "")));
                }

                var logsList = query.Select(s => new LogsReportModel
                {
                    Log_Action = s.Log_Action,
                    Log_Action_Id = s.Log_Action_Id,
                    Log_by = s.Log_by,
                    Log_Date = s.Log_Date,
                    Log_Detail = s.Log_Detail,
                    Log_Id = s.Log_Id,
                    Log_System = s.Log_System,
                    Log_Type = s.Log_Type
                });

                switch (model.sortOrder)
                {
                    case "name_desc":
                        logsList = logsList.OrderByDescending(s => s.Log_System);
                        break;
                    case "Date":
                        logsList = logsList.OrderBy(s => s.Log_Date);
                        break;
                    case "date_desc":
                        logsList = logsList.OrderByDescending(s => s.Log_Date);
                        break;
                    default:
                        logsList = logsList.OrderByDescending(s => s.Log_Date);
                        break;
                }

               
                IPagedList<LogsReportModel> logsPagedList = logsList.ToPagedList(pageIndex, pageSize);  
                model.LogsReportModelPagedList = logsPagedList;
  
                return View(model);

            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }           
        }

        //
        // POST: /LogsReport/logDetailsList
        [HttpPost]
        [CustomAuthorize(10)]//Log Report/Setup/Administration
        public ActionResult logDetailsList(FormCollection form, LogsReportPagedListModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                string searchsystem = model.searchSystem;
                string searchaction = model.searchAction;
                string searchtype = model.searchType;
                string searchDetail = model.searchDetail;
                DateTime? fromdate = model.fromdate;
                DateTime? todate = model.todate;

                IEnumerable<Log> query = db.Logs;
                
                if (fromdate.HasValue)
                {
                    DateTime fd = new DateTime(fromdate.Value.Year, fromdate.Value.Month, fromdate.Value.Day, 0, 0, 0);
                    query = query.Where(x => x.Log_Date >= fd);
                }
                if (todate.HasValue)
                {
                    DateTime td = new DateTime(todate.Value.Year, todate.Value.Month, todate.Value.Day, 23, 59, 59);
                    query = query.Where(x => x.Log_Date <= td);
                }
                if (!String.IsNullOrEmpty(searchsystem))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_System)
                        && x.Log_System.ToLowerInvariant().Contains(searchsystem.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchtype))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Type)
                        && x.Log_Type.ToLowerInvariant().Contains(searchtype.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchaction))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Action)
                        && x.Log_Action.ToLowerInvariant().Contains(searchaction.ToLowerInvariant()));
                }
                if (!String.IsNullOrEmpty(searchDetail))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.Log_Detail)
                        && x.Log_Detail.Replace("/", "").Replace("@", "").Contains(searchDetail.Replace("/", "").Replace("@", "")));
                }

                var logsList = query.Select(s => new LogsReportModel
                {
                    Log_Action = s.Log_Action,
                    Log_Action_Id = s.Log_Action_Id,
                    Log_by = s.Log_by,
                    Log_Date = s.Log_Date,
                    Log_Detail = s.Log_Detail,
                    Log_Id = s.Log_Id,
                    Log_System = s.Log_System,
                    Log_Type = s.Log_Type
                });

                logsList = logsList.OrderByDescending(s => s.Log_Date);

                IPagedList<LogsReportModel> logsPagedList = logsList.ToPagedList(pageIndex, pageSize);
               
                model.LogsReportModelPagedList = logsPagedList;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }
            
        }

        //
        //POST: /LogsReport/GetSystem
        [HttpPost]
        public JsonResult GetSystem(string Prefix)
        {
            var sys = db.Logs.Where(x => !String.IsNullOrEmpty(x.Log_System)
                && x.Log_System.StartsWith(Prefix)).Select(s => s.Log_System).Distinct()
                .Select(l => new { label = l, val = l }).ToList();
            return Json(sys);
        }
        //
        //POST: /LogsReport/GetType
        [HttpPost]
        public JsonResult GetType(string Prefix)
        {
            var type = db.Logs.Where(x => !String.IsNullOrEmpty(x.Log_Type)
                && x.Log_Type.StartsWith(Prefix)).Select(s => s.Log_Type).Distinct()
                .Select(l => new { label = l, val = l }).ToList();
            return Json(type);
        }
        //
        //POST: /LogsReport/GetAction
        [HttpPost]
        public JsonResult GetAction(string Prefix)
        {
            var act = db.Logs.Where(x => !String.IsNullOrEmpty(x.Log_Action)
                && x.Log_Action.StartsWith(Prefix)).Select(s => s.Log_Action).Distinct()
                .Select(l => new { label = l, val = l }).ToList();
            return Json(act);
        }
        //
        //POST: /LogsReport/GetDetail
        [HttpPost]
        public JsonResult GetDetail(string Prefix)
        {
            var detail = db.Logs.Where(x => !String.IsNullOrEmpty(x.Log_Detail)
                && x.Log_Detail.StartsWith(Prefix)).Select(s => s.Log_Detail).Distinct()
                .Select(l => new { label = l, val = l }).ToList();
            return Json(detail);
        }


        public LogsReportModel SaveLogs(LogsReportModel model)
        {
            try
            {
                Log logmodel = new Log()
                {
                    Log_Action = model.Log_Action,
                    Log_Type = model.Log_Type,
                    Log_System = model.Log_System,
                    Log_Detail = model.Log_Detail,
                    Log_Action_Id = model.Log_Action_Id,
                    Log_Date = DateTime.Now,
                    Log_by = model.Log_by
                };
                var result = db.Logs.Add(logmodel);
                db.SaveChanges();

                return model;
            }
            catch{
               // ModelState.AddModelError("", ex.InnerException.Message);
                return model;
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
