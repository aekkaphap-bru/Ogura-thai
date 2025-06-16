using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using OCTWEB_NET45.Models;
using OCTWEB_NET45.Context;
using OCTWEB_NET45.App_Start;
using OCTWEB_NET45.Infrastructure;

namespace OCTWEB_NET45.Controllers
{

    public class AccountController : Controller
    {
        private OCTWEBTESTEntities db = new OCTWEBTESTEntities();

        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (TempData["shortMessage"] != null)
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            /*
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            */
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            /* var user = await UserManager.FindByNameAsync(model.UserName);
             if (user != null)
             {
                 if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                 {
                     string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account-Resend");
                     //ViewBag.errorMessage = "You must have a confirmed email to log on."
                     //                      + "The confirmation token has been resent to your email account.";
                     //ModelState.AddModelError("", "You must have a confirmed email to log on.");

                     TempData["shortMessage"] = "You must have a confirmed email to log on."
                                              + "The confirmation token has been resent to your email account.";
                     //return View("Error"); 
                 }
             }*/

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var userlogin = await UserManager.FindAsync(model.UserName, model.Password);
            if (userlogin != null)
            {
                //Get the Menu details from entity and bind it in MenuModels list.  
                //FormsAuthentication.SetAuthCookie(_loginCredentials.UserName, false); // set the formauthentication cookie  
                //Session["LoginCredentials"] = _loginCredentials; // Bind the _logincredentials details to "LoginCredentials" session  

                await SignInAsync(userlogin, model.RememberMe);
                Session["UserID"] = userlogin.Id.ToString();
                Session["UserName"] = userlogin.UserName.ToString();
                Session["UserCode"] = userlogin.USE_Usercode.ToString();
                Session["FistNameLastName"] = "";
                Session["NickName"] = "";
                Session["USE_Id"] = "";
                Session["Dep_Id"] = "";
                var uud = db.UserDetails.Where(x => x.USE_Usercode == userlogin.USE_Usercode && x.USE_Status == 1).FirstOrDefault();
                if (uud == null)
                {
                    AuthenticationManager.SignOut();
                    Session.Abandon();
                    Session.Clear();
                    Session.RemoveAll();
                    ViewBag.Message = "This account has been disabled, Please contact to the OCTWEB admin.";
                    return View("Info");
                }
                else
                {
                    Session["FistNameLastName"] = String.Concat(uud.USE_Usercode.ToString(), " ", uud.USE_FName.ToString(), " ", uud.USE_LName.ToString());
                    Session["NickName"] = uud.USE_NickName.ToString();
                    Session["USE_Id"] = uud.USE_Id.ToString();
                    Session["Dep_Id"] = uud.Dep_Id.ToString();
                    //Get Menu 
                    if (uud.USE_Id > 0)
                    {
                        var menu = getMenu(uud.USE_Id);
                        Session["MenuModels"] = menu;
                    }
                }

                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/createUser
        [AllowAnonymous]
        public ActionResult createUser(int id)
        {
            try
            {
                var user = db.UserDetails.Find(id);
                if (user != null)
                {
                    RegisterViewModel model = new RegisterViewModel();
                    model.USE_Id = user.USE_Id;
                    model.UserName = user.USE_Account;
                    model.USE_Usercode = Convert.ToInt32(user.USE_Usercode);
                    model.Email = user.USE_Email;

                    model.USE_FName = user.USE_FName;
                    model.USE_LName = user.USE_LName;
                    model.USE_NickName = user.USE_NickName;
                    model.USE_Nationality = user.USE_Nationality;
                    model.USE_Status = user.USE_Status;
                    model.USE_TelNo = user.USE_TelNo;
                    model.Dep_Id = user.Dep_Id;

                    GetSelectList(model);

                    return View(model);
                }
                return View("Error");
            }
            catch (SystemException ioe)
            {
                ModelState.AddModelError("", ioe.Message);
                ViewBag.errorMessage = ioe.Message.ToString();
                return View("Error");
            }

        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> createUser(RegisterViewModel model)
        {
            GetSelectList(model);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, USE_Usercode = model.USE_Usercode };
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {

                        /*string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "OCTWEB - Confirm your account");
                        string VMessage = "Check your email and confirm your account, you must be confirmed "
                                    + "before you can log in."; */

                        UserDetail ud = db.UserDetails.Where(u => u.USE_Id == model.USE_Id).FirstOrDefault();
                        ud.Dep_Id = model.Dep_Id;
                        ud.USE_Account = model.UserName;
                        ud.USE_Email = model.Email;
                        ud.USE_FName = model.USE_FName;
                        ud.USE_LName = model.USE_LName;
                        ud.USE_Nationality = model.USE_Nationality;
                        ud.USE_NickName = model.USE_NickName;
                        ud.USE_Status = model.USE_Status;
                        ud.USE_TelNo = model.USE_TelNo;

                        //Edit UserDetail
                        db.Entry(ud).State = System.Data.Entity.EntityState.Modified;

                        db.SaveChanges();

                        SendEmailCreateUser(model);

                        string VMessage = String.Format("Created a new account successfully, Employee ID {0}", model.USE_Usercode);

                        TempData["shortMessage"] = VMessage;
                        return RedirectToAction("userDetailList", "UserDetail");
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                catch (SystemException ioe)
                {
                    ModelState.AddModelError("", ioe.Message);
                    ViewBag.errorMessage = ioe.Message.ToString();
                    return View("Error");
                }

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]// [CustomAuthorize(4)]//User Detail/Setup/Administration //
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            GetSelectList(model);

            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]//[CustomAuthorize(4)]//User Detail/Setup/Administration//
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            GetSelectList(model);

            if (ModelState.IsValid)
            {
                try
                {
                    UserDetailModel userdetailModel = db.UserDetails
                                    .Where(u => u.USE_Usercode == model.USE_Usercode)
                                    .Select(s => new UserDetailModel
                                    {
                                        Dep_Id = s.Dep_Id,
                                        USE_Account = s.USE_Account,
                                        USE_Email = s.USE_Email,
                                        USE_FName = s.USE_FName,
                                        USE_Id = s.USE_Id,
                                        USE_LName = s.USE_LName,
                                        USE_Nationality = s.USE_Nationality,
                                        USE_NickName = s.USE_NickName,
                                        USE_Status = s.USE_Status,
                                        USE_TelNo = s.USE_TelNo,
                                        USE_Usercode = s.USE_Usercode,
                                        Department_name = s.Department.Dep_Name
                                    }).FirstOrDefault();

                    if (userdetailModel != null)
                    {
                        ViewBag.Message = String.Format("Cannot insert duplicate User Code ({0}) in user account.", model.USE_Usercode);
                        return View(model);
                    }

                    var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, USE_Usercode = model.USE_Usercode };
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        //  Comment the following line to prevent log in until the user is confirmed.
                        //await SignInAsync(user, isPersistent: false);

                        //string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account");

                        // Uncomment to debug locally 
                        // TempData["ViewBagLink"] = callbackUrl;

                        //string VMessage = "Check your email and confirm your account, you must be confirmed "
                        //           + "before you can log in.";

                        //Go to Create UserDetail method.
                        UserDetail ud_model = new UserDetail();
                        ud_model.USE_Account = model.UserName;
                        ud_model.USE_Email = model.Email;
                        ud_model.USE_FName = model.USE_FName;
                        ud_model.USE_LName = model.USE_LName;
                        ud_model.USE_NickName = model.USE_NickName;
                        ud_model.USE_Nationality = model.USE_Nationality;
                        ud_model.USE_Status = model.USE_Status;
                        ud_model.USE_TelNo = model.USE_TelNo;
                        ud_model.USE_Usercode = model.USE_Usercode;
                        ud_model.Dep_Id = model.Dep_Id;
                        ud_model.USE_Password = "-";

                        var result_detail = db.UserDetails.Add(ud_model);
                        //Save Logs
                        if (result_detail != null)
                        {
                            string user_nickname = null;
                            if (Session["NickName"] != null)
                            {
                                user_nickname = Session["NickName"].ToString();
                            }
                            Log logmodel = new Log()
                            {
                                Log_Action_Id = result_detail.USE_Id,
                                Log_System = "Administration",
                                Log_Type = "User",
                                Log_Detail = String.Concat(result_detail.USE_Account, "/", result_detail.USE_Usercode),
                                Log_Action = "add",
                                Log_by = user_nickname,
                                Log_Date = DateTime.Now
                            };
                            db.Logs.Add(logmodel);
                        }
                        db.SaveChanges();

                        SendEmailCreateUser(model);

                        string VMessage = String.Format("Successfully created a new account, Employee ID {0}", model.USE_Usercode);

                        TempData["shortMessage"] = VMessage;

                        return RedirectToAction("userDetailList", "UserDetail");
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                catch (SystemException ioe)
                {
                    ModelState.AddModelError("", ioe.Message);
                    ViewBag.errorMessage = ioe.ToString();
                    return View("Error");
                }

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    /*
                    if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    {
                        ViewBag.errorMessage = "The user is not email confirmed";
                        return View("Error");
                    } 
                     */
                }
                else
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    ViewBag.errorMessage = "The user does not exist.";
                    return View("Error");
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                // "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                string messagebody = "<p style='font-size: 20px'>This process is used to reset OCTWEB application account password, please click the link : "
                         + "<a href=\"" + callbackUrl + "\">link</a></p><br/>"
                         + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a><br/>"
                         + "<p style='font-size: 20px'>If clicking the link above doesn't work, please copy and paste the URL in a new browser window instead.</p><br/><br/>";

                await UserManager.SendEmailAsync(user.Id, "OCTWEB - Reset Password.", messagebody);

                ViewBag.Message = "Check your email and click the link, You will be redirected to the Reset password page "
                                    + "and set your new password.";

                return View("Info");

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            ResetPasswordViewModel resetpass = new ResetPasswordViewModel();
            if (userId == null || code == null)
            {
                ViewBag.errorMessage = "User Id or Code is Null @Account/ResetPassword";
                return View("Error");
            }
            var user = UserManager.FindById(userId);

            if (user != null)
            {
                resetpass.UserId = user.Id;
                resetpass.Code = code;
                resetpass.UserName = user.UserName;
                return View(resetpass);
            }

            ViewBag.errorMessage = "Reset Password failed. @Account/ResetPassword";
            return View("Error");

        }


        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult ResetPasswordResult = await UserManager.ResetPasswordAsync(model.UserId, model.Code, model.NewPassword);
                if (ResetPasswordResult.Succeeded)
                {
                    var callbackUrl = Url.Action("Login", "Account", "", protocol: Request.Url.Scheme);

                    string messagebody = "<p style='font-size: 18px'>Successfully reset password on OCT WEB, you can log in by following this link. "
                             + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"
                             + "<p style='font-size: 18px'>New Password: <strong style='background-color:yellow'>" + model.NewPassword + "</strong></p><br/><br/> ";

                    await UserManager.SendEmailAsync(model.UserId, "OCTWEB - Reset your password successed.", messagebody);

                    TempData["shortMessage"] = "Your password has been changed.";

                    return RedirectToAction("Login");
                }
                else
                {
                    AddErrors(ResetPasswordResult);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        //return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                        var callbackUrl = Url.Action("Login", "Account", "", protocol: Request.Url.Scheme);

                        string messagebody = "<p style='font-size: 18px'>Successfully changed your password on OCT WEB, you can log in by following this link. "
                                 + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"
                                 + "<p style='font-size: 18px'>New Password: <strong style='background-color:yellow'>" + model.NewPassword + "</strong></p><br/><br/> ";

                        await UserManager.SendEmailAsync(User.Identity.GetUserId(), "OCTWEB - Change your password successed.", messagebody);

                        TempData["shortMessage"] = "Your password has been changed.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }


        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);

