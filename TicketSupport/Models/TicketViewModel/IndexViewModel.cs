using System;
namespace TicketSupport.WEB.Models.TicketViewModel
{
	public class IndexViewModel
	{
		public IndexViewModel()
		{
		}

        public string ticket_id { get; set; }
        public string avatar { get; set; }
        public string fullname { get; set; }

        public string service_type { get; set; }
        public string subject { get; set; }
        public string last_closed_status { get; set; }
        public string last_resolved_status { get; set; }
        public string priority { get; set; }
        public string priority_css_class { get; set; }
        public string user_assigned { get; set; }
        public string created_at { get; set; }
        public DateTime created_at_en { get; set; }
        public bool have_attachment { get; set; }
    }
}

