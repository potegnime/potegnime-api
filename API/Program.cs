// TODO check global usings, add/remove if needed

global using Microsoft.EntityFrameworkCore;
global using API.Models;
global using API.Models.Main;
global using API.DTOs.Auth;
global using API.Enums;
global using API.DTOs.Error;
global using Microsoft.AspNetCore.Mvc;

using API.Services.AuthService;
using System.Text;
using API.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using API.Services.SearchService;
using API.Services.FileService;
using API.Services.RecommendService;
using API.Services.EmailService;
using API.Services.AdminService;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

var connectionString = Environment.GetEnvironmentVariable("DBCONN")!;
var apiKey = Environment.GetEnvironmentVariable("APPKEY")!;
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
                "http://localhost:4200/",
                "http://localhost:4200",
                "http://localhost:4200/prijava"

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
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
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
