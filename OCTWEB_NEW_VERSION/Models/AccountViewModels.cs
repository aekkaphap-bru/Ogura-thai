using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace OCTWEB_NET45.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public int USE_Id { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email: ")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User code: ")]
        public int USE_Usercode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Nationality: ")]
        public string USE_Nationality { get; set; }
        public List<System.Web.Mvc.SelectListItem> National { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Department ID: ")]
        public int Dep_Id { get; set; }
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; }

        [Display(Name = "User Status: ")]
        public int USE_Status { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name: ")]
        public string USE_FName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name: ")]
        public string USE_LName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Nick Name: ")]
        public string USE_NickName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Tel. : ")]
        public string USE_TelNo { get; set; }

        public bool BoolStatus
        {
            get { return USE_Status == 1; }
            set { USE_Status = value ? 1 : 0; }
        }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "User name: ")]
        public string UserName { get; set; }


        [Display(Name = "Email: ")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {

        [DataType(DataType.Text)]
        [Display(Name = "User Account: ")]
        public string UserName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "User Id: ")]
        public string UserId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Code: ")]
        public string Code { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password: ")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password: ")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
