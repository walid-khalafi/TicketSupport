using System;
using TicketSupport.DAL.Enums;

namespace TicketSupport.DAL.Entities.Catalog
{
	public class Ticket:CommonFields
	{
        public Ticket()
        {
		}

        public string Subject { get; set; }
        public string Body { get; set; }
        public string Attachments { get; set; }
        public string Resolved { get; set; }
        public string Closed { get; set; }
        public string Seen { get; set; }
        public string Replays { get; set; }
        public string Assign { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentServiceId { get; set; }
        public TicketPriority TicketPriority { get; set; }
    }
}