            string messagebody = "<p style='font-size: 20px'>This process is for your OCTWEB Registration/Login Application Account, click the link below: "
                         + "<a href=\"" + callbackUrl + "\">link</a></p><br/>"
                         + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a><br/>"
                         + "<p style='font-size: 20px'>If clicking the link above doesn't work, please copy and paste the URL in a new browser window instead.</p><br/><br/>";

            await UserManager.SendEmailAsync(userID, subject, messagebody);

            return callbackUrl;
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            IdentityResult result;
            try
            {
                result = await UserManager.ConfirmEmailAsync(userId, code);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.
                ViewBag.errorMessage = ioe.Message;
                return View("Error");
            }

            if (result.Succeeded)
            {
                return View();
            }

            // If we got this far, something failed.
            AddErrors(result);
            ViewBag.errorMessage = "ConfirmEmail failed";
            return View("Error");
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        //Get Menu 
        public MenuModels getMenu(int userId)
        {
            var userright_list = db.UserRights.Where(x => x.USE_Id == userId).Select(s => s.RIH_Id).ToList();
            MenuModels model = new MenuModels()
            {
                //Administration
                //Setup
                admin_setup = userright_list.Intersect(new List<int>() { 4, 6, 8, 10 }).Any() ? true : false, //
                UserDetail_4 = userright_list.Contains(4) ? true : false,
                ChangePassword_6 = userright_list.Contains(6) ? true : false,
                UserGroup_8 = userright_list.Contains(8) ? true : false,
                LogReport_10 = userright_list.Contains(10) ? true : false,

                //Check Appearance 300%
                check300 = userright_list.Intersect(new List<int>() { 26, 28, 29, 30, 31, 27 }).Any() ? true : false, ///
                //Information
                check300_inform = userright_list.Intersect(new List<int>() { 26 }).Any() ? true : false, //
                InformProblem_26 = userright_list.Contains(26) ? true : false,
                //Report
                check300_report = userright_list.Intersect(new List<int>() { 28, 29 }).Any() ? true : false, //
                ProblemReportGraph_28 = userright_list.Contains(28) ? true : false,
                ProblemReportData_29 = userright_list.Contains(29) ? true : false,
                //Setup
                check300_setup = userright_list.Intersect(new List<int>() { 30, 31, 27 }).Any() ? true : false, //
                LineName_30 = userright_list.Contains(30) ? true : false,
                PartName_31 = userright_list.Contains(31) ? true : false,
                ProblemList_27 = userright_list.Contains(27) ? true : false,

                //Drawing
                drawing = userright_list.Intersect(new List<int>() { 24, 12, 13, 14, 15, 16, 25 }).Any() ? true : false, ///
                //Report
                drawing_report = userright_list.Intersect(new List<int>() { 24, 12 }).Any() ? true : false, //
                FileShare_24 = userright_list.Contains(24) ? true : false,
                DrawingSearch_12 = userright_list.Contains(12) ? true : false,
                //Setup
                drawing_setup = userright_list.Intersect(new List<int>() { 13, 14, 15, 16, 25 }).Any() ? true : false, //
                Process_13 = userright_list.Contains(13) ? true : false,
                PartName_14 = userright_list.Contains(14) ? true : false,
                EngineerDrawing_15 = userright_list.Contains(15) ? true : false,
                ProcessDrawing_16 = userright_list.Contains(16) ? true : false,
                UploadFileShare_25 = userright_list.Contains(25) ? true : false,

                //HR
                hr = userright_list.Intersect(new List<int>() { 61, 57, 37, 32, 33, 34, 53, 54, 55, 56, 63, 64 }).Any() ? true : false, ///
                //Management Setup
                hr_manage = userright_list.Intersect(new List<int>() { 61, 57, 37, 32, 33, 34, 63 }).Any() ? true : false, //
                TrainingCoursesSetup_61 = userright_list.Contains(61) ? true : false,
                TrainingCoursesWSSetup_57 = userright_list.Contains(57) ? true : false,
                FormerEmpListSetup_37 = userright_list.Contains(37) ? true : false,
                EmployeeListSetup_32 = userright_list.Contains(32) ? true : false,
                TrainingCoursesManagement_33 = userright_list.Contains(33) ? true : false,
                MistakesSetup_34 = userright_list.Contains(34) ? true : false,
                LeaveSetup_63 = userright_list.Contains(63) ? true : false,
                BenefitSetup_71 = userright_list.Contains(71) ? true : false,
                //Report
                hr_report = userright_list.Intersect(new List<int>() { 53, 54, 55, 56, 64 }).Any() ? true : false, //
                EmployeeListReport_53 = userright_list.Contains(53) ? true : false,
                FormerEmpListReport_54 = userright_list.Contains(54) ? true : false,
                MistakesReport_55 = userright_list.Contains(55) ? true : false,
                TrainingCourses_56 = userright_list.Contains(56) ? true : false,
                LeaveReport_64 = userright_list.Contains(64) ? true : false,

                //IT System
                //IT Report
                it_report = userright_list.Intersect(new List<int>() { 11 }).Any() ? true : false, //
                ComputerGraph_11 = userright_list.Contains(11) ? true : false,

                //QRAP
                qrap = userright_list.Intersect(new List<int>() { 9, 3, 7, 5, 1, 2 }).Any() ? true : false, ///
                //Report
                qrap_report = userright_list.Intersect(new List<int>() { 9, 3 }).Any() ? true : false, //
                ProblemGraph_9 = userright_list.Contains(9) ? true : false,
                QRAPDetail_3 = userright_list.Contains(3) ? true : false,
                //Update and Modify
                qrap_update = userright_list.Intersect(new List<int>() { 7, 5, 1, 2 }).Any() ? true : false, //
                Approve_7 = userright_list.Contains(7) ? true : false,
                InformProblem_5 = userright_list.Contains(5) ? true : false,
                Answers_1 = userright_list.Contains(1) ? true : false,
                DefineSolution_2 = userright_list.Contains(2) ? true : false,

                //WS & Rule
                ws = userright_list.Intersect(new List<int>() { 22, 19, 17, 18, 20, 21, 23 }).Any() ? true : false, ///
                //Report
                ws_report = userright_list.Intersect(new List<int>() { 22, 19 }).Any() ? true : false, //
                WorkingStandardSearch_22 = userright_list.Contains(22) ? true : false,
                CompanyRuleSearch_19 = userright_list.Contains(19) ? true : false,
                //Setup Rule
                ws_setuprule = userright_list.Intersect(new List<int>() { 17, 18 }).Any() ? true : false, //
                CompanyRuleType_17 = userright_list.Contains(17) ? true : false,
                CompanyRule_18 = userright_list.Contains(18) ? true : false,
                //Setup WS
                ws_setupws = userright_list.Intersect(new List<int>() { 20, 21, 23 }).Any() ? true : false, //
                WorkingStandardType_20 = userright_list.Contains(20) ? true : false,
                WorkingStandard_21 = userright_list.Contains(21) ? true : false,
                WorkingStandardProcess_23 = userright_list.Contains(23) ? true : false,

                //System Support
                system_support = userright_list.Intersect(new List<int>() { 65, 66 }).Any() ? true : false, ///
                APBillingNote_65 = userright_list.Contains(65) ? true : false,
                ALBillingNote_66 = userright_list.Contains(66) ? true : false,
            };
            return model;

        }

