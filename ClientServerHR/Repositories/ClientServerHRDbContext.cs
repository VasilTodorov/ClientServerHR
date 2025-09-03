using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;

namespace ClientServerHR.Repositories
{
    public class ClientServerHRDbContext: IdentityDbContext<ApplicationUser>
    {
        private readonly IDataProtector _protector;
        public ClientServerHRDbContext(DbContextOptions<ClientServerHRDbContext> options, IDataProtectionProvider provider) : base(options) 
        {
            _protector = provider.CreateProtector("IBANProtector");
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //var ibanConverter = new ValueConverter<string?, string?>(
            //v => v == null ? null : _protector.Protect(v),
            //v => v == null ? null : _protector.Unprotect(v));
            var ibanConverter = new ValueConverter<string, string>(
            v =>  _protector.Protect(v),
            v => _protector.Unprotect(v));

            builder.Entity<Employee>()
            .Property(e => e.IBAN)
            .HasConversion(ibanConverter);

            base.OnModelCreating(builder);

            //builder.Entity<ApplicationUser>()
            //    .HasOne(u => u.Employee)
            //    .WithOne(e => e.ApplicationUser)
            //    .HasForeignKey<Employee>(e => e.ApplicationUserId);

            builder.Entity<Employee>()
               .HasOne(e => e.ApplicationUser)
               .WithOne(u => u.Employee)
               .HasForeignKey<Employee>(e => e.ApplicationUserId)
               .IsRequired(); // Employee requires a user
        }
    }
}
