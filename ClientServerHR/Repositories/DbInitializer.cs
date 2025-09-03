using ClientServerHR.Repositories;
using Microsoft.AspNetCore.Identity;
using System.IO.Pipelines;

namespace ClientServerHR.Repositories
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();

            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ClientServerHRDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //var provider = services.GetRequiredService<IDataProtectionProvider>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            //var protector = provider.CreateProtector("Employee.IBAN");
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
            if(!context.Departments.Any())
            {
                context.Departments.AddRange(Departments.Select(c => c.Value));
            }
            if (!context.Countries.Any())
            {
                context.Countries.AddRange(Countries.Select(c => c.Value));
            }
            if (!context.Users.Any())
            {
                CreateUserWithEmployee(userManager, context,
                    firstName: "Vasil",
                    lastName: "Todorov",
                    iban: "DE89370400440532013000",
                    email: "vtodorov00@gmail.com",
                    position: "Junior Dev",
                    salary: 1000m,
                    department: "HR",
                    country: "Bulgaria",
                    role: "employee",                    
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Galin",
                    lastName: "Todorov",
                    iban: "FR1420041010050500013M02606",
                    email: "gtodorov00@gmail.com",
                    position: "Senior Dev",
                    salary: 3050m,
                    department: "HR",
                    country: "Bulgaria",
                    role: "manager",
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Toni",
                    lastName: "Apatra",
                    iban: "ES9121000418450200051332",
                    email: "apacha121@gmail.com",
                    position: "Junior Dev",
                    salary: 1000m,
                    department: "HR",
                    country: "Bulgaria",
                    role: "employee",
                    password: "Test123!");

                CreateUserWithEmployee(userManager, context,
                    firstName: "Ivan",
                    lastName: "Grudev",
                    iban: "NL91ABNA0417164300",
                    email: "igrudev77@gmail.com",
                    position: "Senior Dev",
                    salary: 4200m,
                    department: "HR",
                    country: "Bulgaria",
                    role: "admin",
                    password: "Test123!");

                CreateUser(userManager, context,
                    firstName: "Bobi",
                    lastName: "Topkata",
                    email: "bobiTo1@gmail.com",
                    password: "Test123!");

                CreateUser(userManager, context,
                    firstName: "Angel",
                    lastName: "Konstantinov",
                    email: "angelKT2@gmail.com",
                    password: "Test123!");

                CreateUser(userManager, context,
                    firstName: "Morgan",
                    lastName: "Darkstell",
                    email: "legend11@gmail.com",
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
        private static void CreateUser(UserManager<ApplicationUser> userManager, ClientServerHRDbContext context,
            string firstName, string lastName, string email, string password)
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

        }
        private static void CreateUserWithEmployee(UserManager<ApplicationUser> userManager, ClientServerHRDbContext context,            
            string firstName, string lastName,string iban, string email,string position, decimal salary, string department,string country, string role, string password)
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
                IBAN = iban,
                Position = position,
                Salary = salary,
                Department = Departments[department],
                Country = Countries[country],
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

        private static Dictionary<string, Country>? countries;

        public static Dictionary<string, Country> Countries
        {
            get
            {
                if (countries == null)
                {
                    var countryList = new Country[]
                    {
                        new Country { Name = "United States" },
                        new Country { Name = "Germany" },
                        new Country { Name = "India" },
                        new Country { Name = "Bulgaria" }
                    };

                    countries = new Dictionary<string, Country>();

                    foreach (Country country in countryList)
                    {
                        countries.Add(country.Name, country);
                    }
                }

                return countries;
            }
        }
    }

}
