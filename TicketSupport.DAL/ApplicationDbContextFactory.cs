using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TicketSupport.DAL
{
	public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
		public ApplicationDbContextFactory()
		{
		}

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionbuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            string ConnectionString = "Server=127.0.0.1;Database=TicketSupportDB;Uid=root;Pwd=P@ssw0rd;Convert Zero Datetime=True";

            optionbuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));

            // ensure database created 
            ApplicationDbContext _ctx = new ApplicationDbContext(optionbuilder.Options);

            _ctx.Database.EnsureCreated();

            return _ctx;
        }
    }
}

