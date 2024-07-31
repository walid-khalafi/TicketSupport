using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSupport.DAL;
using TicketSupport.DAL.Entities.Catalog;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.WEB.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private readonly ITicketService _ticketService;
        private string _user_id;
        private string _ipAddress;

        public DepartmentController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IProfileService profileService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _profileService = profileService;
            _ipAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            _user_id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var model = await _context.Departments.ToListAsync();
            if (model == null)
            {
                return View(new List<Department>());
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            var department = await Task.FromResult(_context.Departments.FirstOrDefault(x => x.Title == model.Title && x.Id == model.Id && x.IsDeleted == true));
            if (department == null)
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedAt = DateTime.Now;
                model.CreatedBy = _user_id;
                model.IPAddress = _ipAddress;
                model.EditedBy = _user_id;
                model.EditedAt = DateTime.Now;
                model.IsDeleted = false;
                _context.Departments.Add(model);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return RedirectToAction("Index", "Department");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var department = await _context.Departments.FindAsync(model.Id);
            if (department == null || department.IsDeleted)
            {
                return NotFound();
            }

            department.IPAddress = _ipAddress;
            department.EditedBy = _user_id;
            department.EditedAt = DateTime.Now;
            department.Title = model.Title;
            department.Description = model.Description;
            department.DepartmentAdminId = model.DepartmentAdminId;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index", "Department");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            department.IsDeleted = true;
            department.DeletedAt = DateTime.Now;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Index", "Department");
        }
    }
}

