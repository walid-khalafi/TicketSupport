using System;
using TicketSupport.DAL.Entities.Catalog;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSupport.DAL.Entities.User;

namespace TicketSupport.DAL
{
	public class ApplicationDbContext : IdentityDbContext
    {
		
        #region Ctor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        #endregion

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<RoleAccess> RoleAccesses { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentService> DepartmentServices { get; set; }
    }
}

