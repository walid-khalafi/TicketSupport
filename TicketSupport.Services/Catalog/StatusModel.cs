using System;
namespace TicketSupport.Services.Catalog
{
	public class StatusModel
	{
		public StatusModel()
		{
		}
        public string user_id
        {
            get;
            set;
        }
        public DateTime date
        {
            get;
            set;
        }
        public bool value { get; set; }
    }
}

