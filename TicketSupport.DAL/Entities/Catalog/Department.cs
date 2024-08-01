using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.DAL.Entities.Catalog
{
	public class Department:CommonFields
	{
		public Department()
		{
		}
		[Display(Name ="عنوان")]
		public string Title { get; set; }
        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        [Display(Name = "مدیر دپارتمان")]
        public string DepartmentAdminId { get; set; }
	}
}

