using System;
using System.ComponentModel.DataAnnotations;
using TicketSupport.DAL.Enums;
using TicketSupport.WEB.Models.UserViewModel;

namespace TicketSupport.WEB.Models.ManageViewModels
{
    public class IndexViewModel:ProfileViewModel
    {
        public string StatusMessage { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
        public AccessLevel access_to_stock { get; set; }
        public AccessLevel access_to_units { get; set; }
        public AccessLevel access_to_reports { get; set; }
        public AccessLevel access_to_users { get; set; }
        public AccessLevel access_to_invoice { get; set; }
        public AccessLevel access_to_sms { get; set; }


    }
}
