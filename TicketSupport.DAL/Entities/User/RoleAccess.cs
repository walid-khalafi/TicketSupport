using System;
using TicketSupport.DAL.Enums;

namespace TicketSupport.DAL.Entities.User
{
	public class RoleAccess:CommonFields
	{
		public RoleAccess()
		{
		}
        public AccessLevel Users { get; set; }
        public AccessLevel ManageRoleAccess { get; set; }
    }
}

