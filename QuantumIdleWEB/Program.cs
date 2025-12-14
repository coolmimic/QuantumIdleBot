using Microsoft.AspNetCore.Authentication.JwtBearer; // <--- ��������
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <--- ��������
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services.Tron;
using System.Text; // <--- ��������

var builder = WebApplication.CreateBuilder(args);

// 关闭 WTelegram 日志
WTelegram.Helpers.Log = (level, msg) => { };

// 添加 CORS 支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow any origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        // ����Ĭ�ϵġ���֤ʧ�ܡ���Ӧ�߼�
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = string.Join("; ", context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            var result = new
            {
                success = false,
                message = errors,
                data = (object)null
            };

            return new BadRequestObjectResult(result);
        };
    });

// ================== 1. ���� JWT ��֤����ע�� (��ʼ) ==================
// ��ȡ�����ļ��е� Key
var jwtSettings = builder.Configuration.GetSection("Jwt");
// ����Ӹ��жϣ���ֹ�����ļ���д����
var keyStr = jwtSettings["Key"] ?? "ThisIsADefaultKeyButYouShouldChangeItToSomethingLonger";
var key = Encoding.UTF8.GetBytes(keyStr);

builder.Services.AddAuthentication(options =>
{
    // ����Ĭ����֤ģʽΪ Bearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // ��������ʱ���ʱ���
    };
});
// ================== 1. ���� JWT ��֤����ע�� (����) ==================


builder.Services.AddHostedService<TronMonitorService>();

// ��ȡ�����ַ��� & ע�� DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<QuantumIdleWEB.Services.BotUpdateHandler>();

// 注册 Telegram 客户端服务（供移动版使用）
builder.Services.AddSingleton<QuantumIdleWEB.Services.TelegramClientService>();

// 注册游戏上下文服务
builder.Services.AddSingleton<QuantumIdleWEB.Services.GameContextService>();

// 注册投注和结算服务
builder.Services.AddScoped<QuantumIdleWEB.Services.BettingService>();
builder.Services.AddScoped<QuantumIdleWEB.Services.SettlementService>();

// ע�� TelegramBotClient
builder.Services.AddSingleton<Telegram.Bot.ITelegramBotClient>(provider =>
{
    return new Telegram.Bot.TelegramBotClient("8555756240:AAE-1-zAmlLGSDjGjgWcgKjRWNEsgQm9ZhA");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 本地测试环境不使用 HTTPS 重定向
// app.UseHttpsRedirection();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".exe"] = "application/octet-stream";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".json"] = "application/json";

// ������̬�ļ�����
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseRouting();

// 启用 CORS（必须在 UseAuthentication 之前）
app.UseCors("AllowMobileApp");

// ================== 2. ������֤�м�� (������ UseAuthorization ֮ǰ) ==================
app.UseAuthentication(); // <--- ��һ���ǡ���Ʊ������� Token �Ƿ�Ϸ���
// ====================================================================================

app.UseAuthorization();  // <--- ��һ���ǡ���Ȩ�ޡ�������û��ܲ��ܷ��ʣ�

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// 自动应用数据库迁移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // 这会自动创建数据库并应用所有迁移
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();