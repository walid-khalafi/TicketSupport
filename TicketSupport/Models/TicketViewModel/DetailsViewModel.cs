using System;
using TicketSupport.DAL.Entities.Catalog;
using TicketSupport.DAL.Entities.User;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;

namespace TicketSupport.WEB.Models.TicketViewModel
{
	public class DetailsViewModel
	{
		public DetailsViewModel()
		{
		}
		public string Id { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public ProfileViewModel CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
		public List<AttachmentFileInfo> Attachments { get; set; }

        public Department Department { get; set; }
		public DepartmentService DepartmentService { get; set; }

		public List<ReplayTicketModel> Replays { get; set; }
		public string IsClosed { get; set; }
		public bool CanAssignUserToTicket { get; set; }

        public string IsResolved { get; set; }
        public UserListModel AssignedUser { get; set; }
        public List<UserListModel> TicketUsersSupport { get; set; }

	}
}

