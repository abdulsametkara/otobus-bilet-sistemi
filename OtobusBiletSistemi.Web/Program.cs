using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Repositories;
using OtobusBiletSistemi.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Session desteği ekle
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<YolcuUser, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    
    options.User.AllowedUserNameCharacters = string.Empty;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IUserValidator<YolcuUser>, CustomUserValidator>();

builder.Services.RemoveAll<IUserValidator<YolcuUser>>();
builder.Services.AddTransient<IUserValidator<YolcuUser>, CustomUserValidator>();

builder.Services.AddScoped<IRepository<Otobus>, Repository<Otobus>>();
builder.Services.AddScoped<IRepository<Guzergah>, Repository<Guzergah>>();
builder.Services.AddScoped<IRepository<Sefer>, Repository<Sefer>>();
builder.Services.AddScoped<IRepository<Koltuk>, Repository<Koltuk>>();
builder.Services.AddScoped<IRepository<Bilet>, Repository<Bilet>>();
builder.Services.AddScoped<IRepository<Odeme>, Repository<Odeme>>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Admin kullanıcısını oluştur
using (var scope = app.Services.CreateScope())
{
    AdminSeeder.SeedAdminUser(scope.ServiceProvider).Wait();
}

app.Run();