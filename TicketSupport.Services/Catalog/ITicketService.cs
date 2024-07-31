using System;
using Microsoft.AspNetCore.Http;
using TicketSupport.DAL.Entities.Catalog;

namespace TicketSupport.Services.Catalog
{
	public interface ITicketService
	{
        Task<bool> CreateTicketAsync(Ticket model);
        Task<List<Ticket>> GetUserTicketsAsync(string type);
        Task<List<Ticket>> GetUserTicketsAsync();
        Task<Ticket> GetTicketAsync(string id);
        Task<string> AnswerToTicketAsync(string ticket_id, ReplayTicketModel replay_model);
        Task<string> StateSeenTicketAsync(string ticket_id);
        Task<string> StateResolvedTicketAsync(string ticket_id, bool isResolved);
        Task<string> StateClosedTicketAsync(string ticket_id, bool isClosed);
        Task<string> IsTicketClosedAsync(string ticket_id);
        Task<string> IsTicketResolvedAsync(string ticket_id);
        Task<StatusModel> GetlastTicketClosedAsync(string ticket_id);
        Task<StatusModel> GetlastTicketResolvedAsync(string ticket_id);
        Task<List<UserListModel>> GetTicketUsersSupport(string ticket_id);
        Task<List<UserListModel>> GetDeparmentUsersSupport(string deparment_id);
        Task<bool> AssignUserToDeparmentService(string user_id, string service_id, string deparment_id);
        Task<string> AssignUserToTicketAsync(string user_id, string ticket_id);
        Task<string> CanAssignUserToTicket(string ticket_id);
        Task<List<Ticket>> GetNewTicketListAsync();
        Task<TicketAssignModel> GetLastUserTicketAssignModel(string ticket_id);

    }
}

