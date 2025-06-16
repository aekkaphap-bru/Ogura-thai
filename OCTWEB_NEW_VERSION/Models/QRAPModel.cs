using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class ProblemModel
    {
        private DateTime _date = DateTime.Now;

        public int id { get; set; }
        [Required]
        [MaxLength(250)]
        [Display(Name="Problem")]
        public string detail { get; set; }

        public string file { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name="Reason")]
        public string reason { get; set; }

        [Required]
        [Display(Name="Date")]
        public DateTime date {
            get { return _date;}
            set { _date = value; }
        }
        public string date_str { get; set; }
        public int status { get; set; }

        [MaxLength(50)]
        [Display(Name="Owner")]
        public string owner { get; set; }

        [Required]
        [Display(Name="Department")]
        public int department_id { get; set; }
        public string department { get; set; }
        public string problem_no { get; set; }

        public List<SelectListItem> SelectDepartmet { get; set; }

        public List<SolutionModel> SolutionList { get; set; }
    }

    public class InformListModel //1
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? department_id { get; set; }
        public string problem_no { get; set; }
        public string detail { get; set; }
        public string owner { get; set; }
        public int? status { get; set; }

        public List<SelectListItem> SelectDepartmet { get; set; }
        public List<SelectListItem> SelectStatus { get; set; }
        public IPagedList<ProblemModel> problemPagedListModel { get; set; }

    }

    public class DefineSolutionListModel //2
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? department_id { get; set; }
        public string problem_no { get; set; }
        public string detail { get; set; }
        public string owner { get; set; }
        public int? status { get; set; }

        public List<SelectListItem> SelectDepartmet { get; set; }
        public List<SelectListItem> SelectStatus { get; set; }
        public IPagedList<ProblemModel> problemPagedListModel { get; set; }
    }

    public class SolutionModel
    {
        private DateTime _date = DateTime.Now;

        public int solution_id {get; set;}
        [Required]
        [MaxLength(250)]
        [Display(Name="Solution")]
        public string description {get; set;}

        [Required]
        [MaxLength(50)]
        [Display(Name = "Response Owner")]
        public string response_owner { get; set; }

        public string define_by { get; set; }
        public DateTime date {
            get { return _date; }
            set { _date = value; }
        }

        [Required]   
        [Display(Name = "Department")]
        public int? department_id { get; set; }
        public string department { get; set; }
        public bool need_evidence { get; set; }
        public int problem_id { get; set; }
        public string status { get; set; }
       
        public List<SelectListItem> SelectDepartmet { get; set; }
        public List<SelectListItem> SelectResponseOwner { get; set; }
    }

    public class AnswerListModel //3
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? department_id { get; set; }
        public string problem_no { get; set; }
        public string detail { get; set; }
        public string owner { get; set; }
        public int? status { get; set; }

        public List<SelectListItem> SelectDepartmet { get; set; }
        public List<SelectListItem> SelectStatus { get; set; }
        public IPagedList<ProblemModel> problemPagedListModel { get; set; }
        public List<ProblemModel> problemList { get; set; }
    }

    public class AnswerModel
    {
        public int answer_id { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name="Answer")]
        public string answer { get; set; }
        
        public string file_evidence { get; set; }       
        public DateTime? answer_date { get; set; }        
        public string approver { get; set; } //approve function
        public DateTime? approver_date { get; set; } //approve function
        public string comment { get; set; } //approve function
        public int? parent { get; set; } //solution_id
        public string parent_detail { get; set; }
        public string status { get; set; }
       
        //Problem
        public int problem_id { get; set; }
        public string problem_no { get; set; }
        public string problem_detail { get; set; }
        public string problem_reason { get; set; }
        public string problem_department { get; set; }
        public DateTime problem_date { get; set; }
        public string problem_file { get; set; }
        //Solution
        public int solution_id { get; set; }
        public string solution_detail { get; set; }
        public string solution_department { get; set; }
        public string solution_response_name { get; set; }
        public DateTime solution_date { get; set; }
        public bool need_evidence { get; set; }

    }

    public class ApproveListModel //4
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? department_id { get; set; }
        public string problem_no { get; set; }
        public string detail { get; set; }
        public string owner { get; set; }
        public int? status { get; set; }

        public List<SelectListItem> SelectDepartmet { get; set; }
        public List<SelectListItem> SelectStatus { get; set; }
        public IPagedList<ProblemModel> problemPagedListModel { get; set; }
        public List<ProblemModel> problemList { get; set; }
    }

    public class ApproveModel
    {
        public int answer_id { get; set; }
        public string answer { get; set; }

        public string file_evidence { get; set; }
        public DateTime? answer_date { get; set; }
        public string approver { get; set; } //approve function
        public DateTime? approver_date { get; set; } //approve function

        [MaxLength(250)]
        [Display(Name="Comment")]
        public string comment { get; set; } //approve function
        public int? parent { get; set; } //solution_id
        public string parent_detail { get; set; }
        public string status { get; set; }

        //Problem
        public int problem_id { get; set; }
        public string problem_no { get; set; }
        public string problem_detail { get; set; }
        public string problem_reason { get; set; }
        public string problem_department { get; set; }
        public DateTime problem_date { get; set; }
        public string problem_file { get; set; }
        //Solution
        public int solution_id { get; set; }
        public string solution_detail { get; set; }
        public string solution_department { get; set; }
        public string solution_response_name { get; set; }
        public DateTime solution_date { get; set; }
        public bool need_evidence { get; set; }

    }

    public class DataTableDetailModel
    {
        public int index { get; set; }
        public int solution_id { get; set; }
        public string status { get; set; }
        public string solution_detail { get; set; }
        public string plan_date { get; set; }
        public string department { get; set; }
        public string approver { get; set; }
        public string approve_date { get; set; }
        public string answer { get; set; }
        public string evidence_file { get; set; }
        public int rev { get; set; }
        public string comment { get; set; }
    }

    public class QRAPGraphModel
    {
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int graphType_id { get; set; }

        public List<SelectListItem> SelectGraphType { get; set; }
        public DataSetChartModel DataSetChart_0 { get; set; }
        public DataSetChartModel DataSetChart_1 { get; set; }
        public List<ProblemTableModel> ProblemTable { get; set; }
        public List<MissionTableModel> MissionTable { get; set; }
        public int problem_total { get; set; }
        public int mission_total { get; set; }
        public int mission_total_noanswer { get; set; }
        public int mission_total_answered { get; set; }
        public int mission_total_approved { get; set; }
        public int mission_total_approvedlate { get; set; }
    }

    public class ProblemTableModel
    {
        public int item { get; set; }
        public string department { get; set; }
        public int problem_count { get; set; }  
    }

    public class MissionTableModel
    {
        public int item { get; set; }
        public int? department_id { get; set; }
        public string department { get; set; }
        public int no_answer_count { get; set; }
        public int answered_count { get; set; }
        public int approved_count { get; set; }
        public int approved_late_count { get; set; }
        public int total_count { get; set; }
    }


    public class DataSetChartModel
    {
        public List<string> labels { get; set; }
        public List<DataSetModel> datasets { get; set; }
    }
    public class DataSetModel
    {
        public string label { get; set; }
        public List<int> data { get; set; }
        public bool fill { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public int borderWidth { get; set; }
    }

    public class QRAPDataJoinModel
    {
        public int pr_id { get; set; }
        public int so_id { get; set; }
        public int ev_id { get; set; }
        public string ev_status { get; set; }
        public int? department_id { get; set; }
        public string department_str { get; set; }
        public DateTime pr_plan_date { get; set; }
        public DateTime? so_plan_date { get; set; }
        public DateTime? ev_answer_date { get; set; }
        public DateTime? ev_approved_date { get; set; }
        public string pr_problem_no { get; set; }
        public string pr_problem_name { get; set; }
        public string pr_department { get; set; }
        public string so_response { get; set; }
        public string so_detail { get; set; }
        public string ev_answer { get; set; }
        public string ev_approver { get; set; }
        public string ev_evidence_file { get; set; }
        public int ev_rev { get; set; }
        public string ev_comment { get; set; }

    }

    public class MissionTableDetailModel
    {
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? department_id { get; set; }
        public int? status { get; set; }
        public List<SelectListItem> SelectDepartment { get; set; }
        public List<SelectListItem> SelectStatus { get; set; }
        public List<QRAPDataJoinModel> DataJoinList { get; set; }
    }
    //Items	Problem No.	Occur Date	Problem Name	Ocurr Department	
    //Solulion Detail	Plan Date	Response Department	Owner	Answer	Answer Date
    //Approver	Approved Date	Evidence	Revision	Comment


}