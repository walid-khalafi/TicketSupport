using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSupport.DAL;
using TicketSupport.Services.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TicketSupport.WEB.Models.AccountViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using TicketSupport.DAL.Entities.User;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.WEB.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private string _user_id;
        private string _ipAddress;

        public UsersController(IHttpContextAccessor httpContextAccessor, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext context, IProfileService profileService)
        {
            _context = context;
            _profileService = profileService;
            _signInManager = signInManager;
            _userManager = userManager;
            _httpContextAccessor=httpContextAccessor;
            _ipAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            _user_id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /<controller>/
        public IActionResult Index()
        {
           
            return View();
        }

        public static bool IsPhoneNumber(string number)
        {
            string pat1 = "(09[0-9]{9}$)";
            //return Regex.Match(number, @"^(\+[0-9]{9})$").Success;
            return Regex.Match(number, pat1).Success;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await Task.FromResult(_context.Departments.Where(x => x.IsDeleted == false).ToList());
            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Title");
            return View();
        }

        [HttpPost]
       
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            bool isPhoneNumber = IsPhoneNumber(model.PhoneNumber);
            if (!isPhoneNumber)
            {
                TempData["error_msg"] = "شماره تلفن  به درستی وارد نشده است. ";
                return View(model);
            }


            var db_user = _context.Users.FirstOrDefault(x => x.PhoneNumber == model.PhoneNumber);
            if (db_user != null)
            {
                TempData["error_msg"] = "ایمیل یا شماره تماس قبلا در سیستم ثبت شده است درصورتی که رمز عبور خود را فراموش کرده اید از بخش فراموشی رمز عبور اقدام به بازیابی حساب کاربری خود نمایید.";
                return View(model);
            }
            string user_id = Guid.NewGuid().ToString();
            var user = new IdentityUser { Id = user_id, UserName = model.PhoneNumber, Email = "notset@notset.com", PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.PhoneNumber);
            if (result.Succeeded)
            {
                var profile = await Task.FromResult(_context.Profiles.FirstOrDefault(x => x.Id == user_id));
                if (profile !=null)
                {
                    _context.Profiles.Remove(profile);
                }
               
                _context.Profiles.Add(new DAL.Entities.User.Profile()
                {
                    Avatar = string.Empty,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ThemeColor = "light",
                    Id = user_id,
                    NavigationSize = "normal-navigation",
                    CreatedAt = DateTime.Now,
                    CreatedBy = _user_id,
                    IPAddress = _ipAddress,
                    DeletedAt = DateTime.Now,
                    DepartmentId = model.DepartmentId,
                    EditedAt = DateTime.Now,
                    EditedBy = _user_id,
                    IsDeleted = false,
                    CompanyName = model.CompanyName
                });
              
                var role = _context.Roles.FirstOrDefault(x => x.Name == "Client");
                if (role == null)
                {
                    _context.Roles.Add(new IdentityRole
                    {
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        Id = Guid.NewGuid().ToString(),
                        Name = "Client",
                        NormalizedName = "CLIENT",
                    });
                 
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch(Exception ex){
                    Console.WriteLine(ex.Message);
                }
                 
                await _userManager.AddToRoleAsync(user, "Client");

                TempData["success_msg"] = string.Format("اطلاعات کاربر {0} {1} با موفقیت در سیستم ثبت شد", model.FirstName, model.LastName);
                return RedirectToAction("Create");

            }
            TempData["error_msg"] = "لطفا فرم ثبت نام را با دقت تکمیل نمایید";
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Create");
        }
        [HttpPost]
        public async Task<JsonResult> LoadAjaxUsers()
        {
            // server side parameters
            int start = Convert.ToInt32(Request.Form["start"]);
            int length = Convert.ToInt32(Request.Form["length"]);
            string searchValue = Request.Form["search[value]"].ToString();
            string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].ToString() + "][name]"];
            string sortDirection = Request.Form["order[0][dir]"].ToString();

            List<ProfileViewModel> list = new List<ProfileViewModel>();

            var users = await Task.FromResult(_context.Users.ToList());

            if (users == null)
            {
                return Json(new { data = list, draw = Request.Form["draw"].ToString(), recordsTotal = 0, recordsFiltered = 0 });
            }

            int recordsTotal = users.Count;

            foreach (var item in users)
            {
                var profile = await _profileService.GetUserProfileAsync(item.Id);
                var roles = await _profileService.GetUserRolesAsync(item.Id);
                var department = await _context.Departments.FindAsync(profile.DepartmentId);
                if (profile != null)
                {
                    try
                    {
                        list.Add(new ProfileViewModel()
                        {
                            Id = profile.Id,
                            Avatar = (!string.IsNullOrWhiteSpace(profile.Avatar) ? profile.Avatar : ""),
                            Email = profile.Email,
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            Phonenumber = profile.Phonenumber,
                            ThemeColor = profile.ThemeColor,
                            Username = profile.Username,
                            TwoFactorEnabled = item.TwoFactorEnabled,
                            Role = (roles.Count > 0 ? roles[0] : ""),
                            DepartmentId = (department !=null ? department.Title:""),
                            CompanyName = profile.CompanyName
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }

            }

            list = list.OrderBy(x => x.LastName).ToList();
            if (!string.IsNullOrEmpty(searchValue)) // fiter 
            {
                list = list.Where(x => x.FullName.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            if (sortDirection == "asc")
            {
                switch (sortColumnName)
                {
                    case "FirstName":
                        list = list.OrderBy(x => x.FirstName).ToList();
                        break;
                    case "LastName":
                        list = list.OrderBy(x => x.LastName).ToList();
                        break;
                    case "Username":
                        list = list.OrderBy(x => x.Username).ToList();
                        break;
                    default:
                        break;
                }

            }
            else
            {
                switch (sortColumnName)
                {
                    case "FirstName":
                        list = list.OrderByDescending(x => x.FirstName).ToList();
                        break;
                    case "LastName":
                        list = list.OrderByDescending(x => x.LastName).ToList();
                        break;
                    case "Username":
                        list = list.OrderByDescending(x => x.Username).ToList();
                        break;
                    default:
                        break;
                }
            }

            // paging
            list = list.Skip(start).Take(length).ToList();

            int recordsFilteredTotal = list.Count;
            return Json(new { data = list, draw = Request.Form["draw"].ToString(), recordsTotal = recordsTotal, recordsFiltered = recordsTotal });

        }


        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                TempData["error_msg"] = "لطفا یک کاربر را جهت ویرایش انتخاب نمایید";
                return RedirectToAction("Index");
            }
            ProfileViewModel profile = await _profileService.GetUserProfileAsync(Id);

            if (string.IsNullOrWhiteSpace(profile.Id))
            {
                return StatusCode(404);
            }

            var departments = await Task.FromResult( _context.Departments.Where(x=>x.IsDeleted == false).ToList());
            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Title",profile.DepartmentId);

            var roles = await Task.FromResult(_context.Roles.ToList());
            ViewData["role_id"] = new SelectList(roles, "Id", "Name");

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {


            var user = await Task.FromResult(_context.Users.Find(model.Id));
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var profile = await _profileService.GetUserProfileAsync(user.Id);

            
            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var phoneNumber = user.PhoneNumber;
            if (model.Phonenumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.Phonenumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.Avatar))
            {
                var Avatar = profile.Avatar;
                if (model.Avatar != Avatar)
                {
                    if (!await _profileService.ChangeUserAvatarAsync(user.Id, model.Avatar))
                    {
                        throw new ApplicationException($"Unexpected error occurred setting Avatar for user with ID '{user.Id}'.");
                    }
                }
            }




            var full_name = profile.FullName;

            if (model.FirstName + " " + model.LastName != profile.FullName)
            {
                if (!await _profileService.ChangeUserFullNameAsync(user.Id, model.FirstName, model.LastName))
                {
                    throw new ApplicationException($"Unexpected error occurred setting Avatar for user with ID '{user.Id}'.");
                }
            }

            if (!await _profileService.ChangeUserDepartmentAsync(user.Id, model.DepartmentId))
            {
                throw new ApplicationException($"Unexpected error occurred setting Avatar for user with ID '{user.Id}'.");
            }
            

            TempData["success_msg"] = "پروفایل با موفقیت ویرایش شد";
            StatusMessage = "پروفایل باموفقیت ویرایش شد";
            return RedirectToAction("Index");

        }


        [HttpGet]
        public async Task<IActionResult> ResetUserPass(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return StatusCode(404);
            }
            var result = await _profileService.ResetUserPassAsync(id);
            if (result)
            {
                TempData["success_msg"] = "رمز عبور با موفقیت به qwerty بازنشانی شد.";
            }
            else
            {
                TempData["error_msg"] = "خطا در بازنشانی رمز عبور کاربر رخ داده است.";
            }

            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTheme(string id, string theme)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(theme))
            {
                return Json("false");
            }

            var profile = await Task.FromResult(_context.Profiles.FirstOrDefault(x => x.Id == id));
            if (profile == null)
            {
                return Json("ProfileNotFound");
            }

            profile.ThemeColor = theme;
            await _context.SaveChangesAsync();
            return Json("true");
        }

        [HttpPost]
        public async Task<JsonResult> ChangeNavigation(string id, string size)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(size))
            {
                return Json("false");
            }

            var profile = await Task.FromResult(_context.Profiles.FirstOrDefault(x => x.Id == id));
            if (profile == null)
            {
                return Json("ProfileNotFound");
            }

            profile.NavigationSize = size;
            await _context.SaveChangesAsync();
            return Json("true");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccessLevel(string id, string role_id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var profile = await _profileService.GetUserProfileAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            var role = await Task.FromResult(_context.Roles.FirstOrDefault(x => x.Id == role_id));
            if (role == null)
            {
                TempData["error_msg"] = "سطح دسترسی یافت نشد";
                return RedirectToAction("Edit", "Users", new { id = id });
            }
            var user_role = await Task.FromResult(_context.UserRoles.FirstOrDefault(x => x.UserId == id));
            if (user_role == null)
            {
                _context.UserRoles.Add(new IdentityUserRole<string>()
                {
                    RoleId = role.Id,
                    UserId = id
                });
            }
            else
            {
                _context.UserRoles.Remove(user_role);
                _context.UserRoles.Add(new IdentityUserRole<string>()
                {
                    RoleId = role.Id,
                    UserId = id
                });
            }
            await _context.SaveChangesAsync();
            TempData["success_msg"] = "سطح دسترسی کاربر با موفقیت ویرایش شد";
            return RedirectToAction("Edit", "Users", new { id = id });
        }


        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IdentityUser user = await Task.FromResult(_context.Users.FirstOrDefault(m => m.Id == id));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _profileService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }



    }
}

