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
    public class CollectionStatementController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private Thomas_Ogura_TESTDBEntities tb = new Thomas_Ogura_TESTDBEntities();
        private OCTIIS_WEBAPPEntities accb = new OCTIIS_WEBAPPEntities();
        //
        // GET: /CollectionStatement/BList
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
            BillingNoteModels model = new BillingNoteModels();
            //Select Customer
            var customer_select = tb.MasterCustomers.Where(x => x.IsActive).OrderBy(o => o.CustomerCode).ToList();
            List<SelectListItem> customerList = customer_select
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = String.Concat(s.CustomerCode, " ", s.MasterBusinessPartner.BusinessPartnerName)
                }).ToList();

            //Select Acc-Back Account
            var acc_select = accb.Acc_Bank.ToList();
            List<SelectListItem> accList = acc_select
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = String.Concat(s.BankName_EN, " ", s.Branch),
                }).ToList();

            List<BillListModel> billList = new List<BillListModel>();
            BillHeadModel billHead = new BillHeadModel();
            BillCustomerHeadModel billCustomerHead = new BillCustomerHeadModel();

            model.BillList = billList;
            model.BillHead = billHead;
            model.BillCustomerHead = billCustomerHead;
            model.CustomerSelectList = customerList;
            model.AccSelectList = accList;

            return View(model);
        }

        //
        //POST: /CollectionStatement/BList
        [HttpPost]
        [CustomAuthorize(66)]//Billing Note
        public ActionResult BList(FormCollection form, BillingNoteModels model)
        {
            try
            {
                //Select Customer
                var customer_select = tb.MasterCustomers.Where(x => x.IsActive).OrderBy(o => o.CustomerCode).ToList();
                List<SelectListItem> customerList = customer_select
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = String.Concat(s.CustomerCode, " ", s.MasterBusinessPartner.BusinessPartnerName)
                    }).ToList();
                //Select Acc-Back Account
                var acc_select = accb.Acc_Bank.ToList();
                List<SelectListItem> accList = acc_select
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = String.Concat(s.BankName_EN, " ", s.Branch),
                    }).ToList();

                model.CustomerSelectList = customerList;
                model.AccSelectList = accList;
                //Set Print
                model.status_print = true;

                if (!ModelState.IsValid)
                {
                    List<BillListModel> billList_1 = new List<BillListModel>();
                    BillHeadModel billHead_1 = new BillHeadModel();
                    BillCustomerHeadModel billCustomerHead_1 = new BillCustomerHeadModel();

                    model.BillList = billList_1;
                    model.BillHead = billHead_1;
                    model.BillCustomerHead = billCustomerHead_1;
                    return View(model);
                }
                //For Print PDF
                if (form["PrintData"] == "PrintData")
                {

                    if (!model.acc_id.HasValue)
                    {
                        ModelState.AddModelError("acc_id", "The Bank Account field is required.");
                        GetData(model);
                        return View(model);
                    }
                    if (String.IsNullOrEmpty(model.billNo_str))
                    {
                        ModelState.AddModelError("billNo_str", "The Document No. field is required.");
                        GetData(model);
                        return View(model);
                    }
                    if (String.IsNullOrEmpty(model.amount_type))
                    {
                        ModelState.AddModelError("amount_type", "The Amount Type field is required.");
                        GetData(model);
                        return View(model);
                    }
                    //Check bill_No duplicate
                    var bill_no_check = accb.Acc_BillingDetails.Where(x => x.DocumentNo == model.billNo_str).FirstOrDefault();
                    if (bill_no_check != null)
                    {
                        ModelState.AddModelError("billNo_str", "The document number is already in use.");
                        GetData(model);
                        return View(model);
                    }

                    var selectedItem = form["selectedItem"];
                    if (selectedItem == null)
                    {
                        GetData(model);
                        ViewBag.errorMessage = "Please select an item.";
                        return View(model);
                    }

                    PrepareData(model, selectedItem);
                    ExportPDF(model);

                    ViewBag.Message = "Successfully, bill created.";
                    return View(model);
                }
                else if (form["ExportCSV"] == "ExportCSV")
                {
                    GetDataForCSV(model);
                    ExportToCsv(model);
                }
                else
                {
                    GetData(model);
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

        public BillingNoteModels GetData(BillingNoteModels model)
        {
            try
            {
                //Invoice
                var query_bill = tb.InvoiceHeaders
                    .Where(x => x.MasterCustomerId == model.customer_id
                        && x.InvoiceCreateDate >= model.fromDate
                        && x.InvoiceCreateDate <= model.toDate).ToList();
                List<BillListModel> billList = query_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceNo,
                    InvoiceCreateDate = s.InvoiceCreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                }).ToList();
                //Invoice Debit
                var query_debit_bill = tb.InvoiceDebitCreditNoteHeaders
                    .Where(x => x.MasterCustomerId == model.customer_id
                        && x.CreateDate >= model.fromDate
                        && x.CreateDate <= model.toDate).ToList();
                List<BillListModel> billList_debit = query_debit_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceCreditDebitNoteNo,
                    InvoiceCreateDate = s.CreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                }).ToList();
                //Merge
                List<BillListModel> b_list = new List<BillListModel>();
                if (billList_debit.Any())
                {
                    b_list = billList_debit.Concat(billList).ToList();
                }
                else
                {
                    b_list = billList;
                }

                //Check Old printed
                var invoice_no_list = b_list.Select(s => s.InvoiceNo).ToList();
                var bill_old = accb.Acc_BillingDetails.Where(x => invoice_no_list.Contains(x.InvoiceNo))
                    .Select(s => s.DocumentNo).ToList();
                if (bill_old.Any())
                {
                    var b_old = b_list.Where(x => bill_old.Contains(x.InvoiceNo)).ToList();
                    b_list = b_list.Except(b_old).ToList();
                }

                model.BillList = b_list.OrderBy(o => o.InvoiceNo).ToList();

                //Set Duedate
                var b_1 = b_list.FirstOrDefault();
                if (b_1 != null)
                {
                    model.dueDate = b_1.PaymentDueDate;
                }
                //Set Bill No.
                string bill_no = String.Concat("OCTB", model.createDate.ToString("yy"), "-", model.createDate.ToString("MM"), "-");
                var bill_old_list = accb.Acc_BillingDetails.Where(x => x.DocumentNo.Contains(bill_no))
                    .OrderByDescending(o => o.DocumentNo).ToList();
                var bill_no_old = bill_old_list.Select(s => s.DocumentNo).FirstOrDefault();
                if (bill_no_old != null)
                {
                    var number = bill_no_old.Split('-').Last();
                    int number_int = Convert.ToInt32(number);
                    int number_count = number_int + 1;
                    model.billNo_str = String.Concat(bill_no, number_count.ToString("000"));

                }

                return model;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        public BillingNoteModels GetDataForCSV(BillingNoteModels model)
        {
            try
            {
                //--------------------------------------------------------------------------------------------
                //Get Header Customer
                BillCustomerHeadModel head_customer = tb.MasterCustomers.Where(x => x.Id == model.customer_id)
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
                model.BillCustomerHead = head_customer;

                //----------------------------------------------------------------------------------------------
                //Invoice
                var query_bill = tb.InvoiceHeaders
                    .Where(x => x.MasterCustomerId == model.customer_id
                        && x.InvoiceCreateDate >= model.fromDate
                        && x.InvoiceCreateDate <= model.toDate).ToList();
                List<BillListModel> billList = query_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceNo,
                    InvoiceCreateDate = s.InvoiceCreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                    InvoiceHeaderId = s.Id
                }).ToList();
                //Invoice Debit
                var query_debit_bill = tb.InvoiceDebitCreditNoteHeaders
                    .Where(x => x.MasterCustomerId == model.customer_id
                        && x.CreateDate >= model.fromDate
                        && x.CreateDate <= model.toDate).ToList();
                List<BillListModel> billList_debit = query_debit_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceCreditDebitNoteNo,
                    InvoiceCreateDate = s.CreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                    InvoiceHeaderId = s.Id,
                }).ToList();
                //Merge
                List<BillListModel> b_list = new List<BillListModel>();
                if (billList_debit.Any())
                {
                    b_list = billList_debit.Concat(billList).ToList();
                }
                else
                {
                    b_list = billList;
                }
              
                //Set Remark
                foreach (var b in b_list)
                {
                    var shipping_query = (from sah in tb.ShippingActualHeaders
                                          join sad in tb.ShippingActualDetails on sah.Id equals sad.ShippingActualHeaderId
                                          where sah.InvoiceHeaderId == b.InvoiceHeaderId
                                          select sad.CustomerOrderDetail.CustomerOrderHeaderId).ToList();
                    if (shipping_query.Any())
                    {
                        var customerOrder_query = (from coh in tb.CustomerOrderHeaders
                                                   where shipping_query.Contains(coh.Id)
                                                   select coh.CustomerOrderNo).ToList();
                        var po_no = customerOrder_query.Distinct();
                        b.CustomerOrderNo_Remark = String.Join(",", po_no);
                    }
                }
                model.BillList = b_list.OrderBy(o => o.InvoiceNo).ToList();
               
                return model;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }


        public BillingNoteModels PrepareData(BillingNoteModels model, string selectedItem)
        {
            try
            {
                List<string> invoice_list = new List<string>();

                invoice_list = selectedItem.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s).ToList();

                //Invoice
                var query_bill = tb.InvoiceHeaders.Where(x => invoice_list.Contains(x.InvoiceNo)).ToList();
                List<BillListModel> billList = query_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceNo,
                    InvoiceCreateDate = s.InvoiceCreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                }).ToList();

                //Invoice Debit
                var query_debit_bill = tb.InvoiceDebitCreditNoteHeaders.Where(x => invoice_list.Contains(x.InvoiceCreditDebitNoteNo)).ToList();
                List<BillListModel> billList_debit = query_debit_bill.Select(s => new BillListModel
                {
                    InvoiceNo = s.InvoiceCreditDebitNoteNo,
                    InvoiceCreateDate = s.CreateDate,
                    PaymentDueDate = s.PaymentDueDate,
                    Amount = s.TotalAmount,
                    Amount_str = s.TotalAmount.ToString("N"),
                    Vat = s.Vat,
                    Vat_str = s.Vat.ToString("N"),
                    TotalAmount = s.GrandTotal,
                    TotalAmount_str = s.GrandTotal.ToString("N"),
                }).ToList();

                List<BillListModel> b_list = new List<BillListModel>();
                if (billList_debit.Any())
                {
                    b_list = billList_debit.Concat(billList).ToList();
                }
                else
                {
                    b_list = billList;
                }

                //--------------------------------------------------------------------------------------------
                //Get Header Customer
                BillCustomerHeadModel head_customer = tb.MasterCustomers.Where(x => x.Id == model.customer_id)
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

                model.BillHead = head;
                model.BillCustomerHead = head_customer;
                model.BillList = b_list.OrderBy(o => o.InvoiceNo).ToList();
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
                Response.AddHeader("content-disposition", "attachment;filename=CollectionStatement.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                MemoryStream stream = new MemoryStream();
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                CreatePage(model, document, "ORIGINAL", 0);
                
                document.Close();
                Response.BinaryWrite(stream.ToArray());
                Response.End();

                //Save Records
                var bill_list = model.BillList;
                List<Acc_BillingDetails> save_bill_list = new List<Acc_BillingDetails>();
                foreach (var b in bill_list)
                {
                    Acc_BillingDetails bill = new Acc_BillingDetails();
                    bill.DocumentDate = model.createDate;
                    bill.DocumentNo = model.billNo_str;
                    bill.InvoiceNo = b.InvoiceNo;
                    bill.CustomerCode = model.BillCustomerHead.CustomerCode;
                    bill.CustomerName = model.BillCustomerHead.CustomerName;
                    bill.InvoiceDate = b.InvoiceCreateDate;
                    bill.DueDate = model.dueDate;
                    bill.Amount = Decimal.ToDouble(b.Amount);
                    bill.Vat = Decimal.ToDouble(b.Vat);
                    bill.Total = Decimal.ToDouble(b.TotalAmount);
                    bill.MasterAccBankId = model.acc_id.Value;
                    bill.Payment = Decimal.ToInt32(model.BillCustomerHead.CreditTerm);
                    bill.Currency = model.amount_type;

                    save_bill_list.Add(bill);
                }
                if (save_bill_list.Any())
                {
                    accb.Acc_BillingDetails.AddRange(save_bill_list);
                    accb.SaveChanges();

                    //Save Logs
                    string user_nickname = null;
                    if (Session["NickName"] != null)
                    {
                        user_nickname = Session["NickName"].ToString();
                    }
                    Log logmodel = new Log()
                    {
                        Log_Action = "print",
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
                Paragraph date_create = new Paragraph(model.createDate.ToString("dd-MM-yyyy"));
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
                Table table = new Table(new float[] { 150, 100, 169, 100});
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
                Paragraph foot_bank_1 = new Paragraph("TRANFER BANK: \n"+ model.AccountBank.BankName
                    + "\n " + model.AccountBank.BankAddress 
                    + "\n ACC.NO.: " + model.AccountBank.AccNo 
                    + "\n BRANCH: " + model.AccountBank.BankBranch
                    + "\n SWIFT CODE: "+ model.AccountBank.SwiftCode
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

        public void ExportToCsv(BillingNoteModels model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var data = model.BillList;

                var forexport = data.Select((v, i) => new
                {
                    item = i + 1,
                    invoiceNo = v.InvoiceNo,
                    date = v.InvoiceCreateDate.ToString("yyyy-MM-dd"),
                    dueDate = v.PaymentDueDate.ToString("yyyy-MM-dd"),
                    amount = v.Amount,
                    vat = v.Vat,
                    totalAmount = v.TotalAmount,
                    remark = "\"" + v.CustomerOrderNo_Remark + "\"",

                });

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    "No.","CustomerCode","CustomerName","TermCode", "Invoice No", "Date", "Due Date", "Amount", "Vat", "Total Amount","Remark", Environment.NewLine);

                foreach (var item in forexport)
                {
                    sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        item.item, model.BillCustomerHead.CustomerCode, "\""+model.BillCustomerHead.CustomerName+"\"", model.BillCustomerHead.CreditTerm,
                        item.invoiceNo, item.date, item.dueDate, item.amount, item.vat, item.totalAmount, item.remark, Environment.NewLine);
                }

                //Get Current Response  
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                Response.Charset = "windows-874";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(874);
                response.AddHeader("content-disposition", "attachment;filename=CollectionStatement.CSV ");
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