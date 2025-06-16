using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class CompanyRuleSetController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private string path_ws_company_rule = ConfigurationManager.AppSettings["path_ws_company_rule"];
       
        //
        // GET: /CompanyRuleSet/CompanyRuleSetList/
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult CompanyRuleSetList(CompanyRuleSetModel model)
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

                int? searchCompanyRuleTypeId = null;
                string searchCompanyRuleName = null;
                string searchCompanyRuleNo = null;

                searchCompanyRuleTypeId = model.searchCompanyRuleTypeId.HasValue ? 
                    model.searchCompanyRuleTypeId : null;
                searchCompanyRuleName = !String.IsNullOrEmpty(model.searchCompanyRuleName) ? 
                    model.searchCompanyRuleName : null;
                searchCompanyRuleNo = !String.IsNullOrEmpty(model.searchCompanyRuleNo) ? 
                    model.searchCompanyRuleNo : null;

                IEnumerable<WSR_CompRule> query = model.Page.HasValue ? db.WSR_CompRule : db.WSR_CompRule;
                if (searchCompanyRuleTypeId.HasValue)
                {
                    query = query.Where(x => x.CRT_Id == searchCompanyRuleTypeId);
                }
                if (!String.IsNullOrEmpty(searchCompanyRuleName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Name) 
                        && x.CR_Name.Replace("/","").ToLowerInvariant().Contains(searchCompanyRuleName.ToLowerInvariant())));                          
                }
                if (!String.IsNullOrEmpty(searchCompanyRuleNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Number) && x.CR_Number.ToLowerInvariant().Contains(searchCompanyRuleNo.ToLowerInvariant())));
                }
                var comruleList = query.Select(s => new WSR_CompRuleModel
                {
                    CR_Date = s.CR_Date.ToString("dd/MM/yyyy"),
                    CR_File = s.CR_File,
                    CR_Id = s.CR_Id,
                    CR_Name = s.CR_Name.Replace("/", "<br>"),
                    CR_Note = s.CR_Note,
                    CR_Number = s.CR_Number,
                    CR_Rev = s.CR_Rev,
                    CR_Update = s.CR_Update.ToString("dd/MM/yyyy"),
                    CRT_Id = s.CRT_Id,
                    CompRuleType = (from crt in db.WSR_CompRuleType where crt.CRT_Id == s.CRT_Id select crt.CRT_Type).FirstOrDefault()

                }).OrderBy(o => o.CR_Number).ToList();

                IPagedList<WSR_CompRuleModel> comrulePagedList = comruleList.ToPagedList(pageIndex, pageSize);

                List<SelectListItem> SelectCompanyRuleType_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
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
        // POST: /CompanyRuleSet/CompanyRuleSetList/
        [HttpPost]
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult CompanyRuleSetList(FormCollection form,CompanyRuleSetModel model)
        {
            try
            {

                int pageSize = 30;
                int pageIndex = 1;
                
                int? searchCompanyRuleTypeId = null;
                string searchCompanyRuleName = null;
                string searchCompanyRuleNo = null;

                searchCompanyRuleTypeId = model.searchCompanyRuleTypeId.HasValue ?
                    model.searchCompanyRuleTypeId : null;
                searchCompanyRuleName = !String.IsNullOrEmpty(model.searchCompanyRuleName) ?
                    model.searchCompanyRuleName : null;
                searchCompanyRuleNo = !String.IsNullOrEmpty(model.searchCompanyRuleNo) ?
                    model.searchCompanyRuleNo : null;

                IEnumerable<WSR_CompRule> query = db.WSR_CompRule;
                if (searchCompanyRuleTypeId.HasValue)
                {
                    query = query.Where(x => x.CRT_Id == searchCompanyRuleTypeId);
                }
                if (!String.IsNullOrEmpty(searchCompanyRuleName))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Name) 
                        && x.CR_Name.Replace("/","").ToLowerInvariant().Contains(searchCompanyRuleName.ToLowerInvariant())));
                }
                if (!String.IsNullOrEmpty(searchCompanyRuleNo))
                {
                    query = query.Where(x => (!String.IsNullOrEmpty(x.CR_Number) && x.CR_Number.ToLowerInvariant().Contains(searchCompanyRuleNo.ToLowerInvariant())));
                }
                var comruleList = query.Select(s => new WSR_CompRuleModel
                {
                    CR_Date = s.CR_Date.ToString("dd/MM/yyyy"),
                    CR_File = s.CR_File,
                    CR_Id = s.CR_Id,
                    CR_Name = s.CR_Name.Replace("/", "<br>"),
                    CR_Note = s.CR_Note,
                    CR_Number = s.CR_Number,
                    CR_Rev = s.CR_Rev,
                    CR_Update = s.CR_Update.ToString("dd/MM/yyyy"),
                    CRT_Id = s.CRT_Id,
                    CompRuleType = (from crt in db.WSR_CompRuleType where crt.CRT_Id == s.CRT_Id select crt.CRT_Type).FirstOrDefault()

                }).OrderBy(o => o.CR_Number).ToList();
           
                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(comruleList);
                }

                IPagedList<WSR_CompRuleModel> comrulePagedList = comruleList.ToPagedList(pageIndex, pageSize);

                List<SelectListItem> SelectCompanyRuleType_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
                    .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();

                model.WSR_CompRuleModelPagedList = comrulePagedList;
                model.SelectCompanyRuleType = SelectCompanyRuleType_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = String.Format("Error: Post /CompanyRuleSearch/CompanyRuleList {0}", ex.ToString());
                return View("Error");
            }
        }

        //
        // GET: /CompanyRuleSet/CreateCompanyRule
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult CreateCompanyRule()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                WSR_CompRuleSetModel model = new WSR_CompRuleSetModel();

                List<SelectListItem> comptype_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
                    .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();
                
                model.SelectCompanyRuleType = comptype_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View();
            }
        }

        //
        // POST: /CompanyRuleSet/CreateCompanyRule
        [HttpPost]
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult CreateCompanyRule(HttpPostedFileBase file, WSR_CompRuleSetModel model)
        {
            try
            {
                List<SelectListItem> comptype_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
                  .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();
                model.SelectCompanyRuleType = comptype_list;

                var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
               
                if (file != null)
                {
                    string _FileName = String.Concat(model.CompanyRuleTypeId, '-', model.CR_Number, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                    string _path = Path.Combine(path_ws_company_rule, _FileName);
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                    // Get a list of invalid file characters.
                    /*string pattern = "^[a-zA-Z0-9_.\\s-]*$";
                    Match match = Regex.Match(_FileName, pattern, RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        ViewBag.Message = String.Format("The following characters are invalid in a filename", "a-z,0-9");
                        return View(model);
                    }*/
                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.Message = "Invalid file extension, Only word, PDF, excel, text and image (.jpg, .jpeg, .png) files.";
                        return View(model);
                    }
                    if (System.IO.File.Exists(_path))
                    {
                        ViewBag.Message = String.Format("The {0} file already exists.", _FileName);
                        return View(model);
                    }
                    WSR_CompRule comprule = new WSR_CompRule()
                    {                       
                        CR_File = _FileName,
                        CR_Name = string.Concat(model.CR_Name_th, "/", model.CR_Name_eng),
                        CR_Number = model.CR_Number,
                        CR_Note = model.CR_Note,
                        CR_Rev = model.CR_Rev,
                        CR_Update = model.CR_Update,
                        CR_Date = DateTime.Now,
                        CRT_Id = model.CompanyRuleTypeId
                    };

                    var result = db.WSR_CompRule.Add(comprule);
                    file.SaveAs(_path);                   
                    //Save Logs
                    if(result != null){
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Company Rule",
                            Log_System = "WS & Rule",
                            Log_Detail = string.Concat(result.CR_Number, "/"
                                        , result.CR_Name, "/", result.CRT_Id.ToString(), "/"
                                        , result.CR_File),
                            Log_Action_Id = result.CR_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }

                    db.SaveChanges();
                    
                    TempData["shortMessage"] = String.Format("Created successfully, {0} item. ", model.CR_Name_eng);
                    return RedirectToAction("CompanyRuleSetList", new CompanyRuleSetModel {Page=1, searchCompanyRuleNo = model.CR_Number });
                }
                else
                {
                    ViewBag.Message = String.Format("The Company Rule File is required.");
                    return View(model);
                }                              
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        // GET: /CompanyRuleSet/EditCompRule/5
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult EditCompRule(int id)
        {
            try
            {
                List<SelectListItem> comptype_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
                  .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();
               
                var model = db.WSR_CompRule.Where(x => x.CR_Id == id)
                    .Select(s => new WSR_CompRuleSetModel
                    {
                        CR_Id = s.CR_Id,
                        CR_Number = s.CR_Number,
                        CR_Rev = s.CR_Rev,
                        CR_Date = s.CR_Date,
                        CR_Name_th = s.CR_Name,
                        CR_Note = s.CR_Note,
                        CR_File = s.CR_File,
                        CR_Update = s.CR_Update,
                        CompanyRuleTypeId = s.CRT_Id

                    }).FirstOrDefault();
               
                if (model != null)
                {
                    string[] str_list = model.CR_Name_th.Split(new char[] { '/' }, 2);
                    model.SelectCompanyRuleType = comptype_list;
                    model.CR_Name_th = str_list[0];
                    model.CR_Name_eng = str_list[1];

                    return View(model);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View();
            }

            ViewBag.errorMessage = "Company Rule Type Id is null";
            return View("Error");
        }

        //
        // POST: /CompanyRuleSet/EditCompRule/5
        [HttpPost]
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult EditCompRule(HttpPostedFileBase file,WSR_CompRuleSetModel model)
        {
            try
            {
                List<SelectListItem> comptype_list = db.WSR_CompRuleType.OrderBy(o => o.CRT_Type)
                 .Select(s => new SelectListItem { Value = s.CRT_Id.ToString(), Text = s.CRT_Type }).ToList();

                model.SelectCompanyRuleType = comptype_list;

                if(ModelState.IsValid)
                {
                    WSR_CompRule comprule = db.WSR_CompRule.Where(x => x.CR_Id == model.CR_Id).FirstOrDefault();

                    if (comprule != null)
                    {
                        if (file != null)
                        {
                            string _FileName = String.Concat(model.CompanyRuleTypeId, '-', model.CR_Number, Path.GetExtension(file.FileName));//Path.GetFileName(file.FileName);
                            string _path = Path.Combine(path_ws_company_rule, _FileName);
                            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                            var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpg", "jpeg", "png" };
                            // Get a list of invalid file characters.
                            /*string pattern = "^[a-zA-Z0-9_.\\s-]*$";
                            Match match = Regex.Match(_FileName, pattern, RegexOptions.IgnoreCase);
                            if (!match.Success)
                            {
                                ViewBag.Message = String.Format("The following characters are invalid in a filename", "a-z,0-9");
                                return View(model);
                            }*/
                            if (!supportedTypes.Contains(fileExt))
                            {
                                ViewBag.Message = "Invalid file extension, Only word, PDF, excel, text and image (.jpg, .jpeg, .png) files.";
                                return View(model);
                            }
                            /*
                            if (System.IO.File.Exists(_path))
                            {
                                ViewBag.Message = String.Format("The {0} file already exists.", _FileName);
                                return View(model);
                            }*/
                            file.SaveAs(_path);
                            comprule.CR_File = _FileName;
                        }
                        else
                        {
                            comprule.CR_File = model.CR_File;
                        }
                        
                        comprule.CR_Date = DateTime.Now;
                        comprule.CR_Number = model.CR_Number;
                        comprule.CR_Name = string.Concat(model.CR_Name_th, "/", model.CR_Name_eng);
                        comprule.CR_Rev = model.CR_Rev;
                        comprule.CR_Update = model.CR_Update;
                        comprule.CR_Note = model.CR_Note;
                        comprule.CRT_Id = model.CompanyRuleTypeId;

                        db.Entry(comprule).State = EntityState.Modified;
                     
                        //Save logs
                        if(comprule != null){
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            Log logmodel = new Log()
                            {
                                Log_Action = "edit",
                                Log_Type = "Company Rule",
                                Log_System = "WS & Rule",
                                Log_Detail = string.Concat(comprule.CR_Number, "/"
                                            , comprule.CR_Name, "/", comprule.CRT_Id.ToString(), "/"
                                            , comprule.CR_File),
                                Log_Action_Id = comprule.CR_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }

                        db.SaveChanges();

                        TempData["shortMessage"] = String.Format("Edited successfully, {0} item. ", model.CR_Number);
                        return RedirectToAction("CompanyRuleSetList", new CompanyRuleSetModel { Page = 1, searchCompanyRuleNo = model.CR_Number });
                    }
                    else
                    {
                        ViewBag.errorMessage = "Company Rule Id is null";
                        return View("Error");
                    }
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        //
        // POST: /CompanyRuleSet/DeleteCompanyRule/5
        [HttpPost]
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult DeleteCompanyRule(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var comprule_list = db.WSR_CompRule.Where(x => id_list.Contains(x.CR_Id)).ToList();
                    var file_list = comprule_list.Select(s => s.CR_File).ToList();
                    foreach(var filename in file_list)
                    {
                        string _path = Path.Combine(path_ws_company_rule, filename);
                        // Check if file exists with its full path    
                        if (System.IO.File.Exists(_path))
                        {
                            // If file found, delete it    
                            System.IO.File.Delete(_path);
                            Console.WriteLine("File deleted.");
                        } 
                    }
                  
                    db.WSR_CompRule.RemoveRange(comprule_list);
                    //Save Logs
                    if (comprule_list != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        foreach (var i in comprule_list)
                        {
                            Log logmodel = new Log()
                            {
                                Log_Action = "delete",
                                Log_Type = "Company Rule",
                                Log_System = "WS & Rule",
                                Log_Detail = string.Concat(i.CR_Number, "/"
                                            , i.CR_Name, "/", i.CRT_Id.ToString(), "/"
                                            , i.CR_File),
                                Log_Action_Id = i.CR_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }                       
                    }

                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Deleted successfully, {0} items. ", id_list.Count());
                    return RedirectToAction("CompanyRuleSetList");
                }
                return RedirectToAction("CompanyRuleSetList");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
        public ActionResult DownloadFile(string fileName , string RuleNumer)
        {          
            //Build the File Path.
            //string path = Server.MapPath("~/File/") + fileName;  --visual path

            if (string.IsNullOrEmpty(fileName))
            {
                TempData["message"] = "The file name is invalid.";
                return RedirectToAction("CompanyRuleSetList");
            }
            string path = path_ws_company_rule + fileName;
            try
            {
                //Read the File data into Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                string downloadname = string.IsNullOrEmpty(fileName) ? RuleNumer : $"{RuleNumer}{Path.GetExtension(fileName)}";
                //Send the File to Download.
                return File(bytes, "application/octet-stream", downloadname);
            }
            catch (IOException)
            {
                //ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                //// ViewBag.errorMessage = io.ToString();
                //return View("Error");
                TempData["message"] = $"File not found : {fileName}";
                return RedirectToAction("CompanyRuleSetList");
            }
           
        }
        [CustomAuthorize(18)] //2 Company rule/Setup Rule/WS & Rule
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
        // POST: /CompanyRuleSet/GetComRuleNo/
        [HttpPost]
        public JsonResult GetComRuleNo(string Prefix)
        {
            var crno = (from cr in db.WSR_CompRule
                        where cr.CR_Number.StartsWith(Prefix)
                        select new { label = cr.CR_Number, val = cr.CR_Number }).Take(10).ToList();
            return Json(crno);
        }
        //
        // POST: /CompanyRuleSet/GetComRuleName/
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
