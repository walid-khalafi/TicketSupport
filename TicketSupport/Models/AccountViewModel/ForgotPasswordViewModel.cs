using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.AccountViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }
    }
}

