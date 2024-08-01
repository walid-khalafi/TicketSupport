using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TicketSupport.DAL.Entities.Catalog;
using TicketSupport.Services.Users;
using TicketSupport.DAL;

namespace TicketSupport.Services.Catalog
{
	public class TicketService:ITicketService
	{
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _HttpContextAccessor;

        private string _ipAddress;
        private string _user_id;
        private readonly IProfileService _profileService;
      
        public TicketService(IProfileService profileService, ApplicationDbContext context, IHttpContextAccessor HttpContextAccessor)
        {
            _profileService = profileService;
            _context = context;
            _HttpContextAccessor = HttpContextAccessor;
            _user_id = _HttpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ipAddress = _HttpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        public async Task<string> AnswerToTicketAsync(string ticket_id, ReplayTicketModel replay_model)
        {
            var user = await _profileService.GetUserProfileAsync(_user_id);

            if (replay_model == null || string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            var last_status = await GetlastTicketClosedAsync(ticket.Id);
            if (last_status != null)
            {
                if (last_status.value)
                {
                    return "false";
                }
            }

            List<ReplayTicketModel> replays = new List<ReplayTicketModel>();

            try
            {
                if (!String.IsNullOrWhiteSpace(ticket.Replays))
                {
                    replays.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReplayTicketModel>>(ticket.Replays));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // get ticket department
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (department == null)
            {
                return "false";
            }
           
            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // check user condition to answer to ticket
            if (_user_id == department.DepartmentAdminId || _user_id == ticket.CreatedBy || assignned_to_ticket)
            {

                if (_user_id == ticket.CreatedBy)
                {
                    replay_model.is_me = true;
                }
                else
                {
                    replay_model.is_me = false;
                }
                replay_model.user_id = _user_id;
                replay_model.created_at = DateTime.Now;
                replays.Add(replay_model);

                try
                {
                    ticket.Replays = Newtonsoft.Json.JsonConvert.SerializeObject(replays);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "false";
                }

                await _context.SaveChangesAsync();
                return "true";
            }

            return "false";

        }

        public async Task<bool> AssignUserToDeparmentService(string user_id, string service_id, string deparment_id)
        {
            var service = await Task.FromResult(_context.DepartmentServices.FirstOrDefault(x => x.DepartmentId == deparment_id && x.Id == service_id));
            if (service == null)
            {
                return false;
            }
            service.DefaultUserId = user_id;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> AssignUserToTicketAsync(string user_id, string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (department == null)
            {
                return "false";
            }
           
       
            bool is_drp_admin = false;
           
            if (_user_id == department.DepartmentAdminId)
            {
                is_drp_admin = true;
            }
            if ( !is_drp_admin)
            {
                return "false";
            }

            return "true";
        }

        public async Task<string> CanAssignUserToTicket(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (department == null)
            {
                return "false";
            }
          
     
            bool is_drp_admin = false;
          
            
            if (_user_id == department.DepartmentAdminId)
            {
                is_drp_admin = true;
            }
            if (!is_drp_admin)
            {
                return "false";
            }

            return "true";
        }

        public async Task<bool> CreateTicketAsync(Ticket model)
        {
            if (model == null )
            {
                return false;
            }

            model.CreatedAt = DateTime.Now;
            model.CreatedBy = _user_id;
           

            _context.Tickets.Add(model);
            try
            {
        
                _context.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
      
            return false;

        }

        public async Task<List<UserListModel>> GetDeparmentUsersSupport(string department_id)
        {
            if (string.IsNullOrWhiteSpace(department_id))
            {
                return null;
            }

            var department = await _context.Departments.FindAsync(department_id);
            if (department == null)
            {
                return null;
            }

           
            var drp_users = _context.Profiles.Where(x => x.DepartmentId == department.Id).ToList();
            if (drp_users == null)
            {
                return null;
            }
            List<UserListModel> model = new List<UserListModel>();
            foreach (var item in drp_users)
            {
                var user = await _profileService.GetUserProfileAsync(item.Id);
                if (user != null)
                {
                    model.Add(new UserListModel() { full_name = user.FullName, user_id = user.Id });
                }

            }
            return model;
        }

        public async Task<StatusModel> GetlastTicketClosedAsync(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return null;
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return null;
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return null;
            }
           
            bool assignned_to_ticket = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(ticket.Assign))
                {
                    // check if user assigned to ticket
                    var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                    if (ticketAssign != null)
                    {
                        var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                        if (last_assign.user_id == _user_id)
                        {
                            assignned_to_ticket = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            if (_user_id == drp.DepartmentAdminId ||  ticket.CreatedBy == _user_id || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Closed))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Closed);

                    }
                    if (list != null)
                    {
                        if (list.Count > 0)
                        {
                            var last_status = list.OrderByDescending(x => x.date).First();
                            if (last_status.value)
                            {
                                return last_status;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return null;
        }

        public async Task<StatusModel> GetlastTicketResolvedAsync(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return null;
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return null;
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return null;
            }
           

            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            if (_user_id == drp.DepartmentAdminId  || ticket.CreatedBy == _user_id || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Resolved))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Resolved);

                    }
                    if (list != null)
                    {
                        if (list.Count > 0)
                        {
                            var last_status = list.OrderByDescending(x => x.date).First();
                            if (last_status.value)
                            {
                                return last_status;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return null;
        }

        public async Task<TicketAssignModel> GetLastUserTicketAssignModel(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return null;
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(ticket.Assign))
            {
                return null;
            }
            var users_assigned = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
            if (users_assigned == null)
            {
                return null;
            }
            return users_assigned.LastOrDefault();
        }

        public async Task<List<Ticket>> GetNewTicketListAsync()
        {
            var tickets = await GetUserTicketsAsync("inbox");
            if (tickets == null || tickets.Count == 0)
            {
                return new List<Ticket>();
            }
            tickets = tickets.Where(x => string.IsNullOrWhiteSpace(x.Seen) ||
            (string.IsNullOrWhiteSpace(x.Closed) && string.IsNullOrWhiteSpace(x.Resolved))
            ).ToList();


            if (tickets == null || tickets.Count  == 0)
            {
                return new List<Ticket>();
            }
            return tickets;
        }

        public async Task<Ticket> GetTicketAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return null;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(ticket.Assign))
                {
                    // check if user assigned to ticket
                    var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                    if (ticketAssign != null)
                    {
                        var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                        if (last_assign.user_id == _user_id)
                        {
                            return ticket;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (ticket.CreatedBy == _user_id)
            {
                return ticket;
            }
            if (string.IsNullOrWhiteSpace(ticket.DepartmentId))
            {
                return null;
            }
        
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (department == null)
            {
                return null;
            }

    
            if (department.DepartmentAdminId == _user_id)
            {
                return ticket;
            }

            return null;
        }

        public async Task<List<UserListModel>> GetTicketUsersSupport(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return null;
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return null;
            }
            var department = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (department == null)
            {
                return null;
            }

            var drp_users = _context.Profiles.Where(x => x.DepartmentId == department.Id).ToList();
            if (drp_users == null)
            {
                return null;
            }
            List<UserListModel> model = new List<UserListModel>();
            foreach (var item in drp_users)
            {
                var user = await _profileService.GetUserProfileAsync(item.CreatedBy);
                if (user != null)
                {
                    model.Add(new UserListModel() { full_name = user.FullName, user_id = user.Id });
                }

            }
            return model;
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(string type)
        {
            var user = await _profileService.GetUserProfileAsync(_user_id);
            List<Ticket> list = new List<Ticket>();
            if (user == null)
            {
                return list;
            }
          
            var drp_tickets = await Task.FromResult(_context.Tickets.Where(x => x.DepartmentId == user.DepartmentId).ToList());
            if (drp_tickets !=null && drp_tickets.Count > 0)
            {
                var user_tickets = drp_tickets.Where(x => x.CreatedBy == user.Id).ToList();
                if (user_tickets !=null)
                {
                    switch (type)
                    {
                        case "inbox":
                            // check if user assigned to tickets

                            if (drp_tickets != null)
                            {
                                foreach (var item in drp_tickets)
                                {
                                    if (!string.IsNullOrWhiteSpace(item.Assign))
                                    {
                                        var assign_model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(item.Assign);
                                        var last_assign_model = assign_model.OrderByDescending(x => x.assigned_date).First();
                                        if (last_assign_model.user_id == _user_id)
                                        {
                                            var last_closed_status = await GetlastTicketClosedAsync(item.Id);

                                            if (last_closed_status != null)
                                            {
                                                if (!last_closed_status.value)
                                                {
                                                    list.Add(item);
                                                }

                                                // list.Add(item);
                                            }
                                            else
                                            {
                                                list.Add(item);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "outbox":
                            list.AddRange(await Task.FromResult(_context.Tickets.Where(x => x.CreatedBy == _user_id).ToList()));
                            break;
                        case "closed":
                            foreach (var ticket in user_tickets)
                            {
                                var is_closed = await IsTicketClosedAsync(ticket.Id);
                                if (is_closed == "yes")
                                {
                                    list.Add(ticket);
                                }

                            }
                            break;
                        case "solved":
                            foreach (var ticket in user_tickets)
                            {
                                var is_closed = await IsTicketResolvedAsync(ticket.Id);
                                if (is_closed == "yes")
                                {
                                    list.Add(ticket);
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
       

            return list;
        }

        public async Task<List<Ticket>> GetUserTicketsAsync()
        {
            return await Task.FromResult(_context.Tickets.Where(x => x.CreatedBy == _user_id).ToList());
        }

        public async Task<string> IsTicketClosedAsync(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return "false";
            }
          
            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => (x.assigned_date)).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (_user_id == drp.DepartmentAdminId ||  ticket.CreatedBy == _user_id || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Closed))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Closed);

                    }
                    if (list != null)
                    {
                        if (list.Count > 0)
                        {
                            var last_status = list.OrderByDescending(x => x.date).First();
                            if (last_status.value)
                            {
                                return "yes";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            return "no";
        }

        public async Task<string> IsTicketResolvedAsync(string ticket_id)
        {
           

            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return "false";
            }

            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (_user_id == drp.DepartmentAdminId || ticket.CreatedBy == _user_id || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Resolved))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Resolved);

                    }
                    if (list != null)
                    {
                        if (list.Count > 0)
                        {
                            var last_status = list.OrderByDescending(x => x.date).First();
                            if (last_status.value)
                            {
                                return "yes";
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return "no";
        }

        public async Task<string> StateClosedTicketAsync(string ticket_id, bool isClosed)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return "false";
            }
           
            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            if (_user_id == drp.DepartmentAdminId || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Closed))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Closed);

                    }

                    list.Add(new StatusModel() { user_id = _user_id, date = DateTime.Now, value = isClosed });
                    ticket.Closed = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                    await _context.SaveChangesAsync();
                    return "true";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return "false";
        }

        public async Task<string> StateResolvedTicketAsync(string ticket_id, bool isResolved)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return "false";
            }
           
            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            if (_user_id == drp.DepartmentAdminId || ticket.CreatedBy == _user_id || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Resolved))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Resolved);

                    }

                    list.Add(new StatusModel() { user_id = _user_id, date = DateTime.Now, value = isResolved });
                    ticket.Resolved = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                    await _context.SaveChangesAsync();
                    return "true";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return "false";
        }

        public async Task<string> StateSeenTicketAsync(string ticket_id)
        {

            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return "false";
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return "false";
            }

            // get ticket department
            var drp = await _context.Departments.FindAsync(ticket.DepartmentId);
            if (drp == null)
            {
                return "false";
            }
           
            bool assignned_to_ticket = false;
            try
            {
                // check if user assigned to ticket
                var ticketAssign = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
                if (ticketAssign != null)
                {
                    var last_assign = ticketAssign.OrderByDescending(x => x.assigned_date).First();
                    if (last_assign.user_id == _user_id)
                    {
                        assignned_to_ticket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (_user_id == drp.DepartmentAdminId || assignned_to_ticket)
            {

                List<StatusModel> list = new List<StatusModel>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(ticket.Seen))
                    {
                        list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatusModel>>(ticket.Seen);

                    }

                    list.Add(new StatusModel() { user_id = _user_id, date = DateTime.Now, value = true });
                    ticket.Seen = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                    await _context.SaveChangesAsync();
                    return "true";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            return "false";
        }

        public async Task<ProfileViewModel> GetLastTicketUserSupportAsync(string ticket_id)
        {
            if (string.IsNullOrWhiteSpace(ticket_id))
            {
                return null;
            }
            var ticket = await GetTicketAsync(ticket_id);
            if (ticket == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(ticket.Assign))
            {
                return null;
            }
            var users_assigned = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketAssignModel>>(ticket.Assign);
            if (users_assigned == null)
            {
                return null;
            }
            var profile = await _profileService.GetUserProfileAsync(users_assigned.LastOrDefault().user_id);
            if (profile == null)
            {
                return null;
            }
            return profile;
        }
    }
}

