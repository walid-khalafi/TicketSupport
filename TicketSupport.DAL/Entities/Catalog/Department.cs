using System;
namespace TicketSupport.DAL.Entities.Catalog
{
	public class Department:CommonFields
	{
		public Department()
		{
		}
		public string Title { get; set; }
		public string Description { get; set; }
		public string DepartmentAdminId { get; set; }
	}
}

