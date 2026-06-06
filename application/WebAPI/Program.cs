using Common.Service.Dto;
using Common.Service.Interface;
using Common.Settings;
using Database;
using DotNetEnv;
using Logging.Service.Dto;
using Logging.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Security.Middleware;
using Security.Service.Dto;
using Security.Service.Interface;
using System.Text;
using Serilog;
using Serilog.Formatting.Compact;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ADD LOGGING
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// SERVICES
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient(typeof(IAppLoggerService<>), typeof(AppLoggerService<>));

builder.Services.AddScoped<ISeederService, SeederService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientIpService, ClientIpService>();

// OPTIONS
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.Configure<JwtSettings>(jwtSettings);

// AUTHENTICATION
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = jwtSettings.Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)
            )
        };
    });

// AUTHORIZATION
builder.Services.AddAuthorization();

// BUILD APP
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // REGISTER LOGGING
    var logger = scope.ServiceProvider.GetRequiredService<IAppLoggerService<Program>>();

    var env = app.Configuration["ASPNETCORE_ENVIRONMENT"];
    var urls = app.Configuration["ASPNETCORE_URLS"];
    var port = urls?.Split(':').LastOrDefault();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        logger.Info($"Application started on {env}. Listening on {port}");
    });

    app.Lifetime.ApplicationStopped.Register(() =>
    {
        logger.Info($"Application stopped on {env}");
    });

    // MIGRATIONS
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();

    // SEEDERS
    var seeder = scope.ServiceProvider.GetRequiredService<ISeederService>();

    await seeder.PopulateDatabaseAsync();
}

// MIDDLEWARE
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ClientIpMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}