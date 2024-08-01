using System;
using System.Threading.Tasks;
using TicketSupport.DAL.Entities.User;

namespace TicketSupport.Services.Users
{
	public interface IProfileService
	{
        Task<ProfileViewModel> GetUserProfileAsync(string id);
        Task<bool> ChangeUserAvatarAsync(string id, string img64base);
        Task<bool> ChangeUserFullNameAsync(string id, string first_name, string last_name);
        Task<bool> ChangeUserDepartmentAsync(string id, string department_id);
        Task<List<string>> GetUserRolesAsync(string id);
        Task<bool> ResetUserPassAsync(string id);
        Task<string> GetThemeColorAsync(string id);
        Task<List<ProfileViewModel>> GetUsersProfileAsync();
        Task<RoleAccess> GetUserRoleAccessAsync(string id);
        Task<bool> DeleteUserAsync(string id);
    }
}

