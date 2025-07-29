using Microsoft.AspNetCore.Identity;
using System.IO.Pipelines;

namespace ClientServerHR.Models
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ClientServerHRDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // 1. Seed roles
            string[] roles = { "employee", "manager", "admin" };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    var result = roleManager.CreateAsync(new IdentityRole(role)).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {role}");
                    }
                }
            }

            // 2. Seed users + employees (only if none exist)
            if (!context.Users.Any())
            {
                CreateUserWithEmployee(userManager, context,
                    firstName: "Vasil",
                    lastName: "Todorov",
                    email: "vtodorov00@gmail.com",
                    position: "Junior Dev",
                    salary: 1000m,
                    department: "HR",
                    role: "employee",
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Galin",
                    lastName: "Todorov",
                    email: "gtodorov00@gmail.com",
                    position: "Senior Dev",
                    salary: 3050m,
                    department: "HR",
                    role: "manager",
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Toni",
                    lastName: "Apatra",
                    email: "apacha121@gmail.com",
                    position: "Junior Dev",
                    salary: 1000m,
                    department: "HR",
                    role: "employee",
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Ivan",
                    lastName: "Grudev",
                    email: "igrudev77@gmail.com",
                    position: "Senior Dev",
                    salary: 4200m,
                    department: "HR",
                    role: "admin",
                    password: "Test123!");
            }

            context.SaveChanges();
        }
        public static void DeleteAllData(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ClientServerHRDbContext>();
            // 0.Deletion
            context.Employees.RemoveRange(context.Employees);
            context.Users.RemoveRange(context.Users); // only if you want to clear Identity users
            context.SaveChanges();
        }
        
        private static void CreateUserWithEmployee(UserManager<ApplicationUser> userManager, ClientServerHRDbContext context,
            string firstName, string lastName, string email,string position, decimal salary, string department, string role, string password)
        {
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                Email = email
            };

            var result = userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            userManager.AddToRoleAsync(user, role).Wait();

            var employee = new Employee
            {
                //FirstName = firstName,
                //LastName = lastName,
                //Email = email,
                Position = position,
                Salary = salary,
                Department = Departments[department],
                ApplicationUserId = user.Id
            };

            context.Employees.Add(employee);
        }

        private static Dictionary<string, Department>? departments;

        public static Dictionary<string, Department> Departments
        {
            get
            {
                if (departments == null)
                {
                    var departmentList = new Department[]
                    {
                        new Department { Name = "HR" },
                        new Department { Name = "IT" },
                        new Department { Name = "Finance" },
                        new Department { Name = "Marketing" }
                    };

                    departments = new Dictionary<string, Department>();

                    foreach (Department department in departmentList)
                    {
                        departments.Add(department.Name, department);
                    }
                }

                return departments;
            }
        }
    }

}
