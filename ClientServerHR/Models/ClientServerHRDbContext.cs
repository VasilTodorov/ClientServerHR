using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClientServerHR.Models
{
    public class ClientServerHRDbContext: IdentityDbContext<ApplicationUser>
    {
        public ClientServerHRDbContext(DbContextOptions<ClientServerHRDbContext> options): base(options) 
        {
            
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.ApplicationUser)
                .HasForeignKey<Employee>(e => e.ApplicationUserId);
        }
    }
}
