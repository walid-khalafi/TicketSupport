using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.AccountViewModel
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Display(Name = "مرا بخاطر بسپار")]
        public bool RememberMe { get; set; }
    }
}

