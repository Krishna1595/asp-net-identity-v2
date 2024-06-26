using System;
using IdentityNetCore.Data;
using IdentityNetCore.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
    _ = app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();
app.UseAuthentication();

app.MapDefaultControllerRoute();
app.MapRazorPages();

var connString = app.Configuration["ConnectionStrings:Default"];
var smtp = app.Configuration.GetSection("Smtp");

ConfigureServices(builder.Services);

app.Run();


void ConfigureServices(IServiceCollection services)
{
    _ = services.AddDbContext<ApplicationDBContext>(o => o.UseSqlServer(connString));
    _ = services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>()
        .AddDefaultTokenProviders();

    _ = services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequiredLength = 3;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

        options.SignIn.RequireConfirmedEmail = false;
    });

    _ = services.ConfigureApplicationCookie(option =>
    {
        option.LoginPath = "/Identity/Signin";
        option.AccessDeniedPath = "/Identity/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromHours(10);
    });

    _ = services.Configure<SmtpOptions>(smtp);

    _ = services.AddSingleton<IEmailSender, SmtpEmailSender>();
    _ = services.AddAuthorization(option =>
    {
        option.AddPolicy("MemberDep", p => { _ = p.RequireClaim("Department", "Tech").RequireRole("Member"); });

        option.AddPolicy("AdminDep", p => { _ = p.RequireClaim("Department", "Tech").RequireRole("Admin"); });
    });
    _ = services.AddControllersWithViews();
}
