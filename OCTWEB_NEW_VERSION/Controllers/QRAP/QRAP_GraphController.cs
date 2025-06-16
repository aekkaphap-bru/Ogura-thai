using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.QRAP
{
    [Authorize]
    public class QRAP_GraphController : Controller
    {
        private string path_qrap_answers = ConfigurationManager.AppSettings["path_qrap_answers"];

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        //
        // GET: /QRAP_Graph/
        [CustomAuthorize(9)]//Problem Graph
        public ActionResult Graph(QRAPGraphModel model)
        {
            try
            {
                if (TempData["shortMessage"] != null)
                {
                    ViewBag.Message = TempData["shortMessage"].ToString();
                }              
                if (model.graphType_id == 0)
                {
                    GetProblemChart(model);                   
                }
                else
                {
                    GetMissionChart(model);
                }
                //Select Graph Type
                List<SelectListItem> selectGraphType = new List<SelectListItem>();
                selectGraphType.Add(new SelectListItem { Value = "0", Text = "Problem" });
                selectGraphType.Add(new SelectListItem { Value = "1", Text = "Mission" });

                model.SelectGraphType = selectGraphType;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //QRAP_Graph/Graph {0}", ex.ToString());
                return View("Error");
            } 
        }
        //
        // POST: /QRAP_Graph/
        [HttpPost]
        [CustomAuthorize(9)]//Problem Graph
        public ActionResult Graph(FormCollection form,QRAPGraphModel model)
        {
            try
            {
                if (model.graphType_id == 0)
                {
                    GetProblemChart(model);
                }
                else
                {  
                    GetMissionChart(model);
                }
                //Select Graph Type
                List<SelectListItem> selectGraphType = new List<SelectListItem>();
                selectGraphType.Add(new SelectListItem { Value = "0", Text = "Problem" });
                selectGraphType.Add(new SelectListItem { Value = "1", Text = "Mission" });

                model.SelectGraphType = selectGraphType;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //QRAP_Graph/Graph {0}", ex.ToString());
                return View("Error");
            }
        }

        public QRAPGraphModel GetProblemChart(QRAPGraphModel model)
        {
            DateTime? start_date = model.start_date;
            DateTime? end_date = model.end_date;

            IEnumerable<CM_Problem> query = db.CM_Problem;
            if (start_date.HasValue)
            {
                query = query.Where(x => x.PRB_PlanDate >= start_date.Value);
            }
            if (end_date.HasValue)
            {
                query = query.Where(x => x.PRB_PlanDate <= end_date.Value);
            }
            //Group Count Problem
            var groupDept = (from pr in query
                             join dep in db.Departments on pr.Dep_Id equals dep.Dep_Id
                             group dep by dep.Dep_Name into g
                             select new { department_str = g.Key, count_problem = g.Count() });
            //Group Count Problem Department String
            var groupDept_model = groupDept
                .Select(s => new
                {
                    department_str = s.department_str,
                    count_problem = s.count_problem,
                }).OrderBy(o => o.department_str).ToList();

            //Set to model
            DataSetChartModel dept_chart_model = new DataSetChartModel();
            List<string> labels = new List<string>();
            List<int> values = new List<int>();

            foreach (var item in groupDept_model)
            {
                labels.Add(item.department_str);
                values.Add(item.count_problem);
            }
            dept_chart_model.labels = labels;
            List<DataSetModel> dataset_list = new List<DataSetModel>();
            DataSetModel dataset = new DataSetModel();
            dataset.label = "Problem (Times)";
            dataset.data = values;
            dataset.fill = false;
            dataset.backgroundColor = "rgb(111,168,220)";
            dataset.borderColor = "rgb(54, 162, 235)";
            dataset.borderWidth = 1;
            dataset_list.Add(dataset);
            dept_chart_model.datasets = dataset_list;


            //Set ProblemTableModel
            List<ProblemTableModel> problem_table = groupDept_model
                .Select((val, i) => new ProblemTableModel
                {
                    item = i,
                    department = val.department_str,
                    problem_count = val.count_problem,
                }).ToList();

            model.DataSetChart_0 = dept_chart_model;
            model.ProblemTable = problem_table;
            model.problem_total = values.Sum();

            return model;
        }


        public QRAPGraphModel GetMissionChart(QRAPGraphModel model)
        {
            DateTime? start_date = model.start_date;
            DateTime? end_date = model.end_date;

            IEnumerable<CM_Problem> query = db.CM_Problem;
            if (start_date.HasValue)
            {
                query = query.Where(x => x.PRB_PlanDate >= start_date.Value);
            }
            if (end_date.HasValue)
            {
                query = query.Where(x => x.PRB_PlanDate <= end_date.Value);
            }
            var result_join = (from pr in query
                               join so in db.CM_TodoList on pr.PRB_Id equals so.PRB_Id
                               join ev in db.CM_TodoEvidence on so.TDO_Id equals ev.TDO_Id
                               join dept in db.Departments on so.Dep_Id equals dept.Dep_Id
                               select new QRAPDataJoinModel
                               {
                                    pr_id = pr.PRB_Id,
                                    so_id = so.TDO_Id,
                                    ev_id = ev.TDE_Id,
                                    ev_status = ev.TDE_Status,
                                    department_id = so.Dep_Id,
                                    department_str = dept.Dep_Name,
                                    pr_plan_date = pr.PRB_PlanDate,
                                    so_plan_date = so.TDO_PlanDate,
                                    ev_answer_date = ev.TDE_AnswerDate,
                                    ev_approved_date = ev.TDE_ApprovedDate,
                               }).ToList();

            //Create Status: 3 (Approved late)
            result_join.ForEach(f => { if (f.ev_approved_date < f.so_plan_date) { f.ev_status = "3"; } });
            
            //Group by Status
            var result_group = (from dd in result_join
                               group dd by new { dd.department_str, dd.ev_status , dd.department_id} into g
                               select new
                               {
                                   department_id = g.Key.department_id,
                                   deparment_str = g.Key.department_str,
                                   ev_status = g.Key.ev_status,
                                   value_count = g.Count()
                               }).OrderBy(o=>o.deparment_str);
           
            
            List<string> temp_labels = result_join.Select(s => s.department_str).ToList();
            List<string> labels = temp_labels.Distinct().OrderBy(o => o).ToList();
            List<string> status_list = new List<string>(){"0", "1", "2","3"};
            List<string> str_list = new List<string>() { "No Answer", "Answered", "Approved", "Approved Late" };
            List<string> color_list = new List<string>() { "rgb(255,217,102)", "rgb(111,168,220)", "rgb(147,196,125)", "rgb(252,84,58)" };
            
            //Create Chart Model
            DataSetChartModel chart_model = new DataSetChartModel();
            chart_model.labels = labels;
            List<DataSetModel> dataset_list = new List<DataSetModel>();

            //For in Status
            foreach (var item in status_list.Select((value, i) => new {i,value }))
            {
                List<int> values = new List<int>();
                //For in labels
                foreach (var l in labels)
                {
                    int result = result_group.Where(x => x.ev_status.Trim() == item.value && x.deparment_str == l)
                        .Select(s => s.value_count).FirstOrDefault();
                    values.Add(result);
                }
               
                DataSetModel dataset = new DataSetModel();
                dataset.label = str_list[item.i];
                dataset.data = values;
                dataset.fill = false;
                dataset.backgroundColor = color_list[item.i];
                dataset.borderColor = "rgb(248, 249, 249)";
                dataset.borderWidth = 1;
                dataset_list.Add(dataset);
            }
            chart_model.datasets = dataset_list;

            //For Mission Table
            List<MissionTableModel> missionTable_list = new List<MissionTableModel>();
            foreach (var item in labels.Select((value, i) => new { i, value }))
            {
                MissionTableModel missionModel = new MissionTableModel();
                var result = result_group.Where(x => x.deparment_str == item.value).ToList();
                missionModel.department = item.value;
                missionModel.department_id = result.First().department_id;
                missionModel.no_answer_count = result.Where(x => x.ev_status.Trim() == "0").Select(s => s.value_count).FirstOrDefault();
                missionModel.answered_count = result.Where(x => x.ev_status.Trim()  == "1").Select(s => s.value_count).FirstOrDefault();
                missionModel.approved_count = result.Where(x => x.ev_status.Trim()  == "2").Select(s => s.value_count).FirstOrDefault();
                missionModel.approved_late_count = result.Where(x => x.ev_status.Trim()  == "3").Select(s => s.value_count).FirstOrDefault();
                missionModel.total_count = result.Select(s => s.value_count).Sum();
                missionTable_list.Add(missionModel);     
            }
            //Add Table
            model.MissionTable = missionTable_list;
            model.mission_total = result_group.Select(s => s.value_count).Sum();
            model.mission_total_noanswer = result_group.Where(x => x.ev_status.Trim() == "0").Select(s => s.value_count).Sum();
            model.mission_total_answered = result_group.Where(x => x.ev_status.Trim() == "1").Select(s=>s.value_count).Sum();
            model.mission_total_approved = result_group.Where(x => x.ev_status.Trim() == "2").Select(s => s.value_count).Sum();
            model.mission_total_approvedlate = result_group.Where(x => x.ev_status.Trim() == "3").Select(s => s.value_count).Sum();
            //Add Chart
            model.DataSetChart_1 = chart_model;
            return model;
        }

        //
        //GET: QRAP_Graph/MissionTableDetail/
        [CustomAuthorize(9)]//Problem Graph
        public ActionResult MissionTableDetail(int? department_id, int? status, DateTime? start_date, DateTime? end_date)
        {
            try
            {
                MissionTableDetailModel model = new MissionTableDetailModel();

                //Problem data
                IEnumerable<CM_Problem> pr_query = db.CM_Problem;
                if (start_date.HasValue)
                {
                    pr_query = pr_query.Where(x => x.PRB_PlanDate >= start_date.Value);
                }
                if(end_date.HasValue)
                {
                    pr_query = pr_query.Where(x => x.PRB_PlanDate <= end_date.Value);
                }
                //Solution data
                IEnumerable<CM_TodoList> so_query = db.CM_TodoList;
                if (department_id.HasValue)
                {
                    so_query = so_query.Where(x => x.Dep_Id == department_id);
                }
                //Evidence data
                IEnumerable<CM_TodoEvidence> ev_query = db.CM_TodoEvidence;
                if (status.HasValue && status != 3)
                {
                    string status_str = status.ToString();
                    ev_query = ev_query.Where(x => x.TDE_Status.Trim() == status_str);
                }
               
                var result_join = (from pr in pr_query
                                   join so in so_query on pr.PRB_Id equals so.PRB_Id
                                   join ev in ev_query on so.TDO_Id equals ev.TDO_Id
                                   join pr_dept in db.Departments on pr.Dep_Id equals pr_dept.Dep_Id
                                   join so_dept in db.Departments on so.Dep_Id equals so_dept.Dep_Id
                                   select new QRAPDataJoinModel
                                   {
                                       pr_id = pr.PRB_Id,
                                       so_id = so.PRB_Id,
                                       ev_id = ev.TDE_Id,
                                       ev_status = ev.TDE_Status,
                                       department_id = so_dept.Dep_Id,
                                       department_str = so_dept.Dep_Name,
                                       pr_plan_date = pr.PRB_PlanDate,
                                       so_plan_date = so.TDO_PlanDate,
                                       ev_answer_date = ev.TDE_AnswerDate,
                                       ev_approved_date = ev.TDE_ApprovedDate,
                                       pr_problem_no = pr.PRB_Number,
                                       pr_problem_name = pr.PRB_Description,
                                       pr_department = pr_dept.Dep_Name,
                                       so_response = so.TDO_Owner,
                                       so_detail = so.TDO_Description,
                                       ev_answer = ev.TDE_Answer,
                                       ev_approver = ev.TDE_Approver,
                                       ev_evidence_file = ev.TDE_Evidence,
                                       ev_rev = ev.TDE_Revision,
                                       ev_comment = ev.TDE_Comment,
                                   }).OrderBy(o=>o.pr_id).ToList();
                //Case status approved late
                if (status.HasValue && status == 3)
                {
                    result_join = result_join.Where(x => x.ev_approved_date < x.so_plan_date).ToList();
                }
                else if (status.HasValue && status == 2)
                {
                    result_join = result_join.Where(x => x.ev_approved_date >= x.so_plan_date).ToList();
                }

                //Select Status
                List<SelectListItem> selectStatus = new List<SelectListItem>();
                selectStatus.Add(new SelectListItem { Value = "0", Text = "No Answer" });
                selectStatus.Add(new SelectListItem { Value = "1", Text = "Answered" });
                selectStatus.Add(new SelectListItem { Value = "2", Text = "Approved" });
                selectStatus.Add(new SelectListItem { Value = "3", Text = "Approved Late" });

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectStatus = selectStatus;
                model.SelectDepartment = selectDept;

                model.DataJoinList = result_join;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Get //QRAP_Graph/MissionTableDetail {0}", ex.ToString());
                return View("Error");
            }
        }
        //
        //GET: QRAP_Graph/MissionTableDetail/
        [HttpPost]
        [CustomAuthorize(9)]//Problem Graph
        public ActionResult MissionTableDetail(MissionTableDetailModel model)
        {
            try
            {
                DateTime? start_date = model.start_date;
                DateTime? end_date = model.end_date;
                int? department_id = model.department_id;
                int? status = model.status;

                //Problem data
                IEnumerable<CM_Problem> pr_query = db.CM_Problem;
                if (start_date.HasValue)
                {
                    pr_query = pr_query.Where(x => x.PRB_PlanDate >= start_date.Value);
                }
                if (end_date.HasValue)
                {
                    pr_query = pr_query.Where(x => x.PRB_PlanDate <= end_date.Value);
                }
                //Solution data
                IEnumerable<CM_TodoList> so_query = db.CM_TodoList;
                if (department_id.HasValue)
                {
                    so_query = so_query.Where(x => x.Dep_Id == department_id);
                }
                //Evidence data
                IEnumerable<CM_TodoEvidence> ev_query = db.CM_TodoEvidence;
                if (status.HasValue && status != 3)
                {
                    string status_str = status.ToString();
                    ev_query = ev_query.Where(x => x.TDE_Status.Trim() == status_str);
                }

                var result_join = (from pr in pr_query
                                   join so in so_query on pr.PRB_Id equals so.PRB_Id
                                   join ev in ev_query on so.TDO_Id equals ev.TDO_Id
                                   join pr_dept in db.Departments on pr.Dep_Id equals pr_dept.Dep_Id
                                   join so_dept in db.Departments on so.Dep_Id equals so_dept.Dep_Id
                                   select new QRAPDataJoinModel
                                   {
                                       pr_id = pr.PRB_Id,
                                       so_id = so.PRB_Id,
                                       ev_id = ev.TDE_Id,
                                       ev_status = ev.TDE_Status,
                                       department_id = so_dept.Dep_Id,
                                       department_str = so_dept.Dep_Name,
                                       pr_plan_date = pr.PRB_PlanDate,
                                       so_plan_date = so.TDO_PlanDate,
                                       ev_answer_date = ev.TDE_AnswerDate,
                                       ev_approved_date = ev.TDE_ApprovedDate,
                                       pr_problem_no = pr.PRB_Number,
                                       pr_problem_name = pr.PRB_Description,
                                       pr_department = pr_dept.Dep_Name,
                                       so_response = so.TDO_Owner,
                                       so_detail = so.TDO_Description,
                                       ev_answer = ev.TDE_Answer,
                                       ev_approver = ev.TDE_Approver,
                                       ev_evidence_file = ev.TDE_Evidence,
                                       ev_rev = ev.TDE_Revision,
                                       ev_comment = ev.TDE_Comment,
                                   }).OrderBy(o => o.pr_id).ToList();
                //Case status approved late
                if (status.HasValue && status == 3)
                {
                    result_join = result_join.Where(x => x.ev_approved_date < x.so_plan_date).ToList();
                }
                else if (status.HasValue && status == 3)
                {
                    result_join = result_join.Where(x => x.ev_approved_date >= x.so_plan_date).ToList();
                }

                //Select Status
                List<SelectListItem> selectStatus = new List<SelectListItem>();
                selectStatus.Add(new SelectListItem { Value = "0", Text = "No Answer" });
                selectStatus.Add(new SelectListItem { Value = "1", Text = "Answered" });
                selectStatus.Add(new SelectListItem { Value = "2", Text = "Approved" });
                selectStatus.Add(new SelectListItem { Value = "3", Text = "Approved Late" });

                //Select Department
                List<SelectListItem> selectDept = db.Departments
                    .Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();

                model.SelectStatus = selectStatus;
                model.SelectDepartment = selectDept;

                model.DataJoinList = result_join;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = String.Format("Error: Post //QRAP_Graph/MissionTableDetail {0}", ex.ToString());
                return View("Error");
            }
        }


        //DownLoad file
        [CustomAuthorize(9)]//Problem Graph
        public ActionResult DownloadFileEvidence(string fileName)
        {
            string path = path_qrap_answers + fileName;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    //Read the File data into Byte Array.
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    //Send the File to Download.
                    return File(bytes, "application/octet-stream", fileName);
                }
                ViewBag.errorMessage = String.Format("Could not find file.");
                return View("Error");
            }
            catch (IOException)
            {
                ViewBag.errorMessage = String.Format("Could not find file {0}", path);
                // ViewBag.errorMessage = io.ToString();
                return View("Error");
            }
        }


        //For test
        [HttpPost]
        public JsonResult NewChart()
        {
            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee", System.Type.GetType("System.String"));
            dt.Columns.Add("Credit", System.Type.GetType("System.Int32"));

            DataRow dr = dt.NewRow();
            dr["Employee"] = "Sam";
            dr["Credit"] = 123;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Employee"] = "Alex";
            dr["Credit"] = 456;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Employee"] = "Michael";
            dr["Credit"] = 587;
            dt.Rows.Add(dr);
            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

	}


}