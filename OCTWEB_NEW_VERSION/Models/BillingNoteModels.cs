using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class BillingNoteModels
    {
        private DateTime _SetFromDate = DateTime.Today;
        private DateTime _SetToDate = DateTime.Today;
        private DateTime _SetCreateDate = DateTime.Today;
        private DateTime _SetDueDate = DateTime.Today;
        private string _SetDocumentNo = String.Concat("OCTB",DateTime.Today.ToString("yy"),"-",DateTime.Today.ToString("MM"),"-001");
        private string _SetAmountType = "THB";

        [Required]
        [Display(Name = "Customer")]
        public int customer_id { get; set; }

        [Required]
        [Display(Name = "From Date")]
        public DateTime fromDate {
            get
            {
                return _SetFromDate;
            }
            set
            {
                _SetFromDate = value;
            }
        }

        [Required]
        [Display(Name = "To Date")]
        public DateTime toDate {
            get
            {
                return _SetToDate;
            }
            set
            {
                _SetToDate = value;
            }
        }
        [Required]
        [Display(Name = "Due Date")]
        public DateTime dueDate
        {
            get
            {
                return _SetDueDate;
            }
            set
            {
                _SetDueDate = value;
            }
        }

        [Display(Name = "Document No.")]
        [RegularExpression(@"^[A-Z]{4}[0-9]{2}-[0-9]{2}-[0-9]{3}", ErrorMessage = "The field Document No. must match the regular expression - Ex. OCTB23-01-001")]
        public string billNo_str
        {
            get
            {
                return _SetDocumentNo;
            }
            set
            {
                _SetDocumentNo = value;
            }
        }

        [Required]
        [Display(Name = "Create Date")]
        public DateTime createDate
        {
            get
            {
                return _SetCreateDate;
            }
            set
            {
                _SetCreateDate = value;
            }
        }

        [Display(Name = "Acc")]
        public int? acc_id { get; set; }

        public int id { get; set; }
        public decimal GrandAmount { get; set; }
        public string GrandAmount_str { get; set; }
        public decimal GrandVat { get; set; }
        public string GrandVat_str { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public string GrandTotalAmount_str { get; set; }
        public string GrandTotalAmountTH_str { get; set; }
        public int count_page { get; set; }
        public string createDate_str { get; set; }
        public string dueDate_str { get; set; }
        public string creditTerm { get; set; }
        public bool status_print { get; set; } //status: 1 = print
        
        [Display(Name = "Amount Type")]
        public string amount_type
        {
            get
            {
                return _SetAmountType;
            }
            set
            {
                _SetAmountType = value;
            }
        } //AMOUNT(THB)

        public List<BillListModel> BillList { get; set; }
        public BillHeadModel BillHead { get; set; }
        public BillCustomerHeadModel BillCustomerHead { get; set; }
        public AccountBankModel AccountBank { get; set; }

        public List<SelectListItem> CustomerSelectList { get; set; }
        public List<SelectListItem> AccSelectList { get; set; }
               
    }

    public class BillListModel
    {
        public int id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceCreateDate { get; set; }
        public string InvoiceCreateDate_str { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string PaymentDueDate_str { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public decimal TotalAmount { get; set; }
        public string Amount_str { get; set; }
        public string Vat_str { get; set; }
        public string TotalAmount_str { get; set; }
        public int InvoiceHeaderId { get; set; }
        public string CustomerOrderNo_Remark { get; set; }
        
    }

    public class BillHeadModel
    {
        //CompanyCode	CompanyName1	CompanyName2	PostalCode	Address1	Address2	AddressTH1	AddressTH2	TelephoneNo1	FaxNo1	Logo	TaxId
        public string CompanyCode { get; set; }
        public string CompanyName1 { get; set; }
        public string CompanyName2 { get; set; }
        public string PostalCode { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string AddressTH1 { get; set; }
        public string AddressTH2 { get; set; }
        public string TelephoneNo1 { get; set; }
        public string FaxNo1 { get; set; }
        public byte[] Logo { get; set; }
        public string Logo_str { get; set; }
        public string TaxId { get; set; }
    }

    public class BillCustomerHeadModel
    {
        //CustomerName	Address1	Address2	PostalCode	CreditTerm	CustomerCode	CustomerTaxId	Tel	Fax			
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostalCode { get; set; }
        public decimal CreditTerm { get; set; }
        public string CustomerTaxId { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string PersonInCharge { get; set; }
    }

    public class AccountBankModel
    {
        //Id	BankName_EN	Address_EN	BankName_Thi	Address_Thi	AccOn	Branch	SwiftCode
        public int acc_id { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string AccNo { get; set; }
        public string BankBranch { get; set; }
        public string SwiftCode { get; set; }
    }

    public class BillReprintListModel
    {
        public string HeaderDate_str { get; set; }
        public DateTime? HeaderDate { get; set; }
        public string BillNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Amount_str { get; set; }
        public decimal Vat { get; set; }
        public string Vat_str { get; set; }
        public decimal Total { get; set; }
        public string Total_str { get; set; }
        public string DueDate_str { get; set; }
        public DateTime? DueDate { get; set; }
        public int CreditTerm { get; set; }
        public string Currency { get; set; }
        public string BankAcc { get; set; }

    }

    public class BillReprintModel
    {
        public string customerCode { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }

        public List<BillReprintListModel> BillReprintList { get; set; }

        public List<SelectListItem> SelectCustomerName { get; set; }
    }

}