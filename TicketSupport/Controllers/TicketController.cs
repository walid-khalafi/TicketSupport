﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using TicketSupport.DAL;
using TicketSupport.DAL.Entities.Catalog;
using TicketSupport.DAL.Enums;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;
using TicketSupport.WEB.Models.TicketViewModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.WEB.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private readonly ITicketService _ticketService;
        private string _user_id;
        private string _ipAddress;


        public TicketController(ApplicationDbContext context,ITicketService ticketService, IHttpContextAccessor httpContextAccessor, IProfileService profileService)
        {
            _context = context;
            _ticketService = ticketService;
            _httpContextAccessor = httpContextAccessor;
            _profileService = profileService;
            _ipAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            _user_id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        // GET: /<controller>/
        public IActionResult Index(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                type = "outbox";
            }
            ViewData["ticket_type"] = type;

            return View();
        }

        [HttpPost("[controller]/[action]/{type?}")]
        public async Task<JsonResult> LoadAjaxTickets(string type)
        {
            // server side parameters
            int start = Convert.ToInt32(Request.Form["start"]);
            int length = Convert.ToInt32(Request.Form["length"]);
            string searchValue = Request.Form["search[value]"].ToString();
            string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].ToString() + "][name]"];
            string sortDirection = Request.Form["order[0][dir]"].ToString();

            if (string.IsNullOrWhiteSpace(type))
            {
                type = "outbox";
            }
            List<Ticket> model = new List<Ticket>();
            List<IndexViewModel> list = new List<IndexViewModel>();
            try
            {
                model = await _ticketService.GetUserTicketsAsync(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return Json(new { data = list, draw = Request.Form["draw"].ToString(), recordsTotal = 0, recordsFiltered = 0 });
            }
            
           
            if (model == null)
            {
                return Json(new { data = list, draw = Request.Form["draw"].ToString(), recordsTotal = 0, recordsFiltered = 0 });
            }


            int recordsTotal = model.Count;
            foreach (var item in model)
            {
                var item_user = await _profileService.GetUserProfileAsync(@item.CreatedBy);
                var user_assigned = await _ticketService.GetLastTicketUserSupportAsync(item.Id);
                var is_have_attachment = false;
                var service = await _context.DepartmentServices.FindAsync(item.DepartmentServiceId);
                StatusModel last_closed_status = await _ticketService.GetlastTicketClosedAsync(item.Id);
                StatusModel last_resolved_status = await _ticketService.GetlastTicketResolvedAsync(item.Id);
                // ticket status
                string closed_status = string.Empty;
                string resolved_status = string.Empty;
                string priority_css_class = "bg-info";
                if (last_closed_status != null)
                {
                    if (last_closed_status.value)
                    {
                        closed_status = "بسته شده";
                    }
                }
                if (last_resolved_status != null)
                {
                    if (last_resolved_status.value)
                    {
                        resolved_status = "برطرف شده";
                    }
                }
                // ticket priority
                string ticket_priority = "";
                switch (item.TicketPriority)
                {
                    case TicketPriority.very_high:
                        ticket_priority = "خیلی زیاد";
                        priority_css_class = "bg-danger";
                        break;
                    case TicketPriority.high:
                        ticket_priority = "زیاد";
                        priority_css_class = "bg-danger";
                        break;
                    case TicketPriority.medium:
                        ticket_priority = "متوسط";
                        priority_css_class = "bg-warning";
                        break;
                    case TicketPriority.low:
                        ticket_priority = "کم";
                        priority_css_class = "bg-primary";
                        break;
                    case TicketPriority.very_low:
                        ticket_priority = "خیلی کم";
                        priority_css_class = "bg-info";
                        break;
                    default:
                        ticket_priority = "تعیین نشده";
                        priority_css_class = "bg-info";
                        break;
                }

                list.Add(new IndexViewModel()
                {
                    ticket_id = item.Id,
                    fullname = item_user.FullName,
                    avatar = (!string.IsNullOrWhiteSpace(item_user.Avatar) ? item_user.Avatar : null),
                    created_at_en = item.CreatedAt,
                    created_at = PersianDate.Standard.ConvertDate.ToFa(item.CreatedAt, "F"),
                    have_attachment = is_have_attachment,
                    subject = item.Subject,
                    user_assigned = (user_assigned != null ? user_assigned.FullName : "در انتظار کارشناس"),
                    service_type = (service != null ? service.Title : "تعیین نشده"),
                    last_closed_status = closed_status,
                    last_resolved_status = resolved_status,
                    priority = ticket_priority,
                    priority_css_class = priority_css_class

                });

            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                list = list.Where(x =>
                x.fullname.Contains(searchValue) ||
                x.subject.Contains(searchValue) ||
                x.priority.Contains(searchValue) ||
                x.service_type.Contains(searchValue) ||
                x.user_assigned.Contains(searchValue) ||
                x.last_closed_status.Contains(searchValue) ||
                x.last_resolved_status.Contains(searchValue) ||
                x.created_at.Contains(searchValue)
                ).ToList();
            }

            // sorting
            if (sortDirection == "asc")
            {
                switch (sortColumnName)
                {
                    case "fullname":
                        list = list.OrderBy(x => x.fullname).ToList();
                        break;
                    case "subject":
                        list = list.OrderBy(x => x.subject).ToList();
                        break;
                    case "priority":
                        list = list.OrderBy(x => x.priority).ToList();
                        break;
                    case "service_type":
                        list = list.OrderBy(x => x.service_type).ToList();
                        break;
                    case "user_assigned":
                        list = list.OrderBy(x => x.user_assigned).ToList();
                        break;
                    case "last_resolved_status":
                        list = list.OrderBy(x => x.last_resolved_status).ToList();
                        break;
                    case "last_closed_status":
                        list = list.OrderBy(x => x.last_closed_status).ToList();
                        break;
                    case "created_at":
                        list = list.OrderBy(x => x.created_at_en).ToList();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (sortColumnName)
                {
                    case "fullname":
                        list = list.OrderByDescending(x => x.fullname).ToList();
                        break;
                    case "subject":
                        list = list.OrderByDescending(x => x.subject).ToList();
                        break;
                    case "priority":
                        list = list.OrderByDescending(x => x.priority).ToList();
                        break;
                    case "service_type":
                        list = list.OrderByDescending(x => x.service_type).ToList();
                        break;
                    case "user_assigned":
                        list = list.OrderByDescending(x => x.user_assigned).ToList();
                        break;
                    case "last_resolved_status":
                        list = list.OrderByDescending(x => x.last_resolved_status).ToList();
                        break;
                    case "last_closed_status":
                        list = list.OrderByDescending(x => x.last_closed_status).ToList();
                        break;
                    case "created_at":
                        list = list.OrderByDescending(x => x.created_at_en).ToList();
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
        public async Task<ActionResult> Compose()
        {

          var departments = await Task.FromResult(_context.Departments.Where(x=>x.IsDeleted==false).ToList());

            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Title");

            return View(new Ticket() { Id = Guid.NewGuid().ToString() });
        }
        [HttpPost]
        public async Task<JsonResult> Compose(Ticket model)
        {
            if (String.IsNullOrWhiteSpace(model.Subject) || String.IsNullOrWhiteSpace(model.Body))
            {
                return Json("false");
            }
            bool TicketCreated = false;
            try
            {
                TicketCreated = await _ticketService.CreateTicketAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
         

            if (!TicketCreated)
            {
                return Json("false");
            }

            // asgin default user to ticket
            var selected_service = _context.DepartmentServices.FirstOrDefault(x => x.Id == model.Id);
            if (selected_service != null)
            {
                if (!string.IsNullOrWhiteSpace(selected_service.DefaultUserId))
                {
                    var is_user_asigned = await _ticketService.AssignUserToTicketAsync(selected_service.DefaultUserId, model.Id);
                }
            }

            return Json("true");
            
          
        }
        [HttpPost]
        public async Task<string> GetServiceListAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "false";
            }
            List<DepartmentService> services = await Task.FromResult(_context.DepartmentServices.Where(x=>x.DepartmentId == id).ToList());
            return Newtonsoft.Json.JsonConvert.SerializeObject(services).ToString();

        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {


            if (string.IsNullOrWhiteSpace(id))
            {
                return StatusCode(404);
            }

            DetailsViewModel model = new DetailsViewModel()
            {
                Attachments = new List<AttachmentFileInfo>(),
                Replays = new List<ReplayTicketModel>(),
                TicketUsersSupport = new List<UserListModel>(),
                AssignedUser = new UserListModel(),
            };

            var ticket = await _ticketService.GetTicketAsync(id);
            if (ticket == null)
            {
                return StatusCode(404);
            }
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            var departmentService = await _context.DepartmentServices.FindAsync(ticket.DepartmentServiceId);
            if (department!=null)
            {
                model.Department = department;
            }
            if (departmentService !=null)
            {
                model.DepartmentService = departmentService;
            }
            model.Id = ticket.Id;
            model.Subject = ticket.Subject;
            model.Body = ticket.Body;
            model.IsClosed = await _ticketService.IsTicketClosedAsync(ticket.Id);
            model.IsResolved = await _ticketService.IsTicketResolvedAsync(ticket.Id);
            model.CanAssignUserToTicket = await _ticketService.CanAssignUserToTicket(ticket.Id);
            model.TicketUsersSupport.AddRange(await _ticketService.GetTicketUsersSupport(id));
            model.CreatedAt = ticket.CreatedAt;
            model.CreatedBy = await _profileService.GetUserProfileAsync(ticket.CreatedBy);
            await _ticketService.StateSeenTicketAsync(id);

            try
            {
                if (ticket.Assign != null)
                {
                    var assign_users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                    if (assign_users == null)
                    {
                        model.AssignedUser.user_id = Guid.Empty.ToString();
                        model.AssignedUser.full_name = "تیکت هنوز به کاربری اختصاص داده نشده است";
                    }
                    else
                    {
                        var last_assigned = assign_users.OrderByDescending(x => x.assigned_date).First();
                        var selected_user = await _profileService.GetUserProfileAsync(last_assigned.user_id);
                        if (selected_user != null)
                        {
                            model.AssignedUser.user_id = selected_user.Id;
                            model.AssignedUser.full_name = selected_user.FullName;
                            model.AssignedUser.avatar = selected_user.Avatar;
                            model.AssignedUser.date = last_assigned.assigned_date;
                        }
                    }
                }
                else
                {
                    model.AssignedUser.user_id = Guid.Empty.ToString();
                    model.AssignedUser.full_name = "تیکت هنوز به کاربری اختصاص داده نشده است";
                }

                if (!string.IsNullOrWhiteSpace(ticket.Replays))
                {
                    model.Replays = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReplayTicketModel>>(ticket.Replays);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> AssignUserToTicketAsync(string user_id, string ticket_id)
        {
            var result = await _ticketService.AssignUserToTicketAsync(user_id, ticket_id);

            // await _hubContext.Clients.All.SendAsync("displayNotification", "");

           // var email_result = await SendEmailNewTicketAsync(user_id, "شما یک تیکت جدید دارید", Url.Action("Details", "Ticket", new { id = ticket_id }, Request.Scheme));
            //await _hubContext.Clients.User(user_id).SendAsync("displayNotification", "");
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> CloseTicket(string ticket_id, string isClosed)
        {
            return Json(await _ticketService.StateClosedTicketAsync(ticket_id, bool.Parse(isClosed)));
        }

        [HttpPost]
        public async Task<JsonResult> ResolveTicket(string ticket_id, bool isResolved)
        {
            return Json(await _ticketService.StateResolvedTicketAsync(ticket_id, isResolved));
        }

        [HttpPost]
        public async Task<JsonResult> SendAnswer(ReplayTicketModel model, string ticket_id)
        {
            var ticket = await _ticketService.GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return Json("false");
            }

            var result = await _ticketService.AnswerToTicketAsync(ticket_id, model);


            return Json(result);
        }

    }
}

