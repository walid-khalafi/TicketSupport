using System;
namespace TicketSupport.DAL.Entities.User
{
	public class Profile:CommonFields
	{
		public Profile()
		{
		}

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? CompanyName { get; set; }     
        public string? DepartmentId { get; set; }
        public string Avatar { get; set; }
        public string ThemeColor { get; set; }
        public string NavigationSize { get; set; }
    }
}

