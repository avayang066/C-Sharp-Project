using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Data;
using MyApp.Services;
using Serilog;

// =====================
// Serilog 設定（Log 檔案）
// =====================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning() // 只記錄 Warning 以上
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
Log.Warning("啟動");

// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Information() // 改成記錄 Information 以上
//     .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
//     .CreateLogger();
// Log.Information("啟動");

// =====================
// 建立 WebApplicationBuilder
// =====================
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var jwtKey = builder.Configuration["jwtKey"];
var jwtIssuer = builder.Configuration["jwtIssuer"];

// 若 jwtKey 為空，自動產生 32 字元亂數金鑰（僅建議開發用）
if (string.IsNullOrWhiteSpace(jwtKey))
{
    // 產生 32 字元亂數（A-Z, a-z, 0-9）
    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    jwtKey = new string(
        Enumerable.Repeat(chars, 32).Select(s => s[random.Next(s.Length)]).ToArray()
    );
    Console.WriteLine($"[警告] jwtKey 未設定，自動產生亂數金鑰：{jwtKey}");
}

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyApp API", Version = "v1" });
    c.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT 授權 (格式: Bearer {token})",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );
    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHostedService<ProductionLogGeneratorService>();
builder.Services.AddScoped<IProductionLogService, ProductionLogService>();
builder.Services.AddScoped<IUserService, UserService>();
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
app.UseCors("AllowAll");
app.UseAuthentication();

// 必須在這裡「啟用」CORS，順序要在 Routing 之後，Authorization 之前
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
