using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers
{
    [Authorize]
    public class PermissionsController : Controller
    {

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();
        private LogsReportController savelogs = new LogsReportController();

        //
        //GET: /Permission/SetPermission
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult SetPermission(int id)
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }

            UserDetail ud = db.UserDetails.Where(x => x.USE_Id == id).FirstOrDefault();
            string username = null;
            string usercode = null;
            if (ud != null)
            {
                username = ud.USE_Account;
                usercode = ud.USE_Usercode.ToString();
            }
            List<int> userright_list = db.UserRights.Where(x => x.USE_Id == id).Select(s => s.RIH_Id).ToList();

            PermissionSetModel model = new PermissionSetModel()
            {
                UserName = username,
                UserCode = usercode,
                UserId = id,
                //Administration
                //Setup
                UserDetail_4 = userright_list.Contains(4) ? true : false,
                ChangePassword_6 = userright_list.Contains(6) ? true : false,
                UserGroup_8 = userright_list.Contains(8) ? true : false,
                LogReport_10 = userright_list.Contains(10) ? true : false,

                //Check Appearance 300%
                //Information
                InformProblem_26 = userright_list.Contains(26) ? true : false,
                //Report
                ProblemReportGraph_28 = userright_list.Contains(28) ? true : false,
                ProblemReportData_29 = userright_list.Contains(29) ? true : false,
                //Setup
                LineName_30 = userright_list.Contains(30) ? true : false,
                PartName_31 = userright_list.Contains(31) ? true : false,
                ProblemList_27 = userright_list.Contains(27) ? true : false,

                //Drawing
                //Report
                FileShare_24 = userright_list.Contains(24) ? true : false,
                DrawingSearch_12 = userright_list.Contains(12) ? true : false,
                //Setup
                Process_13 = userright_list.Contains(13) ? true : false,
                PartName_14 = userright_list.Contains(14) ? true : false,
                EngineerDrawing_15 = userright_list.Contains(15) ? true : false,
                ProcessDrawing_16 = userright_list.Contains(16) ? true : false,
                UploadFileShare_25 = userright_list.Contains(25) ? true : false,

                //HR
                //HR permission
                AddHolidayleaveReport_62 = userright_list.Contains(62) ? true : false,
                ToseeemployeesIDcodePassportIDTaxIDSSO_38 = userright_list.Contains(38) ? true : false,
                Toseeemployeesbirthdateinfo_44 = userright_list.Contains(44) ? true : false,
                TosearchwithIDcode_45 = userright_list.Contains(45) ? true : false,
                Toexportdata_49 = userright_list.Contains(49) ? true : false,
                Tounlockpageandedittheinfo_50 = userright_list.Contains(50) ? true : false,
                Tolayoffemployees_52 = userright_list.Contains(52) ? true : false,
                CanseeTrainingEmployee_58 = userright_list.Contains(58) ? true : false,
                CanseeWorkLeaveAll_60 = userright_list.Contains(60) ? true : false,
                
                //Management Setup
                TrainingCoursesSetup_61 = userright_list.Contains(61) ? true : false,
                TrainingCoursesWSSetup_57 = userright_list.Contains(57) ? true : false,
                FormerEmpListSetup_37 = userright_list.Contains(37) ? true : false,
                EmployeeListSetup_32 = userright_list.Contains(32) ? true : false,
                TrainingCoursesManagement_33 = userright_list.Contains(33) ? true : false,
                MistakesSetup_34 = userright_list.Contains(34) ? true : false,
                LeaveSetup_63 = userright_list.Contains(63) ? true : false,
                LeaveSetup_69 = userright_list.Contains(69) ? true : false,
                BenefitSetup_71 = userright_list.Contains(71) ? true : false,
                //Report
                EmployeeListReport_53 = userright_list.Contains(53) ? true : false,
                FormerEmpListReport_54 = userright_list.Contains(54) ? true : false,
                MistakesReport_55 = userright_list.Contains(55) ? true : false,
                TrainingCourses_56 = userright_list.Contains(56) ? true : false,
                LeaveReport_64 = userright_list.Contains(64) ? true :false,
                LeaveCreate_70 = userright_list.Contains(70) ? true : false,

                //IT System
                //IT Report
                ComputerGraph_11 = userright_list.Contains(11) ? true : false,

                //QRAP
                //Report
                ProblemGraph_9 = userright_list.Contains(9) ? true : false,
                QRAPDetail_3 = userright_list.Contains(3) ? true : false,
                //Update and Modify
                Approve_7 = userright_list.Contains(7) ? true : false,
                InformProblem_5 = userright_list.Contains(5) ? true : false,
                Answers_1 = userright_list.Contains(1) ? true : false,
                DefineSolution_2 = userright_list.Contains(2) ? true : false,

                //WS & Rule
                //Permission Document
                PermissionDocumentBOIandPurchase_35 = userright_list.Contains(35) ? true : false,
                PermissionDocumentAccountingandFinance_36 = userright_list.Contains(36) ? true : false,
                //Report
                WorkingStandardSearch_22 = userright_list.Contains(22) ? true : false,
                CompanyRuleSearch_19 = userright_list.Contains(19) ? true : false,
                //Setup Rule
                CompanyRuleType_17 = userright_list.Contains(17) ? true : false,
                CompanyRule_18 = userright_list.Contains(18) ? true : false,
                //Setup WS
                WorkingStandardType_20 = userright_list.Contains(20) ? true : false,
                WorkingStandard_21 = userright_list.Contains(21) ? true : false,
                WorkingStandardProcess_23 = userright_list.Contains(23) ? true : false,

                //System Support
                //AP Billing Note
                APBillingNote_65 = userright_list.Contains(65) ? true : false,
                //AL Billing Note
                ALBillingNote_66 = userright_list.Contains(66) ? true : false,
                //Dashboard
                SaleDashboard_67 = userright_list.Contains(67) ? true : false,
                PurchaseDashboard_68 = userright_list.Contains(68) ? true : false,

                //DocumentControll
                DocumentSetup_73 = userright_list.Contains(73) ? true : false,
                Approve_Dept_74 = userright_list.Contains(74) ? true : false,
                Approve_Dcc_75 = userright_list.Contains(75) ? true : false,
                Approve_Qmr_76 = userright_list.Contains(76) ? true : false,
                Approve_Emr_79 = userright_list.Contains(79) ? true : false,
                Relevant_Document_Revie_77 = userright_list.Contains(77) ? true : false,
                Can_See_All_Document_78 = userright_list.Contains(78) ? true : false
            };

            return View(model);

        }

        //
        //POST: /Permission/SetPermission
        [HttpPost]
        [CustomAuthorize(4)]//User Detail/Setup/Administration
        public ActionResult SetPermission(PermissionSetModel model)
        {
            try
            {
                List<UserRight> ur_old_list = db.UserRights.Where(x => x.USE_Id == model.UserId).ToList();
                List<UserRight> ur_list = new List<UserRight>();

                if (model.UserDetail_4)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 4 };
                    ur_list.Add(ur);
                }
                if (model.ChangePassword_6)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 6 };
                    ur_list.Add(ur);
                }
                if (model.UserGroup_8)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 8 };
                    ur_list.Add(ur);
                }
                if (model.LogReport_10)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 10 };
                    ur_list.Add(ur);
                }
                if (model.InformProblem_26)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 26 };
                    ur_list.Add(ur);
                }
                if (model.ProblemReportGraph_28)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 28 };
                    ur_list.Add(ur);
                }
                if (model.ProblemReportData_29)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 29 };
                    ur_list.Add(ur);
                }
                if (model.LineName_30){ UserRight ur = new UserRight(){ USE_Id = model.UserId,RIH_Id = 30}; 
                    ur_list.Add(ur);}
                if (model.PartName_31){UserRight ur = new UserRight(){USE_Id = model.UserId,RIH_Id = 31};
                    ur_list.Add(ur);}
                if (model.ProblemList_27){UserRight ur = new UserRight(){USE_Id = model.UserId,RIH_Id = 27};
                    ur_list.Add(ur);}
 
                if (model.FileShare_24){UserRight ur = new UserRight(){USE_Id = model.UserId,RIH_Id = 24};
                    ur_list.Add(ur);}
                if (model.DrawingSearch_12)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 12 };
                    ur_list.Add(ur);
                }
                if (model.Process_13)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 13 };
                    ur_list.Add(ur);
                }
                if (model.PartName_14)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 14 };
                    ur_list.Add(ur);
                }
                if (model.EngineerDrawing_15)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 15 };
                    ur_list.Add(ur);
                }
                if (model.ProcessDrawing_16)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 16 };
                    ur_list.Add(ur);
                }
                if (model.UploadFileShare_25)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 25 };
                    ur_list.Add(ur);
                }
                if (model.ToseeemployeesIDcodePassportIDTaxIDSSO_38)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 38 };
                    ur_list.Add(ur);
                }
                if (model.Toseeemployeesbirthdateinfo_44)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 44 };
                    ur_list.Add(ur);
                }
                if (model.TosearchwithIDcode_45)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 45 };
                    ur_list.Add(ur);
                }
                if (model.Toexportdata_49)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 49 };
                    ur_list.Add(ur);
                }
                if (model.Tounlockpageandedittheinfo_50)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 50 };
                    ur_list.Add(ur);
                }
                if (model.Tolayoffemployees_52)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 52 };
                    ur_list.Add(ur);
                }
                if (model.CanseeTrainingEmployee_58)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 58 };
                    ur_list.Add(ur);
                }
                if (model.CanseeWorkLeaveAll_60)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 60 };
                    ur_list.Add(ur);
                }
                if (model.TrainingCoursesSetup_61)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 61 };
                    ur_list.Add(ur);
                }
                if (model.TrainingCoursesWSSetup_57)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 57 };
                    ur_list.Add(ur);
                }
                if (model.FormerEmpListSetup_37)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 37 };
                    ur_list.Add(ur);
                }
                if (model.EmployeeListSetup_32)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 32 };
                    ur_list.Add(ur);
                }
                if (model.TrainingCoursesManagement_33)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 33 };
                    ur_list.Add(ur);
                }
                if (model.MistakesSetup_34)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 34 };
                    ur_list.Add(ur);
                }
                if (model.EmployeeListReport_53)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 53 };
                    ur_list.Add(ur);
                }
                if (model.FormerEmpListReport_54)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 54 };
                    ur_list.Add(ur);
                }
                if (model.MistakesReport_55)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 55 };
                    ur_list.Add(ur);
                }
                if (model.TrainingCourses_56)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 56 };
                    ur_list.Add(ur);
                }
                if (model.ComputerGraph_11)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 11 };
                    ur_list.Add(ur);
                }
                if (model.ProblemGraph_9)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 9 };
                    ur_list.Add(ur);
                }
                if (model.QRAPDetail_3)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 3 };
                    ur_list.Add(ur);
                }
                if (model.Approve_7)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 7 };
                    ur_list.Add(ur);
                }
                if (model.InformProblem_5)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 5 };
                    ur_list.Add(ur);
                }
                if (model.Answers_1)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 1 };
                    ur_list.Add(ur);
                }
                if (model.DefineSolution_2)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 2 };
                    ur_list.Add(ur);
                }
                if (model.PermissionDocumentBOIandPurchase_35)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 35 };
                    ur_list.Add(ur);
                }
                if (model.PermissionDocumentAccountingandFinance_36)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 36 };
                    ur_list.Add(ur);
                }
                if (model.WorkingStandardSearch_22)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 22 };
                    ur_list.Add(ur);
                }
                if (model.CompanyRuleSearch_19)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 19 };
                    ur_list.Add(ur);
                }
                if (model.CompanyRuleType_17)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 17 };
                    ur_list.Add(ur);
                }
                if (model.CompanyRule_18)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 18 };
                    ur_list.Add(ur);
                }
                if (model.WorkingStandardType_20)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 20 };
                    ur_list.Add(ur);
                }
                if (model.WorkingStandard_21)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 21 };
                    ur_list.Add(ur);
                }
                if (model.WorkingStandardProcess_23)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 23 };
                    ur_list.Add(ur);
                }
                if (model.AddHolidayleaveReport_62)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 62 };
                    ur_list.Add(ur);
                }
                if (model.LeaveSetup_63)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 63 };
                    ur_list.Add(ur);
                }
                if (model.LeaveReport_64)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 64 };
                    ur_list.Add(ur);
                }
                if (model.LeaveCreate_70)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 70 };
                    ur_list.Add(ur);
                }
                
                if (model.APBillingNote_65)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 65 };
                    ur_list.Add(ur);
                }
                if (model.ALBillingNote_66)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 66 };
                    ur_list.Add(ur);
                }
                if (model.SaleDashboard_67)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 67 };
                    ur_list.Add(ur);
                }
                if (model.PurchaseDashboard_68)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 68 };
                    ur_list.Add(ur);
                }
                if (model.LeaveSetup_69)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 69 };
                    ur_list.Add(ur);
                }
                if (model.BenefitSetup_71)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 71 };
                    ur_list.Add(ur);
                }
                if (model.DocumentSetup_73)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 73 };
                    ur_list.Add(ur);
                }
                if (model.Approve_Dept_74)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 74 };
                    ur_list.Add(ur);
                }
                if (model.Approve_Dcc_75)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 75 };
                    ur_list.Add(ur);
                }
                if (model.Approve_Qmr_76)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 76 };
                    ur_list.Add(ur);
                }
                if (model.Approve_Emr_79)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 79 };
                    ur_list.Add(ur);
                }
                if (model.Relevant_Document_Revie_77)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 77 };
                    ur_list.Add(ur);
                }
                if (model.Can_See_All_Document_78)
                {
                    UserRight ur = new UserRight() { USE_Id = model.UserId, RIH_Id = 78 };
                    ur_list.Add(ur);
                }
                //For Delete 
                var old_NotIn_new = ur_old_list != null ? ur_old_list.Select(s=>s.RIH_Id).Except(ur_list.Select(s=>s.RIH_Id)).ToList() : new List<int>();
                //For Add New
                var new_NotIn_old = ur_list != null ? ur_list.Select(s=>s.RIH_Id).Except(ur_old_list.Select(s=>s.RIH_Id)).ToList() : new List<int>();

                //Delete right
                if (old_NotIn_new.Any())
                {
                    var old_model = ur_old_list.Where(x => old_NotIn_new.Contains(x.RIH_Id)).ToList();
                    db.UserRights.RemoveRange(old_model);
                }
                //Add new right
                if(new_NotIn_old.Any())
                {
                    var new_model = ur_list.Where(x => new_NotIn_old.Contains(x.RIH_Id)).ToList();
                    db.UserRights.AddRange(new_model);
                }
                //Save Log
                string user_nickname = null;
                if (Session["NickName"] != null)
                {
                    user_nickname = Session["NickName"].ToString();
                }
                Log logmodel = new Log()
                {
                    Log_Action_Id = model.UserId,
                    Log_System = "Administration",
                    Log_Type = "User Detail",
                    Log_Detail = string.Concat("Set permission"),
                    Log_Action = "add",
                    Log_Date = DateTime.Now,
                    Log_by = user_nickname
                };
                db.Logs.Add(logmodel);
                //Save All
                db.SaveChanges();

                TempData["shortMessage"] = "Setup right successfully.";
                ViewBag.Message = "Setup right successfully.";
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                ViewBag.errorMessage = ex.InnerException.ToString();
                return View("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}