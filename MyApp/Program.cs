using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        "Server=localhost\\SQLEXPRESS;Database=MyAppDB;Trusted_Connection=True;TrustServerCertificate=True;"
    )
);
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ProductionLogGeneratorService>();
builder.Services.AddScoped<ProductionLogService>();
builder.Services.AddScoped<IMachineService, MachineService>();

var app = builder.Build();

// 啟動時檢查資料庫連線，失敗則拋出例外
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Database.CanConnect())
    {
        throw new Exception("資料庫連線失敗！請檢查連線字串與資料庫檔案。");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
