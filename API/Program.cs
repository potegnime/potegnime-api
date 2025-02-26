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

var builder = WebApplication.CreateBuilder(args);
var _config = builder.Configuration;

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
            ValidIssuer = _config["AppSettings:Issuer"],
            ValidAudiences = new List<string>
            {
                "http://localhost:4200/",
                "http://localhost:4200",
                "http://localhost:4200/prijava"

            },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Key").Value!)
            ),
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Database connection
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Program services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IRecommnedService, RecommendService>();

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
