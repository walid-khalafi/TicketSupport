using System;
using System.ComponentModel.DataAnnotations;
using TicketSupport.DAL.Enums;

namespace TicketSupport.DAL.Entities.User
{
	public class RoleAccess:CommonFields
	{
		public RoleAccess()
		{
		}
		[Display(Name ="کاربران")]
        public AccessLevel Users { get; set; }
        [Display(Name = "سطوح دسترسی")]
        public AccessLevel ManageRoleAccess { get; set; }
        [Display(Name = "دپارتمان")]
        public AccessLevel Department { get; set; }
	}
}

