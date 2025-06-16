using OCTWEB_NET45.Context;
using OCTWEB_NET45.Infrastructure;
using OCTWEB_NET45.Models;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.TrainingCourse
{
    [Authorize]
    public class TrainingCourseManagementController : Controller
    {

        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        //
        // GET: /TrainingCourseManagement/SubTrainingCourseManagementList
        [CustomAuthorize(33)]//Training courses management
        public ActionResult SubTrainingCourseManagementList()
        {
            var countrequest = db.Emp_TrainingWS_temporary.Where(x => x.Train_Status == 0).Count();
            TrainingCourseManagementModel model = new TrainingCourseManagementModel();
            model.CountRequest = countrequest;

            return View(model);
        }

        //
        // GET: /TrainingCourseManagement/CourseSetup
        [CustomAuthorize(61,33)]//Training courses setup, , //Training courses management
        public ActionResult CourseSetup()
        {
            return View();
        }


        // GET: /TrainingCourseManagement/CourseReport
        [CustomAuthorize(56)]//Training courses report
        public ActionResult CourseReport()
        {
            return View();
        }

       




















       
        //
        // GET: /TrainingCourseManagement/TestSendMailDataTable
        public ActionResult TestSendMailDataTable()
        {

            WSTrainingSendMailModel model = new WSTrainingSendMailModel();

            List<SelectListItem> dept_list = db.Departments.OrderBy(o => o.Dep_Name).Select(s => new SelectListItem()
            {
                Value = s.Dep_Id.ToString(),
                Text = s.Dep_Name
            }).ToList();

            model.SelectDepartmentId = dept_list;

            return View(model);

        }

        //
        // GET: /TrainingCourseManagement/GetDataTable
        public ActionResult GetDataTable()
        {

            return View();
        }
        
       

        //
        // GET: /TrainingCourseManagement/TrainingList
        public ActionResult TrainingList()
        {
            try
            {
                /*
                //Creating instance of DatabaseContext class  
                using (DatabaseContext _context = new DatabaseContext())
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                    //Paging Size (10,20,50,100)    
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int recordsTotal = 0;

                    // Getting all Customer data    
                    var customerData = (from tempcustomer in _context.Customers
                                        select tempcustomer);

                    //Sorting    
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        customerData = customerData.OrderBy(sortColumn + " " + sortColumnDir);
                    }
                    //Search    
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        customerData = customerData.Where(m => m.CompanyName == searchValue);
                    }

                    //total number of rows count     
                    recordsTotal = customerData.Count();
                    //Paging     
                    var data = customerData.Skip(skip).Take(pageSize).ToList();
                    //Returning Json Data    
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                
                 }
                 */
                return View();
            }
            catch (Exception)
            {
                throw;
            }  
        }

        // /TrainingCourseManagement/GetDataInitialUser
        public JsonResult GetDataInitialUser()
        {
            var userlist = db.UserDetails.Select(x => new
            {
                Id = x.USE_Id,
                Name = x.USE_FName,
                SurName = x.USE_LName,
                Email = x.USE_Email
            }).ToList();
            return Json(userlist, JsonRequestBehavior.AllowGet);
        }

        // /TrainingCourseManagement/GetDataUser
        public JsonResult GetDataUser(int department)
        {
            var userlist = db.UserDetails.Where(x=>x.Dep_Id == department).Select(x=> new
            {
                Id = x.USE_Id,
                Name = x.USE_FName,
                SurName = x.USE_LName,
                Email = x.USE_Email
            }).ToList();
            return Json(userlist,JsonRequestBehavior.AllowGet);
        }

      
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

      
    }
}
