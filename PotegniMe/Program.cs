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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "potegnime API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste JWT here (without Bearer prefix)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IRecommendService, RecommendService>();
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
                var uri = new Uri(origin);
                var host = uri.Host.ToLowerInvariant();
                if (uri.Scheme == "https" && (host == "potegni.me" || host == "www.potegni.me" || host.EndsWith(".potegni.me"))) return true;
    
                // allow any subdomain of frontend (e.g. ab027615.potegnime-angular.pages.dev)
                if (host.EndsWith(".potegnime-angular.pages.dev")) return true;
                if (builder.Environment.IsDevelopment() && host == "localhost") return true;
            }
            catch (Exception ex)
            {
                // invalid origin => deny
                Console.WriteLine($"EXCEPTION_CORS:\nOrigin:{origin}\nException:{ex.Message}"); // TODO - observability
                return false;
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
