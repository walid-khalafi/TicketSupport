using System;
namespace TicketSupport.DAL.Entities.Catalog
{
	public class DepartmentService:CommonFields
	{
		public DepartmentService()
		{
		}
        public string DepartmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }
}

