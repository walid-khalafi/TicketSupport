using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.AccountViewModel
{
    public class RegisterViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "موبایل")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تکرار رمز عبور")]
        [Compare("Password", ErrorMessage = "رمز عبور یکسان نمی باشد")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "نشانی")]
        public string address { get; set; }
        [Display(Name = "تاریخ تولد")]
        public string brith_date { get; set; }
    }
}

