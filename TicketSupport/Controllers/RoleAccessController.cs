using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSupport.DAL;
using TicketSupport.DAL.Entities.User;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.Controllers
{
    [Authorize]
    public class RoleAccessController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private readonly ITicketService _ticketService;
        private string _user_id;
        private string _ipAddress;

        public RoleAccessController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IProfileService profileService, ITicketService ticketService)
        {
            _context = context;
            _ticketService = ticketService;
            _httpContextAccessor = httpContextAccessor;
            _profileService = profileService;
            _ipAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            _user_id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            return View(await Task.FromResult( _context.RoleAccesses.ToList()));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleAccess roleAccess, string role_title)
        {

            var role = await Task.FromResult(_context.Roles.FirstOrDefault(x => x.Name == role_title));
            if (role != null)
            {
                TempData["error_msg"] = "این سطح دسترسی قبلا در سیستم تعریف شده است";
                return RedirectToAction(nameof(Index));
            }
            string role_id = Guid.NewGuid().ToString();
            _context.Roles.Add(new IdentityRole
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Id = role_id,
                Name = role_title,
                NormalizedName = role_title.ToUpper(),
            });
            roleAccess.Id = role_id;
            roleAccess.CreatedAt = DateTime.Now;
            roleAccess.CreatedBy = _user_id;
            roleAccess.EditedAt = DateTime.Now;
            roleAccess.EditedBy = _user_id;
            roleAccess.IsDeleted = false;
            roleAccess.IPAddress = _ipAddress;
            _context.Add(roleAccess);
            await _context.SaveChangesAsync();
            TempData["success_msg"] = "سطح دسترسی با موفقیت ایجاد شد";
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await Task.FromResult(_context.Roles.Find(id));

            if (role == null)
            {
                return NotFound();
            }

           
            var roleAccess = await _context.RoleAccesses.FindAsync(id);
            if (roleAccess == null)
            {
                return NotFound();
            }
            ViewData["RoleName"] = role.Name;
            return View(roleAccess);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string role_title, RoleAccess model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            try
            {
                var role = await Task.FromResult(_context.Roles.Find(id));
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = role_title;


                var role_access = await Task.FromResult(_context.RoleAccesses.Find(id));
                if (role_access == null)
                {
                    return NotFound();

                }

                //update
                role_access.ManageRoleAccess = model.ManageRoleAccess;
                role_access.Users = model.Users;
                role_access.Department = model.Department;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roleAccess = await _context.RoleAccesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roleAccess == null)
            {
                return NotFound();
            }

            return View(roleAccess);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var roleAccess = await _context.RoleAccesses.FindAsync(id);
            _context.RoleAccesses.Remove(roleAccess);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoleAccessExists(string id)
        {
            return _context.RoleAccesses.Any(e => e.Id == id);
        }

    }
}

