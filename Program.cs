using KnzwTech.AspNetCore.ResourceBasedLocalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using PronptModel_ver2.Data;
using PronptModel_ver2.Models;                    


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // �ǋL

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(opt => opt.EnableDefaultErrorMessagesFromResource()); // �ǋL

builder.Services.AddDbContext<PronptContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(nameof(PronptContext))));


//                              ���� �������O��ƈقȂ� IdentityUser �ł͂Ȃ� 
//                              ��   StudentUser �ƂȂ��Ă���_�ɒ���
//                              ��
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

app.UseAuthentication(); // �F�؂�L��������

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

IdentityDataSeeder.SeedData(app);
app.Run();