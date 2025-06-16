using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using PagedList;
using System.IO;
using System.Text;
using OCTWEB_NET45.Infrastructure;
using System.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class CompanyRuleSearchController : Controller
    {

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_ws_company_rule = ConfigurationManager.AppSettings["path_ws_company_rule"];

        //
        // GET: /CompanyRuleSearch/CompanyRuleList/
        [CustomAuthorize(19)]//Company rule Search/Report/WS & Rule
        public ActionResult CompanyRuleList(CompanyRuleSearchModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
                pageIndex = model.Page.HasValue ? Convert.ToInt32(model.Page) : 1;

                int? searchCompanyRuleTypeId = null;
                string searchCompanyRuleName = null;
                string searchCompanyRuleNo = null;

                if (model.searchCompanyRuleTypeId.HasValue)
                {
                    searchCompanyRuleTypeId = model.searchCompanyRuleTypeId;
                }
                if (!string.IsNullOrEmpty(model.searchCompanyRuleName))
                {
                    searchCompanyRuleName = model.searchCompanyRuleName;
                }
                if (!string.IsNullOrEmpty(model.searchCompanyRuleNo))
                {
                    searchCompanyRuleNo = model.searchCompanyRuleNo;
                }

                IEnumerable<WSR_CompRule> query = model.Page.HasValue ? db.WSR_CompRule : db.WSR_CompRule;

                if (searchCompanyRuleTypeId.HasValue)
                {
                    query = query.Where(x => x.CRT_Id == searchCompanyRuleTypeId);
                }
                if (!string.IsNullOrEmpty(searchCompanyRuleName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Name) 
                        && x.CR_Name.Replace("/","").ToLowerInvariant().Contains(searchCompanyRuleName.ToLowerInvariant())));                   
                }
                if (!string.IsNullOrEmpty(searchCompanyRuleNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Number) && x.CR_Number.ToLowerInvariant().Contains(searchCompanyRuleNo.ToLowerInvariant())));                   
                }

                var comruleList = query.Select(s => new WSR_CompRuleModel
                {
                    CR_Date = s.CR_Date.ToString("MM/dd/yyyy"),
                    CR_File = s.CR_File,
                    CR_Id = s.CR_Id,
                    CR_Name = s.CR_Name.Replace("/", "<br>"),
                    CR_Note = s.CR_Note,
                    CR_Number = s.CR_Number,
                    CR_Rev = s.CR_Rev,
                    CR_Update = s.CR_Update.ToString("MM/dd/yyyy"),
                    CRT_Id = s.CRT_Id,
                    CompRuleType = (from crt in db.WSR_CompRuleType where crt.CRT_Id == s.CRT_Id select crt.CRT_Type ).FirstOrDefault()
                
                }).OrderBy(o => o.CR_Number).ToList();

                IPagedList<WSR_CompRuleModel> comrulePagedList = comruleList.ToPagedList(pageIndex, pageSize);

                List<WSR_CompRuleType> companyRuleTypeList = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type).ToList();
                List<SelectListItem> SelectCompanyRuleType_list = companyRuleTypeList
                    .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();
                 
                model.WSR_CompRuleModelPagedList = comrulePagedList;
                model.SelectCompanyRuleType = SelectCompanyRuleType_list;
                
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
        // POST: /CompanyRuleSearch/CompanyRuleList/
        [HttpPost]
        [CustomAuthorize(19)]//Company rule Search/Report/WS & Rule
        public ActionResult CompanyRuleList(FormCollection form,CompanyRuleSearchModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                int? searchCompanyRuleTypeId = null;
                string searchCompanyRuleName = null;
                string searchCompanyRuleNo = null;

                if (model.searchCompanyRuleTypeId.HasValue)
                {
                    searchCompanyRuleTypeId = model.searchCompanyRuleTypeId;
                }
                if (!string.IsNullOrEmpty(model.searchCompanyRuleName))
                {
                    searchCompanyRuleName = model.searchCompanyRuleName;
                }
                if (!string.IsNullOrEmpty(model.searchCompanyRuleNo))
                {
                    searchCompanyRuleNo = model.searchCompanyRuleNo;
                }

                IEnumerable<WSR_CompRule> query = db.WSR_CompRule;

                if (searchCompanyRuleTypeId.HasValue)
                {
                    query = query.Where(x => x.CRT_Id == searchCompanyRuleTypeId);
                }
                if (!string.IsNullOrEmpty(searchCompanyRuleName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Name) 
                        && x.CR_Name.Replace("/","").ToLowerInvariant().Contains(searchCompanyRuleName.ToLowerInvariant())));
                }
                if (!string.IsNullOrEmpty(searchCompanyRuleNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Number) && x.CR_Number.ToLowerInvariant().Contains(searchCompanyRuleNo.ToLowerInvariant())));
                }

                var comruleList = query.Select(s => new WSR_CompRuleModel
                {
                    CR_Date = s.CR_Date.ToString("MM/dd/yyyy"),
                    CR_File = s.CR_File,
                    CR_Id = s.CR_Id,
                    CR_Name = s.CR_Name.Replace("/", "<br>"),
                    CR_Note = s.CR_Note,
                    CR_Number = s.CR_Number,
                    CR_Rev = s.CR_Rev,
                    CR_Update = s.CR_Update.ToString("MM/dd/yyyy"),
                    CRT_Id = s.CRT_Id,
                    CompRuleType = (from crt in db.WSR_CompRuleType where crt.CRT_Id == s.CRT_Id select crt.CRT_Type ).FirstOrDefault()
                
                }).OrderBy(o => o.CR_Number).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(comruleList);
                }

                IPagedList<WSR_CompRuleModel> comrulePagedList = comruleList.ToPagedList(pageIndex, pageSize);

                List<WSR_CompRuleType> companyRuleTypeList = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type).ToList();
                List<SelectListItem> SelectCompanyRuleType_list = companyRuleTypeList
                    .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();
               
                model.WSR_CompRuleModelPagedList = comrulePagedList;
                model.SelectCompanyRuleType = SelectCompanyRuleType_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post /CompanyRuleSearch/CompanyRuleList {0}", ex.ToString());
                return View("Error");
            }
        }


        [CustomAuthorize(19)]//Company rule Search/Report/WS & Rule
        public ActionResult DownloadFile(string fileName, string RuleNumber)
        {          
            //Build the File Path.
            //string path = Server.MapPath("~/File/") + fileName;  --visual path

            if (string.IsNullOrEmpty(fileName))
            {
                TempData["message"] = "";
                return RedirectToAction("CompanyRuleList");
                  
            }
            string path = path_ws_company_rule + fileName;
            try
            {
                //Read the File data into Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                string downloadName = string.IsNullOrEmpty(RuleNumber) ? fileName : $"{RuleNumber}{Path.GetExtension(fileName)}"; 

                //Send the File to Download.
                return File(bytes, "application/octet-stream", downloadName);
            }
            catch (IOException)
            {
                TempData["message"] = $"Could not find file: {fileName}";
                //ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                return RedirectToAction("CompanyRuleList");
            }
           
        }

        public void ExportToCsv(List<WSR_CompRuleModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new WSR_CompRuleModel
                {
                    item = i + 1,
                    CR_File = v.CR_File,
                    CR_Number = v.CR_Number,
                    CR_Rev = v.CR_Rev,
                    CompRuleType = v.CompRuleType,
                    CR_Name = v.CR_Name.Replace("<br>", "/"),
                    CR_Note = "\"" + v.CR_Note + "\"", 
                    CR_Update = v.CR_Update

                });
                 
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    "Item", "File", "Company rule No.", "Revision", "Company rule type name",
                    "Company rule name", "Note", "Update", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        item.item, item.CR_File, item.CR_Number, item.CR_Rev, item.CompRuleType,
                        item.CR_Name, item.CR_Note, item.CR_Update, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CompanyRuleReport.CSV ");
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
        // POST: /CompanyRuleSearch/GetComRuleNo/
        [HttpPost]
        public JsonResult GetComRuleNo(string Prefix)
        {
            var crno = (from cr in db.WSR_CompRule
                        where cr.CR_Number.StartsWith(Prefix)
                        select new { label = cr.CR_Number, val = cr.CR_Number }).Take(10).ToList();
            return Json(crno);
        }
        //
        // POST: /CompanyRuleSearch/GetComRuleName/
        [HttpPost]
        public JsonResult GetComRuleName(string Prefix)
        {
            var crname = db.WSR_CompRule
                            .Select(s => new { CR_Name = s.CR_Name.Replace("/", "") })
                            .Where(x => x.CR_Name.StartsWith(Prefix))
                            .Take(10)
                            .Select(s => new { label = s.CR_Name, val = s.CR_Name }).ToList();
            return Json(crname);
        }
      
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
