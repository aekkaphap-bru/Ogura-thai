using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.BillingNote
{
    [Authorize]
    public class CollectionStatementReprintController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private Thomas_Ogura_TESTDBEntities tb = new Thomas_Ogura_TESTDBEntities();
        private OCTIIS_WEBAPPEntities accb = new OCTIIS_WEBAPPEntities();
        
        //
        // GET: /CollectionStatementReprint/BList
        [CustomAuthorize(66)]//Billing Note
        public ActionResult BList()
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            if (TempData["shortError"] != null)
            {
                ViewBag.ErrorMessage = TempData["shortError"].ToString();
            }
            BillReprintModel model = new BillReprintModel();
            List<BillReprintListModel> billReprintList = new List<BillReprintListModel>();
            model.BillReprintList = billReprintList;

            //Select Customer
            var customer_select = accb.Acc_BillingDetails.Select(s => new { s.CustomerCode, s.CustomerName }).Distinct().ToList();
            List<SelectListItem> customerList = customer_select
                .Select(s => new SelectListItem
                {
                    Value = s.CustomerCode,
                    Text = String.Concat(s.CustomerCode, " ", s.CustomerName)
                }).ToList();

            model.SelectCustomerName = customerList;

            return View(model);
        }
        //
        //POST: /CollectionStatementReprint/BList
        [HttpPost]
        [CustomAuthorize(66)]//Billing Note
        public ActionResult BList(FormCollection form, BillReprintModel model)
        {
            try
            {
                //Select Customer
                var customer_select = accb.Acc_BillingDetails.Select(s => new { s.CustomerCode, s.CustomerName }).Distinct().ToList();
                List<SelectListItem> customerList = customer_select
                    .Select(s => new SelectListItem
                    {
                        Value = s.CustomerCode,
                        Text = String.Concat(s.CustomerCode, " ", s.CustomerName)
                    }).ToList();

                model.SelectCustomerName = customerList;
                var query = accb.Acc_BillingDetails.ToList();
                if (!String.IsNullOrEmpty(model.customerCode))
                {
                    query = query.Where(x => x.CustomerCode == model.customerCode).ToList();
                }
                if (model.fromDate.HasValue)
                {
                    query = query.Where(x => x.DocumentDate >= model.fromDate.Value).ToList();
                }
                if (model.toDate.HasValue)
                {
                    query = query.Where(x => x.DocumentDate <= model.toDate.Value).ToList();
                }
                List<BillReprintListModel> bill_list = query.GroupBy(s => new { s.DocumentDate, s.DueDate, s.DocumentNo, s.CustomerCode, s.CustomerName,s.Payment,s.Currency,s.MasterAccBankId })
                    .Select(g => new BillReprintListModel
                    {
                        HeaderDate_str = g.Key.DocumentDate.ToString("dd-MM-yyyy"),
                        HeaderDate = g.Key.DocumentDate,
                        DueDate_str = g.Key.DueDate.ToString("dd-MM-yyyy"),
                        DueDate = g.Key.DueDate,
                        BillNo = g.Key.DocumentNo,
                        CustomerCode = g.Key.CustomerCode,
                        CustomerName = g.Key.CustomerName,
                        Amount = g.Sum(x => Convert.ToDecimal(x.Amount)),
                        Amount_str = (g.Sum(x => Convert.ToDecimal(x.Amount))).ToString("N"),
                        Vat = g.Sum(x => Convert.ToDecimal(x.Vat)),
                        Vat_str = (g.Sum(x => Convert.ToDecimal(x.Vat))).ToString("N"),
                        Total = g.Sum(x => Convert.ToDecimal(x.Total)),
                        Total_str = (g.Sum(x => Convert.ToDecimal(x.Total))).ToString("N"),
                        CreditTerm = g.Key.Payment,
                        Currency = g.Key.Currency,
                        BankAcc = (from b in accb.Acc_Bank where b.Id == g.Key.MasterAccBankId select b.BankName_EN).FirstOrDefault(),
                    }).ToList();

                model.BillReprintList = bill_list;

                //For Export CSV
                if (form["ExportCSV"] == "ExportCSV")
                {
                    ExportToCsv(bill_list);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        public ActionResult BReprint(string bill_no, string customer_code)
        {
            try
            {
                BillingNoteModels model = new BillingNoteModels();

                var query_billList = accb.Acc_BillingDetails.Where(x => x.DocumentNo == bill_no && x.CustomerCode == customer_code).ToList();

                if (!query_billList.Any())
                {
                    ViewBag.errorMessage = "No bill.";
                    return View("Error");
                }
                //----------------------------------------------------------------------------------------------
                //Invoice
                List<BillListModel> b_list = query_billList.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceNo,
                    InvoiceCreateDate = s.InvoiceDate,
                    InvoiceCreateDate_str = s.InvoiceDate.ToString("dd-MM-yyyy"),
                    PaymentDueDate_str = s.DueDate.ToString("dd-MM-yyyy"),
                    PaymentDueDate = s.DueDate,
                    Amount = Convert.ToDecimal(s.Amount),
                    Amount_str = Convert.ToDecimal(s.Amount).ToString("N"),
                    Vat = Convert.ToDecimal(s.Vat),
                    Vat_str = (Convert.ToDecimal(s.Vat)).ToString("N"),
                    TotalAmount = Convert.ToDecimal(s.Total),
                    TotalAmount_str = (Convert.ToDecimal(s.Total)).ToString("N"),

                }).OrderBy(o => o.InvoiceNo).ToList();
                //----------------------------------------------------------------------------------------------
                //Get Grand total amount
                decimal grand_amount = b_list.Select(s => s.Amount).Sum();
                model.GrandAmount = grand_amount;
                model.GrandAmount_str = grand_amount.ToString("N");
                decimal grand_vat = b_list.Select(s => s.Vat).Sum();
                model.GrandVat = grand_vat;
                model.GrandVat_str = grand_vat.ToString("N");
                decimal grand_total_amount = b_list.Select(s => s.TotalAmount).Sum();
                model.GrandTotalAmount = grand_total_amount;
                model.GrandTotalAmount_str = grand_total_amount.ToString("N");
               
                //Page Count
                int count_rows = b_list.Count();
                if (count_rows > 0)
                {
                    int pages = (int)Math.Ceiling(count_rows / 15.0);
                    model.count_page = pages;
                }
                else
                {
                    model.count_page = 1;
                }

                //-------------------------------------------------
                var b_1 = query_billList.FirstOrDefault();
                model.BillList = b_list.OrderBy(o => o.InvoiceNo).ToList();
                model.acc_id = Convert.ToInt32(b_1.MasterAccBankId);
                model.createDate_str = b_1.DocumentDate.ToString("dd-MM-yyyy");
                model.createDate = b_1.DocumentDate;
                model.dueDate_str = b_1.DueDate.ToString("dd-MM-yyyy");
                model.dueDate = b_1.DueDate;
                model.creditTerm = b_1.Payment.ToString("N0");
                model.billNo_str = b_1.DocumentNo;
                model.amount_type = b_1.Currency;

                PrepareData(model, customer_code);
                ExportPDF(model);

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        public BillingNoteModels PrepareData(BillingNoteModels model, string customer_code)
        {
            try
            {
                //--------------------------------------------------------------------------------------------
                //Get Header Customer
                BillCustomerHeadModel head_customer = tb.MasterCustomers.Where(x => x.CustomerCode == customer_code)
                   .Select(s => new BillCustomerHeadModel
                   {
                       CustomerCode = s.CustomerCode,
                       CustomerName = s.MasterInvoice.InvoiceName,
                       Address1 = s.MasterInvoice.Address1,
                       Address2 = s.MasterInvoice.Address2,
                       PostalCode = s.MasterInvoice.PostalCode,
                       CreditTerm = s.MasterInvoice.PaymentTerm,
                       CustomerTaxId = s.MasterInvoice.TaxId,
                       Tel = s.MasterInvoice.TelephoneNo1,
                       Fax = s.MasterInvoice.FaxNo,
                       PersonInCharge = s.PersonInCharge,

                   }).FirstOrDefault();

                //------------------------------------------------------------------------------------------------
                //Get Header 
                BillHeadModel head = new BillHeadModel();

                //------------------------------------------------------------------------------------------------
                //Get Account Bank
                AccountBankModel accBank = accb.Acc_Bank.Where(x => x.Id == model.acc_id)
                    .Select(s => new AccountBankModel
                    {
                        acc_id = s.Id,
                        BankName = s.BankName_EN,
                        BankAddress = s.Address_EN,
                        BankBranch = s.Branch,
                        AccNo = s.AccOn,
                        SwiftCode = s.SwiftCode,
                    }).FirstOrDefault();

                model.BillHead = head;
                model.BillCustomerHead = head_customer;
                model.AccountBank = accBank;

                return model;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        public void ExportPDF(BillingNoteModels model)
        {
            try
            {

                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=CollectStatement_Reprint.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                MemoryStream stream = new MemoryStream();
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                CreatePage(model, document, "ORIGINAL", 0);
               
                document.Close();
                Response.BinaryWrite(stream.ToArray());
                Response.End();

                //Save Logs
                string user_nickname = null;
                if (Session["NickName"] != null)
                {
                    user_nickname = Session["NickName"].ToString();
                }
                Log logmodel = new Log()
                {
                    Log_Action = "re-print",
                    Log_Type = "Collection Statement",
                    Log_System = "Account",
                    Log_Detail = String.Concat(model.billNo_str, "/", model.BillCustomerHead.CustomerCode),
                    Log_Action_Id = 0,
                    Log_Date = DateTime.Now,
                    Log_by = user_nickname
                };
                db.Logs.Add(logmodel);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        public Document CreatePage(BillingNoteModels model, Document document, string page_status, int page_num)
        {
            var path_font_eng1 = Server.MapPath("~/fonts/SEGOEUI.TTF");
            var path_font_eng2 = Server.MapPath("~/fonts/SEGOEUIB.ttf");

            PdfFont font_eng = PdfFontFactory.CreateFont(path_font_eng1, PdfEncodings.IDENTITY_H);
            PdfFont font_eng_bold = PdfFontFactory.CreateFont(path_font_eng2, PdfEncodings.IDENTITY_H);

            int page_first = (model.count_page * page_num) + 1;
            int page_last = (model.count_page * page_num) + model.count_page;
            int r = 1;
            for (var i = page_first; i <= page_last; i++)
            {

                //----------Company Header--------------------------------------------------------------------------------- 
                Paragraph company_eng = new Paragraph();
                company_eng.Add(new Text("OGURA CLUTCH THAILAND CO.,LTD.").SetFont(font_eng_bold).SetUnderline().SetFontSize(24));
                company_eng.SetFixedPosition(i, 38, 745, 500);
                document.Add(company_eng);
                /*
                Paragraph page_original = new Paragraph(page_status);
                page_original.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 512, 808, 100);
                document.Add(page_original);
                */

                //------Customer Header--------------------------------------------------------------------------------------
                Paragraph bill_no_title = new Paragraph("No.:");
                bill_no_title.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 425, 700, 50);
                document.Add(bill_no_title);
                Paragraph bill_no = new Paragraph(model.billNo_str);
                bill_no.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 472, 700, 85);
                document.Add(bill_no);

                Paragraph date_title = new Paragraph("Date:");
                date_title.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 425, 685, 50);
                document.Add(date_title);
                Paragraph date_create = new Paragraph(model.createDate_str);
                date_create.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 472, 685, 85);
                document.Add(date_create);

                //Customer Name + Address
                Paragraph cus_address = new Paragraph();
                cus_address.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 38, 610, 350);
                cus_address.Add(model.BillCustomerHead.CustomerName
                    + "\n " + model.BillCustomerHead.Address1
                    + " " + model.BillCustomerHead.Address2 + " " + model.BillCustomerHead.PostalCode
                    + "\n ATTAN: " + model.BillCustomerHead.PersonInCharge);
                cus_address.Add("\n"
                    + "\n Gentelman:"
                    + "\n We are pleased to enclose the following"
                    + "\n each one price of invoice");
                document.Add(cus_address);

                //-------Title--------------------------------------------------------------------------------------------
                Paragraph title_eng = new Paragraph("-Collection Statement-");
                title_eng.SetFont(font_eng).SetFontSize(18).SetFixedPosition(i, 220, 587, 400);
                document.Add(title_eng);


                //-----------Table----------------------------------------------------------------
                Table table = new Table(new float[] { 150, 100, 169, 100 });
                table.SetFixedPosition(i, 38, 300, 519);
                // table.AddCell(new Cell(1, 3).Add(new Paragraph("Cell with colspan 3")));
                // table.AddCell(new Cell(15, 1).Add(new Paragraph("Cell with rowspan 1")));
                Cell col_invoice_no = new Cell(1, 1);
                col_invoice_no.Add(new Paragraph("INVOICE NO").SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(new SolidBorder(0.5f));
                Cell col_invoice_date = new Cell(1, 1);
                col_invoice_date.Add(new Paragraph("INVOICE DATE").SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(new SolidBorder(0.5f));
                Cell col_amount = new Cell(1, 1);
                col_amount.Add(new Paragraph("AMOUNT(" + model.amount_type + ")").SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(new SolidBorder(0.5f));
                Cell col_due_date = new Cell(1, 1);
                col_due_date.Add(new Paragraph("DUE DATE").SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(new SolidBorder(0.5f));

                table.AddCell(col_invoice_no);
                table.AddCell(col_invoice_date);
                table.AddCell(col_amount);
                table.AddCell(col_due_date);

                int first_row = (r * 15) - 15;
                int last_row = r * 15;

                Cell column_invoiceno = new Cell(15, 1);
                column_invoiceno.SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER);
                Cell column_date = new Cell(15, 1);
                column_date.SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER);
                Cell column_totalamount = new Cell(15, 1);
                column_totalamount.SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER);
                Cell column_duedate = new Cell(15, 1);
                column_duedate.SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER);


                for (int j = first_row; j < last_row; j++)
                {
                    BillListModel element = model.BillList.ElementAtOrDefault(j);
                    if (element != null)
                    {
                        int num = j + 1;

                        column_invoiceno.Add(new Paragraph(element.InvoiceNo)); //.SetMultipliedLeading(1.3f));
                        column_date.Add(new Paragraph(element.InvoiceCreateDate.ToString("dd-MM-yyyy")));//.SetMultipliedLeading(1.3f));
                        column_duedate.Add(new Paragraph(model.dueDate.ToString("dd-MM-yyyy")));//.SetMultipliedLeading(1.3f));
                        column_totalamount.Add(new Paragraph(element.TotalAmount_str));//.SetMultipliedLeading(1.3f));
                    }
                    else
                    {
                        column_invoiceno.Add(new Paragraph("\n")); //.SetMultipliedLeading(1.3f));
                        column_date.Add(new Paragraph("\n")); //.SetMultipliedLeading(1.3f));
                        column_duedate.Add(new Paragraph("\n")); //.SetMultipliedLeading(1.3f));
                        column_totalamount.Add(new Paragraph("\n")); //.SetMultipliedLeading(1.3f));
                    }
                }

                table.AddCell(column_invoiceno);
                table.AddCell(column_date);
                table.AddCell(column_totalamount);
                table.AddCell(column_duedate);

                //--------Footer---------------------------
                Cell grend_total = new Cell(1, 2);
                grend_total.Add(new Paragraph("TOTAL AMOUNT:")).SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(0.5f));

                Cell sum_total = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(0.5f));
                if (r == model.count_page)
                {
                    sum_total.Add(new Paragraph(model.GrandTotalAmount_str).SetTextAlignment(TextAlignment.RIGHT));
                }
                else
                {
                    sum_total.Add(new Paragraph("\n").SetTextAlignment(TextAlignment.RIGHT));
                }

                Cell sum_duedate = new Cell();
                sum_duedate.Add(new Paragraph("\n")).SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(0.5f));


                table.AddCell(grend_total);
                table.AddCell(sum_total);
                table.AddCell(sum_duedate);
                table.SetFont(font_eng).SetFontSize(9);

                document.Add(table);

                //-----------Footer----------------------------------------------------------------------

                //---------Bank Seleced-----------------
                Paragraph foot_bank_1 = new Paragraph("TRANFER BANK: \n" + model.AccountBank.BankName
                    + "\n " + model.AccountBank.BankAddress
                    + "\n ACC.NO.: " + model.AccountBank.AccNo
                    + "\n BRANCH: " + model.AccountBank.BankBranch
                    + "\n SWIFT CODE: " + model.AccountBank.SwiftCode
                    );
                foot_bank_1.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 38, 150, 300); //.SetMultipliedLeading(1); //155
                document.Add(foot_bank_1);

                /*Paragraph foot_bank_2 = new Paragraph("By transfering to Bank A/C Number: "+ model.AccountBank.AccNo);
                foot_bank_2.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 140, 300);
                document.Add(foot_bank_2);
                Paragraph foot_bank_3 = new Paragraph(model.AccountBank.BankBranch+" "+model.AccountBank.BankAddress);
                foot_bank_3.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 110, 300).SetMultipliedLeading(1);
                document.Add(foot_bank_3);*/

                //-------Signed-------------------------              
                Paragraph foot_bill_eng_title = new Paragraph("Your very truly,");
                foot_bill_eng_title.SetFont(font_eng).SetFontSize(9).SetFixedPosition(i, 380, 165, 200);
                document.Add(foot_bill_eng_title);
                Paragraph foot_bill_eng_title_2 = new Paragraph("OGURA CLUTCH THAILAND CO.,LTD");
                foot_bill_eng_title_2.SetFont(font_eng_bold).SetFontSize(9).SetFixedPosition(i, 380, 150, 200);
                document.Add(foot_bill_eng_title_2);
                Paragraph foot_bill = new Paragraph("____________________________");
                foot_bill.SetFont(font_eng).SetFontSize(12).SetFixedPosition(i, 380, 90, 200);
                document.Add(foot_bill);

                r++;
            }

            return document;
        }

        public ActionResult DeleteBill(string bill_no, string customer_code)
        {
            try
            {
                var query_bill = accb.Acc_BillingDetails.Where(x => x.DocumentNo == bill_no && x.CustomerCode == customer_code).ToList();
                if (query_bill.Any())
                {
                    accb.Acc_BillingDetails.RemoveRange(query_bill);
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
                        Log_Type = "Collection Statement",
                        Log_System = "Account",
                        Log_Detail = String.Concat(bill_no, "/", customer_code),
                        Log_Action_Id = 0,
                        Log_Date = DateTime.Now,
                        Log_by = user_nickname
                    };
                    db.Logs.Add(logmodel);
                    db.SaveChanges();
                }

                TempData["shortMessage"] = String.Format("Successfully deleted, Document No: {0} . ", bill_no);
                return RedirectToAction("BList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        public void ExportToCsv(List<BillReprintListModel> listmodel)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = listmodel;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    date = v.HeaderDate_str,
                    dueDate = v.DueDate_str,
                    documentNo = v.BillNo,
                    customer_code = v.CustomerCode,
                    customer_name = "\""+ v.CustomerName + "\"",
                    amount = "\"" + v.Amount + "\"",
                    vat = "\"" + v.Vat + "\"",
                    totalAmount = "\"" + v.Total + "\"",
                    currency = "\"" + v.Currency + "\"",
                    creditTerm = v.CreditTerm.ToString(),
                    bankAcc = "\"" + v.BankAcc + "\""

                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    "Items", "Date","DocumentNo", "CustomerCode","CustomerName", "Amount", "Vat", "Total Amount","Due Date","CreditTerm","Bank","Currency", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        item.item, item.date, item.documentNo, item.customer_code, item.customer_name, item.amount, item.vat, item.totalAmount,item.dueDate,item.creditTerm,item.bankAcc,item.currency, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CollectionStatement_Results.CSV ");
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

        protected override void Dispose(bool disposing)
        {
            tb.Dispose();
            accb.Dispose();
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}