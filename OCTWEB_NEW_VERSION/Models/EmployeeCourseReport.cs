using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class EmployeeCourseReport
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int emp_id { get; set; }
        public string search_topic { get; set; }
        public string search_location { get; set; }
        public List<SelectListItem> SelectTopic { get; set; }
        public List<SelectListItem> SelectLocation { get; set; }
        public DateTime? search_datestart { get; set; }
        public DateTime? search_dateend { get; set; }
        public double sum_hours { get; set; }
        public double sum_minutes { get; set; }
        public IPagedList<CourseReportDetail> CourseReportDetailPagedList { get; set; }
       
    }
    public class CourseReportDetail
    {
        public int course_id { get; set; }
        public string course_training_name { get; set; }
        public string location { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string start_date_str { get; set; }
        public string end_date_str { get; set; }
        public string time { get; set; }
        public string hours { get; set; }
        public string minutes { get; set; }
        public int? price { get; set; }
        public string training_by { get; set; }
        public string number_ws { get; set; }
        public string rev { get; set; }
        public string date_ws { get; set; }
        public string name_area { get; set; }

    }
}