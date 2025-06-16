using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class UserGroupModel
    {
        public int USG_Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Group Name: ")]
        public string USG_Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Memo: ")]
        public string USG_Memo { get; set; }

        public int memberNumber { get; set; }
    }
    public class GroupMemberModel
    {
        public int GMB_Id { get; set; }
        public int USG_Id { get; set; }
        public int USE_Id { get; set; }
    }



}