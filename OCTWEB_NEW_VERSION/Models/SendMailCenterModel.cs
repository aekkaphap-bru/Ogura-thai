using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class SendMailCenterModel
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> Tocc { get; set; }
        public string Subject { get; set; }
        public string Body  { get; set; }  
    }
}