        //
        //  /Account/SendEmail/
        public void SendEmailCreateUser(RegisterViewModel user)
        {
            try
            {
                SendMailCenterModel model = new SendMailCenterModel();

                //Send mail CC
                int idgroup_cc = db.UserGroups.Where(x => x.USG_Name == "SEND_MAIL_CREATE_ACCOUNT_CC").Select(s => s.USG_Id).FirstOrDefault();
                var userlist_cc = db.GroupMembers.Where(x => x.USG_Id == idgroup_cc).Select(x => x.USE_Id).ToList();
                var tomail_cc = db.UserDetails.Where(x => userlist_cc.Contains(x.USE_Id)).Select(s => s.USE_Email).ToList();

                var callbackUrl = Url.Action("Index", "Home", "", protocol: Request.Url.Scheme);

                string html = "<strong style='font-size: 20px'> Dear K. " + user.USE_Usercode + ", </strong>"
                             + "<p style='font-size: 18px'>Successfully created an account on OCT WEB, you can log in by following this link. "
                             + "<a href=\"" + callbackUrl + "\">" + callbackUrl + "</a></p><br/>"
                             + "<p style='font-size: 18px'>User Account: <strong>" + user.UserName + "</strong></p> "
                             + "<p style='font-size: 18px'>Initial Password: <strong style='background-color:yellow'>" + user.Password + "</strong></p> "
                             + "<p style='font-size: 18px'>And you can reset your password by clicking the <em>Change Password </em> button on the OCT WEB. </p>"

                             + "<br/><br/><br/><br/>"
                             + "<p style='font-size: 18px'>Best Regards,<br/>"
                             + "OCT WEB</p>";

                List<string> tomail = new List<string>();
                tomail.Add(user.Email);

                model.To = tomail;
                model.Tocc = tomail_cc;
                model.Subject = "New Account OCT WEB";
                model.Body = html;

                SendMailCenterController.SendMail(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                throw;
            }

        }

        public RegisterViewModel GetSelectList(RegisterViewModel model)
        {
            List<Department> departmentList = db.Departments.OrderBy(o => o.Dep_Name).ToList();
            List<SelectListItem> selectDepartment = new List<SelectListItem>();
            selectDepartment = departmentList.Select(s => new SelectListItem { Value = s.Dep_Id.ToString(), Text = s.Dep_Name }).ToList();
            model.Departments = selectDepartment;
            model.National = new[]
                {
                    new SelectListItem { Value = "Thai", Text = "Thai" },
                    new SelectListItem { Value = "Japanese", Text = "Japanese" } ,
                }.ToList();

            return model;
        }

    }
}