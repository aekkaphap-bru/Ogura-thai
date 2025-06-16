using OCTWEB_NET45.Context;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class TrainingCourseManagementModel
    {
        public string AddWsTrainingCourse { get; set; }
        public string ManageWsTrainingCourse { get; set; }
        public string AddTrainingCourse { get; set; }
        public string ManageTrainingCouse { get; set; }

        public int CountRequest { get; set; }
    }

    public class WSTrainingModel
    {
        public int Id { get; set; }
        public string Train_NumberWS { get; set; }
        public string Train_Header { get; set; }
        public string Train_HeaderThai { get; set; }
        public string Train_DateWS { get; set; }
        public int? Train_Status { get; set; }
        public string Trai_NameArea { get; set; }
        public string Trai_Rev { get; set; }
        public string Train_DateChkSendMail { get; set; }
        
    }

    public class WSTrainingListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public int? selected_status { get; set; }
        public IEnumerable<SelectListItem> SelectStatusId { get; set; }
        
        public IPagedList<WSTrainingModel> WSTrainingModelPagedList { get; set; }

    }

    public class WSTrainingSendMailModel
    {
        public IEnumerable<SelectListItem> SelectDepartmentId { get; set; }
        public int searchDepartmentId { get; set; }

        public WSTrainingModel WsTrain { get; set; }
        public IEnumerable<UserDetailModel> UserList { get; set; }

    }

    public class Emp_TrainingWsHeaderModel
    {
        public int Id { get; set; }
        public string CourseHeader { get; set; }
        public Nullable<int> Trai_Temporary_Id { get; set; }
        public string Train_Location { get; set; }
        public Nullable<int> Train_Price { get; set; }
        public string Train_Name { get; set; }
        public string Train_Date { get; set; }
        public string Train_Hour { get; set; }
        public string Train_Min { get; set; }
        public string Train_DateWS { get; set; }
        public string Train_EmpId { get; set; }
        public string area { get; set; }
        public int status { get; set; }
        public string downloadfile { get; set; }
        public int department_id { get; set; }
        public string Train_WS_Temp { get; set; }

    }


    public class ManageWSListModel
    {
        private bool _default = false;
        public bool showall
        {
            get
            {
                return _default;
            }
            set
            {
                _default = value;
            }
        }
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public int? selected_status { get; set; }
        public IEnumerable<SelectListItem> SelectStatusId { get; set; }

        public string search_wsnumber { get; set; }
        public string search_area { get; set; }

        public IPagedList<Emp_TrainingWsHeaderModel> Emp_TrainingWsHeaderModelPagedList { get; set; }


        //New
        public bool IsAdminAccess { get; set; }

        public Emp_TrainingWsHeaderModel WSTrainHeaderModel { get; set; }
        public IEnumerable<Emp_TrainingWsDate> TrainingDate { get; set; }
        public IEnumerable<TraineeModel> TraineeLists { get; set; }

    }

    public class ManageWSTrainCourseModel
    {
        public IEnumerable<TraineeModel> TraineeList { get; set; }
        public Emp_TrainingWsHeaderModel WSTrainHeaderModel { get; set; }
        public string DateTrainStart { get; set; }
        public string DateTrainEnd { get; set; }
        public string TimeTrainStart { get; set; }
        public string TimeTrainEnd { get; set; }
        public string TrainDays { get; set; }
        public string TrainHours { get; set; }
        public string TrainMinutes { get; set; }
        public string CalAssessment { get; set; }


    }

    public class TraineeModel
    {
        public int emp_id { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string nation { get; set; }
        public string dept { get; set; }
        public string position { get; set; }
        public int assessment { get; set; }
        public int assessment_detail { get; set; }

        public int e_trainWsdate_id { get; set; }
        public int detail_id { get; set; }

    }

    public class WstrainDateModel
    {
        public int Id { get; set; }
        public int E_TrainWsHeader_Id { get; set; }
        public string Training_dateSt { get; set; }
        public string Training_dateEnd { get; set; }
        public string Trai_Times { get; set; }
        public string Trai_Timess { get; set; }
        public string Trai_TimendT { get; set; }
        public string Trai_TimendTs { get; set; }
        public string Trai_House { get; set; }
        public string Train_WsName { get; set; }
    }


    public class AddTraineeModel
    {
        private DateTime _SetDefaultStartDate = DateTime.Today;
        private DateTime _SetDefaultEndDate = DateTime.Today;
        private DateTime _SetDefaultStartTime = DateTime.Today.Add(new TimeSpan(10,10,0));
        private DateTime _SetDefaultEndTime = DateTime.Today.Add(new TimeSpan(12,20,0));
       
        public DateTime DateOfTrainingStart 
        {
            get
            {
                return _SetDefaultStartDate;
            }
            set
            {
                _SetDefaultStartDate = value;
            }
        }

        [DataType( DataType.Time)]
        public DateTime StartTime 
        {
            get
            {
                return _SetDefaultStartTime;
            }
            set
            {
                _SetDefaultStartTime = value;
            }
        }
        public DateTime DateOfTrainingEnd 
        {
            get
            {
                return _SetDefaultEndDate;
            }
            set
            {
                _SetDefaultEndDate = value;
            }
        }

        [DataType(DataType.Time)]
        public DateTime EndTime 
        {
            get
            {
                return _SetDefaultEndTime;
            }
            set
            {
                _SetDefaultEndTime = value;
            }
        }

        public int BreakTime { get; set; }
        public string ImportFilePdf { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public List<int> selectedEmp { get; set; }
        public Emp_TrainingWsHeaderModel TWSHeaderModel { get; set; }

        public WstrainDateModel WstrainDateModel { get; set; }
    }



    /**********Training Course  **************************************************************/

    public class ManageCourseModel
    {
        public IEnumerable<TraineeModel> TraineeList { get; set; }
        public CourseModel CourseModel { get; set; }
    }

    public class CourseListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        private DateTime _SetDefaultStartDate = new DateTime(2020, 1, 1);
        private DateTime _SetDefaultEndDate = DateTime.Today;

        public DateTime? search_startdate { get; set; }

        public DateTime? search_enddate { get; set; }
       
        public string search_topic { get; set; }
        public string search_location { get; set; }
       
        public IEnumerable<SelectListItem> SelectStatusId { get; set; }

        public IPagedList<CourseModel> CourseModelPagedList { get; set; }

    }
    public class CourseModel
    {
        private DateTime _SetDefaultStartDate = DateTime.Today;
        private DateTime _SetDefaultEndDate = DateTime.Today;
        private DateTime _SetDefaultStartTime = DateTime.Today.Add(new TimeSpan(10, 10, 0));
        private DateTime _SetDefaultEndTime = DateTime.Today.Add(new TimeSpan(12, 20, 0));

        public DateTime DateOfTrainingStart
        {
            get
            {
                return _SetDefaultStartDate;
            }
            set
            {
                _SetDefaultStartDate = value;
            }
        }

        [DataType(DataType.Time)]
        public DateTime StartTime
        {
            get
            {
                return _SetDefaultStartTime;
            }
            set
            {
                _SetDefaultStartTime = value;
            }
        }
        public DateTime DateOfTrainingEnd
        {
            get
            {
                return _SetDefaultEndDate;
            }
            set
            {
                _SetDefaultEndDate = value;
            }
        }

        [DataType(DataType.Time)]
        public DateTime EndTime
        {
            get
            {
                return _SetDefaultEndTime;
            }
            set
            {
                _SetDefaultEndTime = value;
            }
        }

        public int BreakTime { get; set; }
        public string ImportFilePdf { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Locatin Name")]
        public string Location { get; set; }

        [MaxLength(500)]
        [Display(Name = "Training Name")]
        public string CourseHeader { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Trainer Name")]
        public string TrainerName { get; set; }

        [Display(Name = "Course Price")]
        public int CoursePrice { get; set; }

        public List<SelectListItem> EmployeeList { get; set; }
        public List<int> selectedEmp { get; set; }
        public Emp_TrainingHeaderModel THeaderModel { get; set; }
        public double? cognitive { get; set; }
        public string difftime_str { get; set; }
        public string time_str { get; set; }
        public int course_header_id { get; set; }

        public int? TotalPrice { get; set; }
    }

    public class Emp_TrainingHeaderModel
    {
        public int Id { get; set; }
        public string CourseHeader { get; set; }
        public System.DateTime Training_dateSt { get; set; }
        public string Trai_Times { get; set; }
        public string Trai_Timess { get; set; }
        public System.DateTime Training_dateEnd { get; set; }
        public string Trai_TimendT { get; set; }
        public string Trai_TimendTs { get; set; }
        public string Trai_House { get; set; }
        public string Trai_Minutices { get; set; }
        public string Train_Location { get; set; }
        public int? Train_Price { get; set; }
        public string Train_Name { get; set; }
        public string Train_Date { get; set; }
        public string Train_Hour { get; set; }
        public string Train_Min { get; set; }
        public string Train_WsFileName { get; set; }

      
    }
}