using System;
namespace TicketSupport.Services.Catalog
{
	public class ReplayTicketModel
	{
		public ReplayTicketModel()
		{
		}

        public string text
        {
            get;
            set;
        }
        public string user_id
        {
            get;
            set;
        }

        public bool is_me
        {
            get;
            set;
        }
        public DateTime created_at
        {
            get;
            set;
        }
    }
}

