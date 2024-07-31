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
		public DateTime CreatedAt { get; set; }
		public DateTime? EditedAt { get; set; }
		public string CreatedBy { get; set; }
		public string? EditedBy { get; set; }
		public string IPAddress { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}

