using Microsoft.AspNetCore.Identity;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<YolcuUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            
            var adminEmail = "admin@otobus.com";
            var adminPassword = "Admin123!";
            
            // Admin rolü mevcut mu kontrol et ve oluştur
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                var adminRole = new IdentityRole<int>("Admin");
                await roleManager.CreateAsync(adminRole);
                Console.WriteLine("Admin rolü oluşturuldu");
            }
            
            // Admin kullanıcısı mevcut mu kontrol et
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                // Admin kullanıcısı yok, oluştur
                adminUser = new YolcuUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    TCNo = "12345678901",
                    Ad = "Admin",
                    Soyad = "User",
                    TelNo = "05551234567",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                
                if (result.Succeeded)
                {
                    // Admin rolünü kullanıcıya ata
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"Admin kullanıcısı oluşturuldu ve Admin rolü atandı: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Admin kullanıcısı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Admin mevcut, şifresini güncelle
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var result = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                
                if (result.Succeeded)
                {
                    Console.WriteLine($"Admin şifresi güncellendi: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Admin şifresi güncellenemedi: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                
                // Admin rolünü kontrol et ve gerekirse ata
                var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin rolü kullanıcıya atandı");
                }
            }
        }
    }
} 
 