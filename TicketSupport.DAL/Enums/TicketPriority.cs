using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TicketSupport.DAL.Enums
{
    public enum TicketPriority
    {
        [Display(Name = "خیلی زیاد")]
        very_high = 100,
        [Display(Name = "زیاد")]
        high = 200,
        [Display(Name = "متوسط")]
        medium = 300,
        [Display(Name = "کم")]
        low = 400,
        [Display(Name = "خیلی کم")]
        very_low = 500
    }
}

