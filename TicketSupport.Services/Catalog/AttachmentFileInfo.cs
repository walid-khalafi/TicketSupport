using System;
namespace TicketSupport.Services.Catalog
{
	public class AttachmentFileInfo
	{
		public AttachmentFileInfo()
		{
		}

        public string Name
        {
            get;
            set;
        }
        public string ContentType
        {
            get;
            set;
        }
        public long Size
        {
            get;
            set;
        }
        public string Base64String
        {
            get;
            set;
        }
    }
}

