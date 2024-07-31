using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.AccountViewModel
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

