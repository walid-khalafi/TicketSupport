using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketSupport.DAL;
using TicketSupport.DAL.Entities.Catalog;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.WEB.Controllers
{
    [Authorize]
    public class DepartmentServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private readonly ITicketService _ticketService;
        private string _user_id;
        private string _ipAddress;


        public DepartmentServiceController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IProfileService profileService, ITicketService ticketService)
        {
            _context = context;
            _ticketService = ticketService;
            _httpContextAccessor = httpContextAccessor;
            _profileService = profileService;
            _ipAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            _user_id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        // GET: /<controller>/
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["error_msg"] = "شناسه دپارتمان ثبت نشده است";
                return RedirectToAction("Index", "Department");
            }
            var department = await _context.Departments.FindAsync(id);
            if (department == null || department.IsDeleted ==true)
            {
                return StatusCode(404);
            }
            ViewData["department_id"] = department.Id;
            ViewData["department_title"] = department.Title;

            var departmentUsersSupport = await _ticketService.GetDeparmentUsersSupport(id);
            ViewData["DefaultUserId"] = new SelectList(departmentUsersSupport, "user_id", "full_name");

            var model = await Task.FromResult(_context.DepartmentServices.Where(x => x.DepartmentId == id && x.IsDeleted ==false).ToList());
            if (model == null)
            {
                return View(new List<DepartmentService>());
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentService model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var department = await _context.Departments.FindAsync(model.DepartmentId);
            if (department == null || department.IsDeleted)
            {
                return NotFound();
            }
            var service = await Task.FromResult(_context.DepartmentServices.FirstOrDefault(x => x.Title == model.Title && x.DepartmentId == model.DepartmentId && x.IsDeleted==true));
            if (service == null)
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreatedAt = DateTime.Now;
                model.CreatedBy = _user_id;
                model.IPAddress = _ipAddress;
                model.EditedBy = _user_id;
                model.EditedAt = DateTime.Now;
                model.IsDeleted = false;
                _context.DepartmentServices.Add(model);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return RedirectToAction("Index", "DepartmentService", new { id = model.DepartmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentService model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var department = await _context.Departments.FindAsync(model.DepartmentId);
            if (department == null || department.IsDeleted)
            {
                return NotFound();
            }
            var service = await _context.DepartmentServices.FindAsync(model.Id);
            if (service != null && service.IsDeleted == false)
            {
                service.IPAddress = _ipAddress;
                service.EditedBy = _user_id;
                service.EditedAt = DateTime.Now;
                service.Title = model.Title;
                service.Description = model.Description;
                service.DefaultUserId = model.DefaultUserId;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return RedirectToAction("Index", "DepartmentService", new { id = model.DepartmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }
            var service = await _context.DepartmentServices.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            service.IsDeleted = true;
            service.DeletedAt = DateTime.Now;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            return RedirectToAction("Index", "DepartmentService", new { id = service.DepartmentId });
        }
    }
}

