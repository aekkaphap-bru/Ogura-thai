using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.BillingNote
{
    [Authorize]
    public class BankAccountController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private OCTIIS_WEBAPPEntities accb = new OCTIIS_WEBAPPEntities();
        
        //
        // GET: /BankAccount/Setup
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Setup()
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            if (TempData["shortError"] != null)
            {
                ViewBag.ErrorMessage = TempData["shortError"].ToString();
            }

            var query = accb.Acc_Bank.ToList();
            List<BankAccountModels> bank_list = query.Select(s => new BankAccountModels
                {
                    id = s.Id,
                    BankName_EN = s.BankName_EN,
                    Address_EN = s.Address_EN,
                    BankName_TH = s.BankName_Thi,
                    Address_TH = s.Address_Thi,
                    Acc_No = s.AccOn,
                    Branch = s.Branch,
                    SwiftCode = s.SwiftCode,
                }).ToList();

            BankAccountListModel model = new BankAccountListModel();
            model.BankAccountList = bank_list;

            return View(model);
        }
        //
        // GET: /BankAccount/Create
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Create()
        {
            
            BankAccountModels model = new BankAccountModels();
            return View(model);
        }
        //
        // POST: /BankAccount/Create
        [HttpPost]
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Create(BankAccountModels model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    Acc_Bank bank_acc = new Acc_Bank();
                    bank_acc.BankName_EN = model.BankName_EN;
                    bank_acc.Address_EN = model.Address_EN;
                    bank_acc.BankName_Thi = model.BankName_TH;
                    bank_acc.Address_Thi = model.Address_TH;
                    bank_acc.AccOn = model.Acc_No;
                    bank_acc.Branch = model.Branch;
                    bank_acc.SwiftCode = model.SwiftCode;

                    accb.Acc_Bank.Add(bank_acc);
                    accb.SaveChanges();

                    //Save Logs
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    Log logmodel = new Log()
                    {
                        Log_Action = "add",
                        Log_Type = "Setup",
                        Log_System = "Account",
                        Log_Detail = String.Concat(model.Acc_No, "/", model.Branch),
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully created, Bank Name: {0} . ", model.BankName_EN);
                    return RedirectToAction("Setup");
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
           
        }
        //
        // GET: /BankAccount/Edit
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Edit(int id)
        {
            BankAccountModels model = new BankAccountModels();
            var bank = accb.Acc_Bank.Where(x => x.Id == id).FirstOrDefault();
            if(bank != null)
            {
                model.id = bank.Id;
                model.BankName_EN = bank.BankName_EN;
                model.BankName_TH = bank.BankName_Thi;
                model.Address_EN = bank.Address_EN;
                model.Address_TH = bank.Address_Thi;
                model.Acc_No = bank.AccOn;
                model.Branch = bank.Branch;
                model.SwiftCode = bank.SwiftCode;

                return View(model);
            }
            ViewBag.errorMessage = "Bank Account is Null.";
            return View("Error");
        }
        //
        // POST: /BankAccount/Edit
        [HttpPost]
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Edit(BankAccountModels model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Acc_Bank bank_acc = accb.Acc_Bank.Where(x => x.Id == model.id).FirstOrDefault();
                    bank_acc.BankName_EN = model.BankName_EN;
                    bank_acc.Address_EN = model.Address_EN;
                    bank_acc.BankName_Thi = model.BankName_TH;
                    bank_acc.Address_Thi = model.Address_TH;
                    bank_acc.AccOn = model.Acc_No;
                    bank_acc.Branch = model.Branch;
                    bank_acc.SwiftCode = model.SwiftCode;

                    accb.Entry(bank_acc).State = System.Data.Entity.EntityState.Modified;
                    accb.SaveChanges();

                    //Save Logs
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    Log logmodel = new Log()
                    {
                        Log_Action = "edit",
                        Log_Type = "Setup",
                        Log_System = "Account",
                        Log_Detail = String.Concat(model.Acc_No, "/", model.Branch),
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();

                    TempData["shortMessage"] = String.Format("Successfully edited, Bank Name: {0} . ", model.BankName_EN);
                    return RedirectToAction("Setup");
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        [CustomAuthorize(66)]//Billing Note
        public ActionResult Delete(int id)
        {
            try
            {
                var bank = accb.Acc_Bank.Where(x => x.Id == id).FirstOrDefault();
                string bank_name = "";
                if (bank != null)
                {
                    bank_name = bank.BankName_EN;
                    accb.Acc_Bank.Remove(bank);
                    accb.SaveChanges();

                    //Save Logs
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    Log logmodel = new Log()
                    {
                        Log_Action = "delete",
                        Log_Type = "Setup",
                        Log_System = "Account",
                        Log_Detail = String.Concat(bank.AccOn, "/", bank.Branch),
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();
                }

                TempData["shortMessage"] = String.Format("Successfully deleted, Bank Name: {0} . ", bank_name);
                return RedirectToAction("Setup");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            accb.Dispose();
            db.Dispose();
            base.Dispose(disposing);
        }

	}
}