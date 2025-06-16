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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.BillingNote
{
    [Authorize]
    public class BillingNoteReprintController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private Thomas_Ogura_TESTDBEntities tb = new Thomas_Ogura_TESTDBEntities();
        private OCTIIS_WEBAPPEntities accb = new OCTIIS_WEBAPPEntities();
        
        //
        // GET: /BillingNoteReprint/BList
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
        //POST: /BillingNoteReprint/BList
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
                if(!String.IsNullOrEmpty(model.customerCode))
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

                List<BillReprintListModel> bill_list = query.GroupBy(s => new { s.DocumentDate, s.DueDate, s.DocumentNo, s.CustomerCode, s.CustomerName, s.Currency,s.Payment,s.MasterAccBankId })
                    .Select(g => new BillReprintListModel
                    {
                        HeaderDate = g.Key.DocumentDate,
                        HeaderDate_str = g.Key.DocumentDate.ToString("dd-MM-yyyy"),
                        DueDate = g.Key.DueDate,
                        DueDate_str = g.Key.DueDate.ToString("dd-MM-yyyy"),
                        BillNo = g.Key.DocumentNo,
                        CustomerCode = g.Key.CustomerCode,
                        CustomerName = g.Key.CustomerName,
                        Amount = g.Sum(x => Convert.ToDecimal(x.Amount)),
                        Amount_str = (g.Sum(x => Convert.ToDecimal(x.Amount))).ToString("N"),
                        Vat = g.Sum(x => Convert.ToDecimal(x.Vat)),
                        Vat_str = ( g.Sum(x => Convert.ToDecimal(x.Vat))).ToString("N"),
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
                    InvoiceCreateDate_str = s.InvoiceDate.ToString("dd-MM-yyyy"),
                    PaymentDueDate_str = s.DueDate.ToString("dd-MM-yyyy"),
                    Amount = Convert.ToDecimal(s.Amount),
                    Amount_str = Convert.ToDecimal(s.Amount).ToString("N"),
                    Vat = Convert.ToDecimal(s.Vat),
                    Vat_str = (Convert.ToDecimal(s.Vat)).ToString("N"),
                    TotalAmount = Convert.ToDecimal(s.Total),
                    TotalAmount_str = (Convert.ToDecimal(s.Total)).ToString("N"),
                }).OrderBy(o=>o.InvoiceNo).ToList();
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
                model.GrandTotalAmountTH_str = ThaiBahtText(grand_total_amount.ToString());
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
                model.dueDate_str = b_1.DueDate.ToString("dd-MM-yyyy");
                model.creditTerm = b_1.Payment.ToString("N0");
                model.billNo_str = b_1.DocumentNo;

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

        public BillingNoteModels PrepareData(BillingNoteModels model,string customer_code)
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

                   }).FirstOrDefault();

                //------------------------------------------------------------------------------------------------
                //Get Header 
                BillHeadModel head = tb.MasterCompanies.Where(x => x.CompanyCode == "OCTCOM")
                    .Select(s => new BillHeadModel
                    {
                        CompanyCode = s.CompanyCode,
                        CompanyName1 = s.CompanyName1,
                        CompanyName2 = s.CompanyName2,
                        PostalCode = s.PostalCode,
                        Address1 = s.Address1,
                        Address2 = s.Address2,
                        AddressTH1 = s.AddressTH1,
                        AddressTH2 = s.AddressTH2,
                        TelephoneNo1 = s.TelephoneNo1,
                        FaxNo1 = s.FaxNo1,
                        Logo = s.Logo,
                        TaxId = s.TaxId,
                    }).FirstOrDefault();
                //Convert byte array to base64string
                string imreBase64Data = Convert.ToBase64String(head.Logo);
                head.Logo_str = string.Format("data:image/jpg;base64,{0}", imreBase64Data);

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
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        [CustomAuthorize(66)]//Billing Note
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
                        Log_Type = "Billing Note",
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
            catch(Exception ex)
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
                Response.AddHeader("content-disposition", "attachment;filename=BillingNote_Reprint.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                MemoryStream stream = new MemoryStream();
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                CreatePage(model, document, "ORIGINAL", 0);
                CreatePage(model, document, "COPY", 1);


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
                    Log_Type = "Billing Note",
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
            var path_font_th1 = Server.MapPath("~/fonts/THSarabun.ttf");
            var path_font_th2 = Server.MapPath("~/fonts/THSarabun Bold.ttf");
            var path_font_eng1 = Server.MapPath("~/fonts/SEGOEUI.TTF");
            var path_font_eng2 = Server.MapPath("~/fonts/SEGOEUIB.ttf");

            PdfFont font_thai = PdfFontFactory.CreateFont(path_font_th1, PdfEncodings.IDENTITY_H);
            PdfFont font_thai_bold = PdfFontFactory.CreateFont(path_font_th2, PdfEncodings.IDENTITY_H);
            PdfFont font_eng = PdfFontFactory.CreateFont(path_font_eng1, PdfEncodings.IDENTITY_H);
            PdfFont font_eng_bold = PdfFontFactory.CreateFont(path_font_eng2, PdfEncodings.IDENTITY_H);

            int page_first = (model.count_page * page_num) + 1;
            int page_last = (model.count_page * page_num) + model.count_page;
            int r = 1;
            for (var i = page_first; i <= page_last; i++)
            {
                //------------Add Logo----------------------------------------------------------------------------------- 
                byte[] imgByte = model.BillHead.Logo;
                ImageData imgData = ImageDataFactory.Create(imgByte);
                Image logo = new Image(imgData);
                logo.SetFixedPosition(i, 20, 762, 220);
                document.Add(logo);

                //----------Company Header--------------------------------------------------------------------------------- 
                Paragraph company_eng = new Paragraph();
                company_eng.Add(new Text(model.BillHead.CompanyName1).SetFont(font_eng_bold).SetFontSize(10));
                company_eng.SetFixedPosition(i, 244, 808, 400);
                document.Add(company_eng);

                Paragraph company_th = new Paragraph();
                company_th.Add(new Text(model.BillHead.CompanyName2).SetFont(font_thai_bold).SetFontSize(10));
                company_th.SetFixedPosition(i, 244, 796, 400);
                document.Add(company_th);

                Paragraph compa_address_eng = new Paragraph();
                compa_address_eng.Add(new Text(model.BillHead.Address1 + " " + model.BillHead.Address2 + " " + model.BillHead.PostalCode).SetFont(font_thai).SetFontSize(10));
                compa_address_eng.SetFixedPosition(i, 244, 786, 400);
                document.Add(compa_address_eng);

                Paragraph compa_address_th = new Paragraph();
                compa_address_th.Add(new Text(model.BillHead.AddressTH1 + " " + model.BillHead.PostalCode).SetFont(font_thai).SetFontSize(10));
                compa_address_th.SetFixedPosition(i, 244, 776, 400);
                document.Add(compa_address_th);

                Paragraph compa_tel = new Paragraph();
                compa_tel.Add(new Text("TEL 038-650 880-4 Ext. 1203(Acc) FAX 038-650 879").SetFont(font_thai).SetFontSize(10));
                compa_tel.SetFixedPosition(i, 244, 766, 400);
                document.Add(compa_tel);

                Paragraph compa_tax = new Paragraph();
                compa_tax.Add(new Text("เลขที่ประจำตัวผู้เสียภาษีอากร/Tax No. " + model.BillHead.TaxId).SetFont(font_thai).SetFontSize(10));
                compa_tax.SetFixedPosition(i, 244, 756, 400);
                document.Add(compa_tax);


                Paragraph page_original = new Paragraph(page_status);
                page_original.SetFont(font_thai_bold).SetFontSize(14).SetFixedPosition(i, 512, 808, 100);
                document.Add(page_original);

                //-------Title--------------------------------------------------------------------------------------------
                Paragraph title_th = new Paragraph("ใบวางบิล");
                title_th.SetFont(font_thai_bold).SetFontSize(18).SetFixedPosition(i, 269, 729, 100);
                document.Add(title_th);
                Paragraph title_eng = new Paragraph("BILLING NOTE");
                title_eng.SetFont(font_thai_bold).SetFontSize(18).SetFixedPosition(i, 256, 716, 100);
                document.Add(title_eng);

                //------Customer Header--------------------------------------------------------------------------------------
                Paragraph bill_no_title = new Paragraph("เลขที่/No.");
                bill_no_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 700, 70);
                document.Add(bill_no_title);
                Paragraph bill_no = new Paragraph(model.billNo_str);
                bill_no.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 472, 700, 85);
                document.Add(bill_no);

                Paragraph cus_name_th_title = new Paragraph("ชื่อลูกค้า");
                cus_name_th_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 688, 70);
                document.Add(cus_name_th_title);
                Paragraph cus_name_eng = new Paragraph(model.BillCustomerHead.CustomerName);
                cus_name_eng.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 95, 688, 280);
                document.Add(cus_name_eng);
                Paragraph cus_name_eng_title = new Paragraph("Customer Name");
                cus_name_eng_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 676, 75);
                document.Add(cus_name_eng_title);
                Paragraph cus_tax_eng = new Paragraph("(TAX ID " + model.BillCustomerHead.CustomerTaxId + ")");
                cus_tax_eng.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 95, 676, 280);
                document.Add(cus_tax_eng);

                Paragraph date_title = new Paragraph("วันที่/Date");
                date_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 676, 70);
                document.Add(date_title);
                Paragraph date_create = new Paragraph(model.createDate_str);
                date_create.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 472, 676, 85);
                document.Add(date_create);

                Paragraph cus_address_thai_title = new Paragraph("ที่อยู่");
                cus_address_thai_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 658, 70);
                document.Add(cus_address_thai_title);
                Paragraph cus_address_1 = new Paragraph(model.BillCustomerHead.Address1);
                cus_address_1.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 95, 658, 350);
                document.Add(cus_address_1);
                Paragraph cus_address_2 = new Paragraph(model.BillCustomerHead.Address2 + " " + model.BillCustomerHead.PostalCode);
                cus_address_2.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 95, 646, 350);
                document.Add(cus_address_2);
                Paragraph cus_address_eng_title = new Paragraph("Address");
                cus_address_eng_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 646, 70);
                document.Add(cus_address_eng_title);

                Paragraph cus_code_title = new Paragraph("รหัสลูกค้า");
                cus_code_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 658, 70);
                document.Add(cus_code_title);
                Paragraph cus_code = new Paragraph(model.BillCustomerHead.CustomerCode);
                cus_code.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 472, 658, 85);
                document.Add(cus_code);
                Paragraph cus_code_eng_title = new Paragraph("Customer Code");
                cus_code_eng_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 646, 70);
                document.Add(cus_code_eng_title);

                Paragraph cus_credit_term_title = new Paragraph("เงื่อนไขการชำระ");
                cus_credit_term_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 628, 70);
                document.Add(cus_credit_term_title);
                Paragraph cus_credit_term = new Paragraph(model.creditTerm + " วัน");
                cus_credit_term.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 472, 628, 85);
                document.Add(cus_credit_term);
                Paragraph cus_credit_term_eng_title = new Paragraph("Credit Term");
                cus_credit_term_eng_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 400, 616, 70);
                document.Add(cus_credit_term_eng_title);

                Paragraph cus_tel_title = new Paragraph("โทรศัพท์/Tel.");
                cus_tel_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 616, 70);
                document.Add(cus_tel_title);
                Paragraph cus_tel = new Paragraph(model.BillCustomerHead.Tel);
                cus_tel.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 95, 616, 350);
                document.Add(cus_tel);

                Paragraph cus_fax_title = new Paragraph("Fax.");
                cus_fax_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 250, 616, 50);
                document.Add(cus_fax_title);
                Paragraph cus_fax = new Paragraph(model.BillCustomerHead.Fax);
                cus_fax.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 300, 616, 90);
                document.Add(cus_fax);

                //-----------Table----------------------------------------------------------------
                Table table = new Table(new float[] { 40, 105, 60, 60, 75, 75, 75, });
                table.SetFixedPosition(i, 20, 255, 555);
                // table.AddCell(new Cell(1, 3).Add(new Paragraph("Cell with colspan 3")));
                // table.AddCell(new Cell(15, 1).Add(new Paragraph("Cell with rowspan 1")));
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("เลขที่").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("No.").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("บิลเลขที่").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Invoice No.").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("วันที่").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Date").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("วันที่ครบกำหนด").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Due Date").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("จำนวนเงิน").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Amount").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("ภาษีมูลค่าเพิ่ม").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Vat 7%").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );
                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph("จำนวนเงิน").SetMultipliedLeading(1)).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Amount").SetMultipliedLeading(0.8f)).SetTextAlignment(TextAlignment.CENTER)
                    );

                int first_row = (r * 15) - 15;
                int last_row = r * 15;

                Cell column_no = new Cell(15, 1);
                column_no.SetTextAlignment(TextAlignment.CENTER);
                Cell column_invoiceno = new Cell(15, 1);
                column_invoiceno.SetTextAlignment(TextAlignment.LEFT);
                Cell column_date = new Cell(15, 1);
                column_date.SetTextAlignment(TextAlignment.CENTER);
                Cell column_duedate = new Cell(15, 1);
                column_duedate.SetTextAlignment(TextAlignment.CENTER);
                Cell column_amount = new Cell(15, 1);
                column_amount.SetTextAlignment(TextAlignment.RIGHT);
                Cell column_vat = new Cell(15, 1);
                column_vat.SetTextAlignment(TextAlignment.RIGHT);
                Cell column_totalamount = new Cell(15, 1);
                column_totalamount.SetTextAlignment(TextAlignment.RIGHT);


                for (int j = first_row; j < last_row; j++)
                {
                    BillListModel element = model.BillList.ElementAtOrDefault(j);
                    if (element != null)
                    {
                        int num = j + 1;
                        column_no.Add(new Paragraph(num.ToString()).SetMultipliedLeading(1.1f));
                        column_invoiceno.Add(new Paragraph(element.InvoiceNo).SetMultipliedLeading(1.1f));
                        column_date.Add(new Paragraph(element.InvoiceCreateDate_str).SetMultipliedLeading(1.1f));
                        column_duedate.Add(new Paragraph(model.dueDate_str).SetMultipliedLeading(1.1f));
                        column_amount.Add(new Paragraph(element.Amount_str).SetMultipliedLeading(1.1f));
                        column_vat.Add(new Paragraph(element.Vat_str).SetMultipliedLeading(1.1f));
                        column_totalamount.Add(new Paragraph(element.TotalAmount_str).SetMultipliedLeading(1.1f));
                    }
                    else
                    {
                        column_no.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_invoiceno.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_date.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_duedate.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_amount.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_vat.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                        column_totalamount.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                    }
                }

                table.AddCell(column_no);
                table.AddCell(column_invoiceno);
                table.AddCell(column_date);
                table.AddCell(column_duedate);
                table.AddCell(column_amount);
                table.AddCell(column_vat);
                table.AddCell(column_totalamount);

                //--------Footer---------------------------
                Cell grend_total = new Cell(1, 4);
                grend_total.Add(new Paragraph("ยอดรวมทั้งสิ้น / Grand Total")).SetTextAlignment(TextAlignment.CENTER);
                Cell sum_amount = new Cell();
                Cell sum_vat = new Cell();
                Cell sum_total = new Cell();
                Cell grend_total_str = new Cell(2, 7);
                grend_total_str.Add(new Paragraph("จำนวนเงินตัวอักษร / Amount In Letters").SetMultipliedLeading(1.1f));

                if (r == model.count_page)
                {
                    sum_amount.Add(new Paragraph(model.GrandAmount_str).SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    sum_vat.Add(new Paragraph(model.GrandVat_str).SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    sum_total.Add(new Paragraph(model.GrandTotalAmount_str).SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    grend_total_str.Add(new Paragraph(model.GrandTotalAmountTH_str).SetMultipliedLeading(1.1f));
                }
                else
                {
                    sum_amount.Add(new Paragraph("\n").SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    sum_vat.Add(new Paragraph("\n").SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    sum_total.Add(new Paragraph("\n").SetTextAlignment(TextAlignment.RIGHT)).SetBorderBottom(new DoubleBorder(2));
                    grend_total_str.Add(new Paragraph("\n").SetMultipliedLeading(1.1f));
                }

                table.AddCell(grend_total);
                table.AddCell(sum_amount);
                table.AddCell(sum_vat);
                table.AddCell(sum_total);
                table.AddCell(grend_total_str);

                table.SetFont(font_thai).SetFontSize(12);

                document.Add(table);

                //-----------Footer----------------------------------------------------------------------
                //-----------Document----------------------------------
                Paragraph foot_doc_title = new Paragraph("เอกสารที่ใช้ในการวางบิล");
                foot_doc_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 230, 150);
                document.Add(foot_doc_title);
                Paragraph foot_doc_eng_title = new Paragraph("Document for Billing");
                foot_doc_eng_title.SetFont(font_thai_bold).SetFontSize(12).SetFixedPosition(i, 20, 215, 150);
                document.Add(foot_doc_eng_title);

                Paragraph foot_doc_title_1 = new Paragraph("ต้นฉบับใบกำกับภาษี / ใบแจ้งหนี้ / ใบส่งสินค้า");
                foot_doc_title_1.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 200, 180);
                document.Add(foot_doc_title_1);
                Paragraph foot_doc_title_1_eng = new Paragraph("Original Tax Inv. / Invoice / Delivery Order");
                foot_doc_title_1_eng.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 185, 180);
                document.Add(foot_doc_title_1_eng);
                Paragraph foot_doc_title_1_eng_ = new Paragraph("__________");
                foot_doc_title_1_eng_.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 20, 185, 40);
                document.Add(foot_doc_title_1_eng_);

                Paragraph foot_doc_title_2 = new Paragraph("สำเนาใบกำกับภาษี / ใบแจ้งหนี้ / ใบส่งสินค้า");
                foot_doc_title_2.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 170, 180);
                document.Add(foot_doc_title_2);
                Paragraph foot_doc_title_2_eng = new Paragraph("Copy Tax Inv. / Invoice / Delivery Order");
                foot_doc_title_2_eng.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 155, 180);
                document.Add(foot_doc_title_2_eng);
                Paragraph foot_doc_title_2_eng_ = new Paragraph("__________");
                foot_doc_title_2_eng_.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 20, 155, 40);
                document.Add(foot_doc_title_2_eng_);

                Paragraph foot_doc_title_3 = new Paragraph("ต้นฉบับใบวางบิล / Original Billing receipt");
                foot_doc_title_3.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 140, 180);
                document.Add(foot_doc_title_3);
                Paragraph foot_doc_title_3_ = new Paragraph("__________");
                foot_doc_title_3_.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 20, 140, 40);
                document.Add(foot_doc_title_3_);

                Paragraph foot_doc_title_4 = new Paragraph("สำเนาใบวางบิล / Copy Billing receipt");
                foot_doc_title_4.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 60, 125, 180);
                document.Add(foot_doc_title_4);
                Paragraph foot_doc_title_4_ = new Paragraph("__________");
                foot_doc_title_4_.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 20, 125, 40);
                document.Add(foot_doc_title_4_);

                //----------Bank -----------------------
                Paragraph foot_bank_title_1 = new Paragraph("โปรดชำระเงินด้วยเช็คขีดคร่อม 'เข้าบัญชีผู้รับเท่านั้น' สั่งจ่ายในนาม");
                foot_bank_title_1.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 240, 230, 300);
                document.Add(foot_bank_title_1);
                Paragraph foot_bank_title_2 = new Paragraph("บริษัท โอกุระ คลัทช์ (ไทยแลนด์) จำกัด");
                foot_bank_title_2.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 215, 300);
                document.Add(foot_bank_title_2);
                Paragraph foot_bank_title_1_eng = new Paragraph("Please make payment by crossed cheque 'Account payee only' Payable to ");
                foot_bank_title_1_eng.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 240, 200, 300);
                document.Add(foot_bank_title_1_eng);
                Paragraph foot_bank_title_2_eng = new Paragraph("Ogura Clutch (Thailand) Co.,Ltd.");
                foot_bank_title_2_eng.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 185, 300);
                document.Add(foot_bank_title_2_eng);


                //---------Bank Seleced-----------------
                Paragraph foot_bank_1 = new Paragraph(model.AccountBank.BankName + "\n By transfering to Bank A/C Number: "
                    + model.AccountBank.AccNo + "\n"
                    + model.AccountBank.BankBranch + " " + model.AccountBank.BankAddress);
                foot_bank_1.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 110, 300).SetMultipliedLeading(1); //155
                document.Add(foot_bank_1);

                /*Paragraph foot_bank_2 = new Paragraph("By transfering to Bank A/C Number: "+ model.AccountBank.AccNo);
                foot_bank_2.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 140, 300);
                document.Add(foot_bank_2);
                Paragraph foot_bank_3 = new Paragraph(model.AccountBank.BankBranch+" "+model.AccountBank.BankAddress);
                foot_bank_3.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 240, 110, 300).SetMultipliedLeading(1);
                document.Add(foot_bank_3);*/

                //-------Signed-------------------------
                Paragraph foot_duedate_title = new Paragraph("นัดชำระเงิน");
                foot_duedate_title.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 20, 69, 40);
                document.Add(foot_duedate_title);
                Paragraph foot_duedate_eng_title = new Paragraph("Due date");
                foot_duedate_eng_title.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 20, 57, 40);
                document.Add(foot_duedate_eng_title);
                Paragraph foot_duedate = new Paragraph("_______/_______/_______");
                foot_duedate.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 60, 69, 100);
                document.Add(foot_duedate);

                Paragraph foot_receive_title = new Paragraph("ผู้รับวางบิล");
                foot_receive_title.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 190, 69, 60);
                document.Add(foot_receive_title);
                Paragraph foot_receive_eng_title = new Paragraph("Receive By");
                foot_receive_eng_title.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 190, 57, 60);
                document.Add(foot_receive_eng_title);
                Paragraph foot_receive = new Paragraph("_______________________");
                foot_receive.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 250, 69, 100);
                document.Add(foot_receive);
                Paragraph foot_receivedate_title = new Paragraph("Date");
                foot_receivedate_title.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 190, 45, 60);
                document.Add(foot_receivedate_title);
                Paragraph foot_receivedate = new Paragraph("_______/_______/_______");
                foot_receivedate.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 250, 45, 100);
                document.Add(foot_receivedate);

                Paragraph foot_bill_title = new Paragraph("ผู้วางบิล");
                foot_bill_title.SetFont(font_thai_bold).SetFontSize(10).SetFixedPosition(i, 380, 69, 40);
                document.Add(foot_bill_title);
                Paragraph foot_bill_eng_title = new Paragraph("Bill Collector");
                foot_bill_eng_title.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 380, 57, 60);
                document.Add(foot_bill_eng_title);
                Paragraph foot_bill = new Paragraph("_______________________");
                foot_bill.SetFont(font_thai).SetFontSize(12).SetFixedPosition(i, 440, 69, 100);
                document.Add(foot_bill);
                Paragraph foot_billdate_title = new Paragraph("Date");
                foot_billdate_title.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 380, 45, 60);
                document.Add(foot_billdate_title);
                Paragraph foot_billdate = new Paragraph("_______/_______/_______");
                foot_billdate.SetFont(font_thai).SetFontSize(10).SetFixedPosition(i, 440, 45, 100);
                document.Add(foot_billdate);

                r++;
            }

            return document;
        }

        public string ThaiBahtText(string strNumber, bool IsTrillion = false)
        {

            string BahtText = "";
            string strTrillion = "";
            string[] strThaiNumber = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
            string[] strThaiPos = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };

            decimal decNumber = 0;
            decimal.TryParse(strNumber, out decNumber);

            if (decNumber == 0)
            {
                return "ศูนย์บาทถ้วน";
            }

            strNumber = decNumber.ToString("0.00");
            string strInteger = strNumber.Split('.')[0];
            string strSatang = strNumber.Split('.')[1];

            if (strInteger.Length > 13)
                throw new Exception("รองรับตัวเลขได้เพียง ล้านล้าน เท่านั้น!");

            bool _IsTrillion = strInteger.Length > 7;
            if (_IsTrillion)
            {
                strTrillion = strInteger.Substring(0, strInteger.Length - 6);
                BahtText = ThaiBahtText(strTrillion, _IsTrillion);
                strInteger = strInteger.Substring(strTrillion.Length);
            }

            int strLength = strInteger.Length;
            for (int i = 0; i < strInteger.Length; i++)
            {
                string number = strInteger.Substring(i, 1);
                if (number != "0")
                {
                    if (i == strLength - 1 && number == "1" && strLength != 1)
                    {
                        BahtText += "เอ็ด";
                    }
                    else if (i == strLength - 2 && number == "2" && strLength != 1)
                    {
                        BahtText += "ยี่";
                    }
                    else if (i != strLength - 2 || number != "1")
                    {
                        BahtText += strThaiNumber[int.Parse(number)];
                    }

                    BahtText += strThaiPos[(strLength - i) - 1];
                }
            }

            if (IsTrillion)
            {
                return BahtText + "ล้าน";
            }

            if (strInteger != "0")
            {
                BahtText += "บาท";
            }

            if (strSatang == "00")
            {
                BahtText += "ถ้วน";
            }
            else
            {
                strLength = strSatang.Length;
                for (int i = 0; i < strSatang.Length; i++)
                {
                    string number = strSatang.Substring(i, 1);
                    if (number != "0")
                    {
                        if (i == strLength - 1 && number == "1" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "เอ็ด";
                        }
                        else if (i == strLength - 2 && number == "2" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "ยี่";
                        }
                        else if (i != strLength - 2 || number != "1")
                        {
                            BahtText += strThaiNumber[int.Parse(number)];
                        }

                        BahtText += strThaiPos[(strLength - i) - 1];
                    }
                }

                BahtText += "สตางค์";
            }

            return BahtText;
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
                    bankAcc = "\""+ v.BankAcc + "\""

                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    "Items", "Date","DocumentNo", "CustomerCode","CustomerName", "Amount", "Vat", "Total Amount","Due Date","CreditTerm","Bank","Currency", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        item.item, item.date, item.documentNo, item.customer_code, item.customer_name, item.amount, item.vat, item.totalAmount, item.dueDate,item.creditTerm,item.bankAcc,item.currency, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=BillingNote_Results.CSV ");
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