using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.AccountViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "گذرواژه جدید")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تایید گذرواژه")]
        [Compare("Password", ErrorMessage = "گذرواژه یکسان نمی باشد.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}

