using KnzwTech.AspNetCore.ResourceBasedLocalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using PronptModel_ver2.Data;
using PronptModel_ver2.Models;                    


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // 追記

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(opt => opt.EnableDefaultErrorMessagesFromResource()); // 追記

builder.Services.AddDbContext<PronptContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(nameof(PronptContext))));


//                              ┌─ ここが前回と異なり IdentityUser ではなく 
//                              │   StudentUser となっている点に注意
//                              ↓
builder.Services.AddIdentity<StudentUser, IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<PronptContext>();

builder.Services.ConfigureApplicationCookie(opt => {
    opt.LoginPath = "/Accounts/Login";
    opt.LogoutPath = "/Accounts/Logout";
    opt.AccessDeniedPath = "/Accounts/AccessDenied";
});

builder.Services.AddTransient<IdentityDataSeeder>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Home/AccessError/{0}");

app.UseRouting();

app.UseAuthentication(); // 認証を有効化する

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

IdentityDataSeeder.SeedData(app);
app.Run();