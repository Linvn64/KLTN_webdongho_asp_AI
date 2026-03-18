// File: SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Models;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<ShopDongHoDbContext>();

        string[] roles = { "Admin", "NhanVien", "KhachHang" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        string adminEmail = "admin@gmail.com";
        string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");

                var adminProfile = new NguoiDung
                {
                    UserId = user.Id,
                    HoTen = "Quản trị viên",
                    Email = adminEmail,
                    NgayTao = DateTime.Now
                };
                dbContext.NguoiDungs.Add(adminProfile);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}