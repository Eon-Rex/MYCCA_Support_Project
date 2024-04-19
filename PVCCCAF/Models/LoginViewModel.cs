using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;


namespace PVCCCAF.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name ="Username")]
        public string UserId { get; set; }
        public string HDUserId { get; set; }

        //[Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string HDPassword { get; set; }

        public string hdrandomseed { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "Old Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The New Password and Confirm Password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class SetPaswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The New Password and Confirm Password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
        public string HDConfirmPassword { get; set; }

        [Required]
        public string OTP { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public string hdrandomseed { get; set; }
    }

    public class RequestOTPViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserId { get; set; }

        [Display(Name = "Enter OTP")]
        public string OTP { get; set; }
    }
}