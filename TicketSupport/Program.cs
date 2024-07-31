using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSupport.DAL;
using TicketSupport.Services.Catalog;
using TicketSupport.Services.Users;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MainConnection"); builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddTransient<IProfileService, ProfileService>();
builder.Services.AddTransient<ITicketService, TicketService>();
builder.Services.AddControllersWithViews();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Tokens.ChangePhoneNumberTokenProvider = "Phone";
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5000);


    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
    // options.ExpireTimeSpan = TimeSpan.FromHours(1);
    // options.Cookie.MaxAge = TimeSpan.FromDays(30);
    options.Cookie.IsEssential = true;

});

builder.Services.AddDistributedMemoryCache();


// RuntimeCompilation Service Added
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddDataProtection();


builder.WebHost.ConfigureKestrel(t =>
{
    t.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(60));


//builder.Services.AddMvc(options =>
//   options.Filters.Add(new WPVE.Gazelle.BakeryCafe.Web.Filters.Access())
//);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

