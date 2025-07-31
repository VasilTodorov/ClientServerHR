using ClientServerHR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ClientServerHR.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ClientServerHRDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ClientServerHRDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ClientServerHRDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:ClientServerHRDbContextConnection"]);
});

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ClientServerHRDbContext>();
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ClientServerHRDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;  // <-- Enable unique emails here
})
.AddEntityFrameworkStores<ClientServerHRDbContext>()
.AddDefaultTokenProviders(); ;
//.AddDefaultTokenProviders();
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ClientServerHRDbContext>();
//builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
//    options.SignIn.RequireConfirmedAccount = false;
//})
//.AddRoles<IdentityRole>() // if you're using roles
//.AddEntityFrameworkStores<ClientServerHRDbContext>();

builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else 
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

DbInitializer.Seed(app);
app.Run();
