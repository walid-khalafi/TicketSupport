using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TicketSupport.DAL.Entities.Catalog
{
	public class DepartmentService:CommonFields
	{
		public DepartmentService()
        {
		}
        public string DepartmentId { get; set; }
        [Display(Name = "عنوان")]
        public string Title { get; set; }
        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        [Display(Name = "کارشناس پیشفرض")]
        public string? DefaultUserId { get; set; }

	}
}

