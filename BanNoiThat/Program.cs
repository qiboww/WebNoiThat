using BanNoiThat.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BanNoiThat.Repositories;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register Identity services using ApplicationUser and IdentityRole
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Tắt yêu cầu xác nhận email
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Đăng ký dịch vụ gửi Email giả lập cho các trang Identity (Đăng ký, Quên mật khẩu...)
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, DummyEmailSender>();

// Register Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();


var app = builder.Build();

// Khởi tạo dữ liệu tài khoản Admin
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
//        await DbInitializer.SeedDataAsync(userManager, roleManager);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "Có lỗi xảy ra khi seed database.");
//    }
//}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.Run();