using System;
namespace TicketSupport.Services.Catalog
{
	public class UserListModel
	{
		public UserListModel()
		{
		}

        public string user_id
        {
            get;
            set;
        }
        public string full_name
        {
            get;
            set;
        }

        public string avatar
        {
            get;
            set;
        }
        public DateTime date { get; set; }


    }
}

