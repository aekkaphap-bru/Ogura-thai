using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class BankAccountModels
    {
        public int id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name="Bank Name (EN)")]
        public string BankName_EN { get; set; }

        [Required]
        [MaxLength(3000)]
        [Display(Name = "Address (EN)")]
        public string Address_EN { get; set; }

        [MaxLength(100)]
        [Display(Name = "Bank Name (TH)")]
        public string BankName_TH { get; set; }

        [MaxLength(3000)]
        [Display(Name = "Address (TH)")]
        public string Address_TH { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Account No.")]
        public string Acc_No { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Branch")]
        public string Branch { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Swift Code")]
        public string SwiftCode { get; set; }
    }

    public class BankAccountListModel
    {
        public List<BankAccountModels> BankAccountList { get; set; }
    }

}