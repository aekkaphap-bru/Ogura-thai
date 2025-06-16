using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.WsAndRule
{
    [Authorize]
    public class CompanyRuleTypeSetController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
       
        //
        // GET: /CompanyRuleTypeSet/CompanyRuleTypeSetList/
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult CompanyRuleTypeSetList(CompanyRuleTypeSetModel model)
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

                string searchCompanyRuleType = !String.IsNullOrEmpty(model.searcCompanyRuleType) ? model.searcCompanyRuleType : null;

                IEnumerable<WSR_CompRuleType> query = model.Page.HasValue ? db.WSR_CompRuleType : db.WSR_CompRuleType;
                if (!String.IsNullOrEmpty(searchCompanyRuleType))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CRT_Type) 
                        && x.CRT_Type.ToLowerInvariant().Contains(searchCompanyRuleType.ToLowerInvariant()));
                }
                var compruletypeList = query.Select(s => new WSR_CompRuleTypeModel
                {
                    CRT_Id = s.CRT_Id,
                    CRT_Note = s.CRT_Note,
                    CRT_Type = s.CRT_Type

                }).OrderBy(o => o.CRT_Id).ToList();

                IPagedList<WSR_CompRuleTypeModel> compruletypePagedList = compruletypeList.ToPagedList(pageIndex, pageSize);

                model.WSR_CompRuleTypeModelPagedList = compruletypePagedList;
               
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View("");
            }
           
        }

        //
        // POST: /CompanyRuleTypeSet/CompanyRuleTypeSetList/
        [HttpPost]
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult CompanyRuleTypeSetList(FormCollection form,CompanyRuleTypeSetModel model)
        {
            try
            {
                int pageSize = 30;
                int pageIndex = 1;
               
                string searchCompanyRuleType = !String.IsNullOrEmpty(model.searcCompanyRuleType) ? model.searcCompanyRuleType : null;

                IEnumerable<WSR_CompRuleType> query = db.WSR_CompRuleType;
                if (!String.IsNullOrEmpty(searchCompanyRuleType))
                {
                    query = query.Where(x => !String.IsNullOrEmpty(x.CRT_Type)
                        && x.CRT_Type.ToLowerInvariant().Contains(searchCompanyRuleType.ToLowerInvariant()));        
                }
                var compruletypeList = query.Select(s => new WSR_CompRuleTypeModel
                {
                    CRT_Id = s.CRT_Id,
                    CRT_Note = s.CRT_Note,
                    CRT_Type = s.CRT_Type

                }).OrderBy(o => o.CRT_Id).ToList();

                if (form["ExportToCsv"] == "ExportToCsv")
                {
                    ExportToCsv(compruletypeList);
                }

                IPagedList<WSR_CompRuleTypeModel> compruletypePagedList = compruletypeList.ToPagedList(pageIndex, pageSize);

                model.WSR_CompRuleTypeModelPagedList = compruletypePagedList;
                            
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View("");
            }
        }

        //
        // GET: /CompanyRuleTypeSet/CreateCompanyRuleType
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult CreateCompanyRuleType()
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }
                WSR_CompRuleTypeModel model = new WSR_CompRuleTypeModel();

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // POST: /CompanyRuleTypeSet/CreateCompanyRuleType
        [HttpPost]
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult CreateCompanyRuleType(WSR_CompRuleTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WSR_CompRuleType compruletype = new WSR_CompRuleType()
                    {
                        CRT_Note = model.CRT_Note,
                        CRT_Type = model.CRT_Type,
                    };
                    //Check add duplicate
                    var check = db.WSR_CompRuleType
                        .Where(x => x.CRT_Type == model.CRT_Type ).FirstOrDefault();
                    if (check != null)
                    {
                        ViewBag.Message = String.Format("Invalid data, Duplicate on the company rule ");
                        return View(model);
                    }

                    var result = db.WSR_CompRuleType.Add(compruletype);
                    //Save log
                    if (result != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        } 
                        Log logmodel = new Log()
                        {
                            Log_Action = "add",
                            Log_Type = "Company Rule Type",
                            Log_System = "WS & Rule",
                            Log_Detail = string.Concat(result.CRT_Type),
                            Log_Action_Id = result.CRT_Id,
                            Log_Date = DateTime.Now,
                            Log_by = user_nickname
                        };
                        db.Logs.Add(logmodel);
                    }

                    db.SaveChanges();
                    TempData["shortMessage"] = String.Format("Created successfully, {0} item. ", model.CRT_Type);
                    return RedirectToAction("CompanyRuleTypeSetList", new CompanyRuleTypeSetModel { Page=1, searcCompanyRuleType = model.CRT_Type});
                }
                return View(model);
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // GET: /CompanyRuleTypeSet/EditCompRuleType/5
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult EditCompRuleType(int id)
        {
            try
            {
                var model = db.WSR_CompRuleType.Where(x => x.CRT_Id == id)
                    .Select(s => new WSR_CompRuleTypeModel
                    {
                        CRT_Id = s.CRT_Id,
                        CRT_Type = s.CRT_Type,
                        CRT_Note = s.CRT_Note
                    }).FirstOrDefault();

                if (model != null)
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
            ViewBag.errorMessage = "Company Rule Type Id is null";
            return View("Error");
        }

        //
        // POST: /CompanyRuleTypeSet/EditCompRuleType/5
        [HttpPost]
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult EditCompRuleType(WSR_CompRuleTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WSR_CompRuleType compruletype = db.WSR_CompRuleType.Where(x => x.CRT_Id == model.CRT_Id).FirstOrDefault();

                    if (compruletype != null)
                    {
                        compruletype.CRT_Type = model.CRT_Type;
                        compruletype.CRT_Note = model.CRT_Note;
                        //Check add duplicate
                        var check = db.WSR_CompRuleType
                            .Where(x => x.CRT_Type == model.CRT_Type && x.CRT_Id != model.CRT_Id).FirstOrDefault();
                        if (check != null)
                        {
                            ViewBag.Message = String.Format("Invalid data, Duplicate on the company rule ");
                            return View(model);
                        }
                        //Edit 
                        db.Entry(compruletype).State = EntityState.Modified;                       
                        //Save Logs
                        if (compruletype != null)
                        {
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }                           
                            Log logmodel = new Log()
                            {
                                Log_Action = "edit",
                                Log_Type = "Company Rule Type",
                                Log_System = "WS & Rule",
                                Log_Detail = string.Concat(compruletype.CRT_Type),
                                Log_Action_Id = compruletype.CRT_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }
                        db.SaveChanges();
                        TempData["shortMessage"] = String.Format("Edited successfully, {0} item. ", model.CRT_Type);
                        return RedirectToAction("CompanyRuleTypeSetList", new CompanyRuleTypeSetModel { Page = 1, searcCompanyRuleType = model.CRT_Type });
                    }
                    else
                    {
                        ViewBag.errorMessage = "Company Rule Type Id is null";
                        return View("Error");
                    }
                }
                return View(model);
               
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }   
        }


        //
        // POST: /CompanyRuleTypeSet/Delete/5
        [HttpPost]
        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public ActionResult DeleteCompanyRuleType(FormCollection form)
        {
            try
            {
                List<int> id_list = new List<int>();
                var selectedItem = form["selectedItem"];
                if (selectedItem != null)
                {
                    id_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    var compruletype_list = db.WSR_CompRuleType.Where(x => id_list.Contains(x.CRT_Id)).ToList();

                    foreach(var i in compruletype_list)
                    {
                        var check = db.WSR_CompRule.Where(x => x.CRT_Id == i.CRT_Id).FirstOrDefault();
                        if (check != null)
                        {
                            TempData["shortError"] = String.Format("Cannot be deleted, {0}", i.CRT_Type);
                            return RedirectToAction("CompanyRuleTypeSetList");
                        }
                        db.WSR_CompRuleType.Remove(i);
                    }

                    //Save Logs
                    if (compruletype_list != null)
                    {
                        string user_nickname = null;
                        if (Session["NickName"] != null)
                        {
                            user_nickname = Session["NickName"].ToString();
                        }
                        foreach (var i in compruletype_list)
                        {
                            Log logmodel = new Log()
                            {
                                Log_Action = "delete",
                                Log_Type = "Company Rule Type",
                                Log_System = "WS & Rule",
                                Log_Detail = string.Concat(i.CRT_Type),
                                Log_Action_Id = i.CRT_Id,
                                Log_Date = DateTime.Now,
                                Log_by = user_nickname
                            };
                            db.Logs.Add(logmodel);
                        }
                        db.SaveChanges();
                    }
                    
                    TempData["shortMessage"] = String.Format("Deleted successfully, {0} items. ", id_list.Count());
                    return RedirectToAction("CompanyRuleTypeSetList");

                }
                return RedirectToAction("CompanyRuleTypeSetList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        [CustomAuthorize(17)]//1 Company rule Type/Setup Rule/WS & Rule
        public void ExportToCsv(List<WSR_CompRuleTypeModel> model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model;

                var forexport = data.Select((v, i) => new WSR_CompRuleTypeModel
                {
                    item = i + 1,
                    CRT_Type = v.CRT_Type,
                    CRT_Note = "\""+ v.CRT_Note + "\"",
                });
               
                sb.AppendFormat("{0},{1},{2},{3}",
                    "Item", "Company rule type name", "Note", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3}",
                        item.item, item.CRT_Type,item.CRT_Note, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CompanyRuleType.CSV ");
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

        // POST: /CompanyRuleTypeSet/GetComRuleTypeName/
        [HttpPost]
        public JsonResult GetComRuleTypeName(string Prefix)
        {
            var crtname = db.WSR_CompRuleType
                            .Where(x => x.CRT_Type.StartsWith(Prefix))
                            .Take(10)
                            .Select(s => new { label = s.CRT_Type, val = s.CRT_Type }).ToList();
            return Json(crtname);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
