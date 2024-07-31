using System;
namespace TicketSupport.Services.Catalog
{
	public class TicketAssignModel
	{
		public TicketAssignModel()
		{
		}
        public string assigned_by
        {
            get;
            set;
        }
        public DateTime assigned_date
        {
            get;
            set;
        }
        public string user_id
        {
            get;
            set;
        }
    }
}

