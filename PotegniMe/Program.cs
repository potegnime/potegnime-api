// TODO check global usings, add/remove if needed

global using Microsoft.EntityFrameworkCore;
global using PotegniMe.Models;
global using PotegniMe.Models.Main;
global using PotegniMe.DTOs.Auth;
global using PotegniMe.Enums;
global using PotegniMe.DTOs.Error;
global using Microsoft.AspNetCore.Mvc;

using PotegniMe.Services.AuthService;
using System.Text;
using PotegniMe.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PotegniMe.Services.SearchService;
using PotegniMe.Services.FileService;
using PotegniMe.Services.RecommendService;
using PotegniMe.Services.EmailService;
using PotegniMe.Services.AdminService;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

var connectionString = Environment.GetEnvironmentVariable("POTEGNIME_DB_CONN")!;
var apiKey = Environment.GetEnvironmentVariable("POTEGNIME_APP_KEY")!;
var issuer = builder.Configuration["AppSettings:Issuer"];

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudiences = new List<string>
            {
                "https://potegni.me/",
                "https://potegni.me",
                "https://potegni.me/prijava"
            },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey)
            ),
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Database connection
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(
        connectionString,
        o => o.MapEnum<NotificationType>("NotificationType")
    )
);

// Program services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IRecommnedService, RecommendService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS
builder.Services.AddCors(options => options.AddPolicy(name: "NgOrigins",
    policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return true; // non-browser or same-origin requests
            try
            {
                var host = new Uri(origin).Host.ToLowerInvariant();
                if (host == "potegni.me") return true;
                // allow any subdomain of frontend (e.g. ab027615.potegnime-angular.pages.dev)
                if (host.EndsWith(".potegnime-angular.pages.dev")) return true;
                if (host.EndsWith(".pages.dev")) return true;

                // TODO - check if debug version and allow localhost
                // var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                //if (host == "localhost") return true;
            }
            catch
            {
                // invalid origin => deny
            }
            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    }));

var app = builder.Build();

// Middleware
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NgOrigins");
app.UseHttpsRedirection();

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
