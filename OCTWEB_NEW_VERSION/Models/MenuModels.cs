using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class MenuModels
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }

        //Administration
        //Setup
        public bool admin_setup { get; set; } //
        public bool UserDetail_4 { get; set; }
        public bool ChangePassword_6 { get; set; }
        public bool UserGroup_8 { get; set; }
        public bool LogReport_10 { get; set; }

        //Check Appearance 300%
        public bool check300 { get; set; }///
        //Information
        public bool check300_inform { get; set; }//
        public bool InformProblem_26 { get; set; }
        //Report
        public bool check300_report { get; set; }//
        public bool ProblemReportGraph_28 { get; set; }
        public bool ProblemReportData_29 { get; set; }
        //Setup
        public bool check300_setup { get; set; }//
        public bool LineName_30 { get; set; }
        public bool PartName_31 { get; set; }
        public bool ProblemList_27 { get; set; }

        //Drawing
        public bool drawing { get; set; }///
        //Report
        public bool drawing_report { get; set; }//
        public bool FileShare_24 { get; set; }
        public bool DrawingSearch_12 { get; set; }
        //Setup
        public bool drawing_setup { get; set; }//
        public bool Process_13 { get; set; }
        public bool PartName_14 { get; set; }
        public bool EngineerDrawing_15 { get; set; }
        public bool ProcessDrawing_16 { get; set; }
        public bool UploadFileShare_25 { get; set; }

        //HR
        public bool hr { get; set; }///
        //Management Setup
        public bool hr_manage { get; set; }//
        public bool TrainingCoursesSetup_61 { get; set; }
        public bool TrainingCoursesWSSetup_57 { get; set; }
        public bool FormerEmpListSetup_37 { get; set; }
        public bool EmployeeListSetup_32 { get; set; }
        public bool TrainingCoursesManagement_33 { get; set; }
        public bool MistakesSetup_34 { get; set; }
        public bool LeaveSetup_69 { get; set; }
        public bool BenefitSetup_71 { get; set; }
        //Report
        public bool hr_report { get; set; }//
        public bool EmployeeListReport_53 { get; set; }
        public bool FormerEmpListReport_54 { get; set; }
        public bool MistakesReport_55 { get; set; }
        public bool TrainingCourses_56 { get; set; }
        public bool LeaveReport_64 { get; set; }
        public bool LeaveSetup_63 { get; set; }

        //IT System
        //IT Report
        public bool it_report { get; set; }//
        public bool ComputerGraph_11 { get; set; }

        //QRAP
        public bool qrap { get; set; }///
        //Report
        public bool qrap_report { get; set; }//
        public bool ProblemGraph_9 { get; set; }
        public bool QRAPDetail_3 { get; set; }
        //Update and Modify
        public bool qrap_update { get; set; }//
        public bool Approve_7 { get; set; }
        public bool InformProblem_5 { get; set; }
        public bool Answers_1 { get; set; }
        public bool DefineSolution_2 { get; set; }

        //WS & Rule
        public bool ws { get; set; }///
        //Report
        public bool ws_report { get; set; }//
        public bool WorkingStandardSearch_22 { get; set; }
        public bool CompanyRuleSearch_19 { get; set; }
        //Setup Rule
        public bool ws_setuprule { get; set; }//
        public bool CompanyRuleType_17 { get; set; }
        public bool CompanyRule_18 { get; set; }
        //Setup WS
        public bool ws_setupws { get; set; }//
        public bool WorkingStandardType_20 { get; set; }
        public bool WorkingStandard_21 { get; set; }
        public bool WorkingStandardProcess_23 { get; set; }

        public bool dc {  get; set; }
        public bool DocumentSetup_73 { get; set; }

        //System Support
        public bool system_support { get; set; }
        public bool APBillingNote_65 { get; set; }
        public bool ALBillingNote_66 { get; set; }
    }
}