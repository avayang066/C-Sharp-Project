using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Services;
using Serilog;

// =====================
// Serilog 設定（Log 檔案）
// =====================
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// =====================
// 建立 WebApplicationBuilder
// =====================
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// =====================
// DI 服務註冊
// =====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        "Server=localhost\\SQLEXPRESS;Database=MyAppDB;Trusted_Connection=True;TrustServerCertificate=True;"
    )
);
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ProductionLogGeneratorService>();
builder.Services.AddScoped<ProductionLogService>();
builder.Services.AddScoped<IMachineService, MachineService>();

// --- 新增：註冊 CORS 服務 ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin() // 允許任何來源（如 localhost:3000）
                .AllowAnyMethod() // 允許任何方法（PUT, POST, GET, DELETE）
                .AllowAnyHeader(); // 允許任何 Header
        }
    );
});

// =====================
// 建立 WebApplication 實例
// =====================
var app = builder.Build();

// =====================
// 啟動時檢查資料庫連線
// =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Database.CanConnect())
    {
        throw new Exception("資料庫連線失敗！請檢查連線字串與資料庫檔案。");
    }
}

// =====================
// HTTP 請求管線設定
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll"); // 必須在這裡「啟用」CORS，順序要在 Routing 之後，Authorization 之前 
app.UseAuthorization();

// =====================
// 路由設定
// =====================
app.MapControllers(); // API Controller 通常需要這一行來確保 [ApiController] 屬性能被正確映射
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// =====================
// 啟動應用程式
// =====================
app.Run();
