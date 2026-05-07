using System.Text;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// 建立 WebApplicationBuilder，並配置服務和中介軟體
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 添加控制器服務，允許使用控制器來處理 HTTP 請求
builder.Services.AddControllers();

// 註冊 AppDbContext，並使用 SQLite 作為資料庫
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 允許跨來源請求，特別是來自 Angular 前端的請求
builder.Services.AddCors();

// 註冊 TokenService 為 ITokenService 的實現
builder.Services.AddScoped<ITokenService, TokenService>();

// 配置 JWT 認證
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("TokenKey is not configured");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey))
        };
    });

// 建構 WebApplication 實例
WebApplication app = builder.Build();

// 配置 HTTP 請求管道
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"));

// 啟用認證和授權中介軟體
app.UseAuthentication();
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 啟動應用程式
app.Run();
