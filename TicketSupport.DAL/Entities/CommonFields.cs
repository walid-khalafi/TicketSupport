using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.DAL.Entities
{
	public class CommonFields
	{
		public CommonFields()
		{
		}
		[Key]
		public string Id { get; set; }
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "تاریخ ویرایش")]
        public DateTime? EditedAt { get; set; }
        [Display(Name = "ایجاد توسط")]
        public string CreatedBy { get; set; }
        [Display(Name = "ویرایش توسط")]
        public string? EditedBy { get; set; }
        [Display(Name = "آی پی")]
        public string IPAddress { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}

