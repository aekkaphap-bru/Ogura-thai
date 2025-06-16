using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class LogsReportModel
    {

        public int Log_Id { get; set; }

        public string Log_System { get; set; }
        public string Log_Type { get; set; }
        public string Log_Detail { get; set; }
        public string Log_Action { get; set; }
        public Nullable<System.DateTime> Log_Date { get; set; }
        public string Log_by { get; set; }
        public int Log_Action_Id { get; set; }
    }

    public class SearchLogs
    {
        private DateTime _SetDefaultDate = DateTime.Now;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime formDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime toDate { get; set; }

    }

    public class LogsReportPagedListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        [DisplayName("From Date")]
        [DataType(DataType.Date)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? fromdate { get; set; }

        [DisplayName("To Date")]
        [DataType(DataType.Date)]
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? todate { get; set; }

        public string searchSystem { get; set; }
        public string searchType { get; set; }
        public string searchAction { get; set; }
        public string searchDetail { get; set; }
        public IPagedList<LogsReportModel> LogsReportModelPagedList { get; set; }
    }
}