using System;
using Microsoft.AspNetCore.Identity;
using TicketSupport.DAL;
using TicketSupport.DAL.Entities.User;

namespace TicketSupport.Services.Users
{
	public class ProfileService:IProfileService
	{
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ProfileService(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {

            _context = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> ChangeUserAvatarAsync(string id, string img64base)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var user = await Task.FromResult(_context.Users.Find(id));
            if (user != null)
            {
                var profile = await Task.FromResult(_context.Profiles.Find(user.Id));
                if (profile == null)
                {
                    _context.Profiles.Add(new DAL.Entities.User.Profile()
                    {
                        Id = user.Id,
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        ThemeColor = "light",
                        Avatar = img64base,
                        NavigationSize = "normal-navigation",
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.Id,
                        DepartmentId = Guid.Empty.ToString(),
                        EditedAt = DateTime.Now,
                        EditedBy = user.Id,
                        IPAddress = string.Empty,
                        DeletedAt = DateTime.Now,
                        IsDeleted = false
                    });
                    await _context.SaveChangesAsync();
                    return true;
                }
                profile.Avatar = img64base;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }

        public async Task<bool> ChangeUserDepartmentAsync(string id, string department_id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile !=null)
            {
                profile.DepartmentId = department_id;
                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return false;
         
        }

        public async Task<bool> ChangeUserFullNameAsync(string id, string first_name, string last_name)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(first_name) || string.IsNullOrWhiteSpace(last_name))
            {
                return false;
            }
            var user = await Task.FromResult(_context.Users.Find(id));
            if (user != null)
            {
                var profile = await Task.FromResult(_context.Profiles.Find(user.Id));
                if (profile == null)
                {
                    _context.Profiles.Add(new DAL.Entities.User.Profile()
                    {
                        Id = user.Id,
                        FirstName = first_name,
                        LastName = last_name,
                        ThemeColor = "light",
                        Avatar = string.Empty,
                        NavigationSize = "normal-navigation",
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.Id,
                        DepartmentId = Guid.Empty.ToString(),
                        EditedAt = DateTime.Now,
                        EditedBy = user.Id,
                        IPAddress = string.Empty,
                        DeletedAt = DateTime.Now,
                        IsDeleted = false
                    });
                }
                else
                {
                    profile.FirstName = first_name;
                    profile.LastName = last_name;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }
            var user = await Task.FromResult(_context.Users.Find(id));
            if (user != null)
            {
                var profile = await Task.FromResult(_context.Profiles.Find(user.Id));
                if (profile != null)
                {
                    profile.IsDeleted = true;
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;

            }
            return false;
        }

        public async Task<string> GetThemeColorAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "light";
            }
            var profile = await Task.FromResult(_context.Profiles.Find(id));
            if (profile != null)
            {
                return profile.ThemeColor;
            }
            return "light";
        }

        public async Task<ProfileViewModel> GetUserProfileAsync(string id)
        {
            ProfileViewModel model = new ProfileViewModel();
            if (string.IsNullOrWhiteSpace(id))
            {
                return model;
            }
            var user = await Task.FromResult(_context.Users.Find(id));
            if (user != null)
            {
                var profile = await Task.FromResult(_context.Profiles.Find(id));
                model.Username = user.UserName;
                model.Email = user.Email;
                model.Id = user.Id;
                model.Phonenumber = user.PhoneNumber;
                if (profile != null)
                {
                    model.FirstName = profile.FirstName;
                    model.LastName = profile.LastName;
                    model.ThemeColor = profile.ThemeColor;
                    model.Avatar = profile.Avatar;
                    model.NavigationSize = profile.NavigationSize;
                    model.DepartmentId = profile.DepartmentId;
                    model.CompanyName = profile.CompanyName;

                }
                var role = await GetUserRolesAsync(user.Id);

                try
                {
                    if (role.Count > 0)
                    {
                        model.Role = role[0];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return model;
        }

        public async Task<RoleAccess> GetUserRoleAccessAsync(string id)
        {
            var roles = await GetUserRolesAsync(id);
            if (roles == null)
            {
                return new RoleAccess() { Id = "0", ManageRoleAccess = DAL.Enums.AccessLevel.None, Users = DAL.Enums.AccessLevel.None};
            }
            if (string.IsNullOrWhiteSpace(roles[0]))
            {
                return new RoleAccess() { Id = "0", ManageRoleAccess = DAL.Enums.AccessLevel.None, Users = DAL.Enums.AccessLevel.None };
            }
            var role = await Task.FromResult(_context.Roles.FirstOrDefault(x => x.Name == roles[0]));
            if (role == null)
            {
                return new RoleAccess() { Id = "0", ManageRoleAccess = DAL.Enums.AccessLevel.None, Users = DAL.Enums.AccessLevel.None };
            }
            var roles_access = await Task.FromResult(_context.RoleAccesses.Find(role.Id));
            if (roles_access == null)
            {
                return new RoleAccess() { Id = "0", ManageRoleAccess = DAL.Enums.AccessLevel.None, Users = DAL.Enums.AccessLevel.None };
            }
            return roles_access;
        }

        public async Task<List<string>> GetUserRolesAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new List<string>();
            }
            var user = await Task.FromResult(_context.Users.Find(id));

            if (user != null)
            {
                var roles = await Task.FromResult(_context.UserRoles.Where(x => x.UserId == user.Id).ToList());
                List<string> user_roles = new List<string>();
                if (roles != null)
                {
                    if (roles.Count > 0)
                    {
                        foreach (var item in roles)
                        {
                            var role = _context.Roles.Find(item.RoleId);
                            if (role != null)
                            {
                                user_roles.Add(role.Name);
                            }
                        }
                        return user_roles;
                    }
                }

            }
            return new List<string>();
        }

        public async Task<List<ProfileViewModel>> GetUsersProfileAsync()
        {
            var users = await Task.FromResult(_context.Users.ToList());
            List<ProfileViewModel> profiles = new List<ProfileViewModel>();

            foreach (var item in users)
            {
                var profile = await GetUserProfileAsync(item.Id);
                profiles.Add(profile);
            }

            return profiles;
        }

        public async Task<bool> ResetUserPassAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }
            var user = await Task.FromResult(_context.Users.Find(id));
            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, code, "qwerty");
                return true;

            }
            return false;
        }
    }
}

