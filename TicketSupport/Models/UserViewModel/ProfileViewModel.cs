using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSupport.WEB.Models.UserViewModel
{
	public class ProfileViewModel
	{
		public ProfileViewModel()
		{
		}

        public string Id { get; set; }
        [Display(Name = "نام")]
        public string FirstName { get; set; }
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }
        [Display(Name = "ایمیل")]
        public string Email { get; set; }
        [Display(Name = "تلفن همراه")]
        public string Phonenumber { get; set; }
        [Display(Name = "تصویر")]
        public string Avatar { get; set; }
        [Display(Name = "تم")]
        public string ThemeColor { get; set; }
        public string NavigationSize { get; set; }
        [Display(Name = "سطح دسترسی")]
        public string Role { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public bool TwoFactorEnabled { get; set; }

    }
}

