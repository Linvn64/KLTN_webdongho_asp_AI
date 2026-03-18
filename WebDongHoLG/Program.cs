using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShopDongHoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()                        
.AddEntityFrameworkStores<ShopDongHoDbContext>() 
.AddDefaultTokenProviders();                    

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true; // GDPR‑style
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
});

//var googleSection = builder.Configuration.GetSection("Authentication:Google");
//builder.Services.AddAuthentication()
//    .AddGoogle(options =>
//    {
//        options.ClientId = googleSection["ClientId"]!;
//        options.ClientSecret = googleSection["ClientSecret"]!;
//        options.CallbackPath = "/signin-google";
//    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();